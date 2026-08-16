using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using ZXing;
using ZXing.Windows.Compatibility;

namespace VjWms.Desktop.UI.Services.Scanner;

/// <summary>
/// Dual-mode scanner implementation:
/// Mode 1: Screen QR — pick image file → decode with ZXing.Net
/// Mode 2: HID USB — listen for rapid keyboard input from USB barcode scanner
/// 
/// USB barcode scanners act as keyboard HID devices: they type characters rapidly
/// and press Enter at the end. We detect this by buffering keystrokes that arrive
/// within 50ms of each other (too fast for human typing).
/// </summary>
public class ScannerService : IScannerService
{
    private Action<string>? _onScannedCallback;
    private readonly StringBuilder _hidBuffer = new();
    private readonly DispatcherTimer _hidTimer;
    private bool _isHidListening;

    // Threshold: if characters arrive faster than this, it's a scanner, not a human
    private const int ScannerInputThresholdMs = 80;

    public ScannerService()
    {
        _hidTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(ScannerInputThresholdMs)
        };
        _hidTimer.Tick += OnHidTimerTick;
    }

    public bool IsHidListening => _isHidListening;

    // ================================================================
    // MODE 1: Screen QR — Pick image and decode
    // ================================================================

    public Task<string?> ScanFromImageAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Chọn ảnh QR/Barcode / Select QR/Barcode image",
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files (*.*)|*.*",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
            return Task.FromResult<string?>(null);

        try
        {
            var reader = new BarcodeReader();
            // Support multiple formats
            reader.Options.PossibleFormats = new[]
            {
                BarcodeFormat.QR_CODE,
                BarcodeFormat.CODE_128,
                BarcodeFormat.CODE_39,
                BarcodeFormat.EAN_13,
                BarcodeFormat.EAN_8,
                BarcodeFormat.UPC_A,
                BarcodeFormat.DATA_MATRIX
            };

            using var bitmap = new Bitmap(dialog.FileName);
            var result = reader.Decode(bitmap);
            return Task.FromResult(result?.Text);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ScannerService] Image decode failed: {ex.Message}");
            return Task.FromResult<string?>(null);
        }
    }

    // ================================================================
    // MODE 2: HID USB Scanner — listen for keyboard input
    // ================================================================

    public void StartHidListening(Action<string> onScanned)
    {
        _onScannedCallback = onScanned;
        _isHidListening = true;

        // Hook into the main window's PreviewKeyDown
        if (System.Windows.Application.Current.MainWindow is Window mainWindow)
        {
            mainWindow.PreviewKeyDown += OnPreviewKeyDown;
        }
    }

    public void StopHidListening()
    {
        _isHidListening = false;
        _onScannedCallback = null;

        if (System.Windows.Application.Current.MainWindow is Window mainWindow)
        {
            mainWindow.PreviewKeyDown -= OnPreviewKeyDown;
        }

        _hidBuffer.Clear();
        _hidTimer.Stop();
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!_isHidListening) return;

        // Enter = end of barcode scan
        if (e.Key == Key.Enter && _hidBuffer.Length > 0)
        {
            _hidTimer.Stop();
            var scannedText = _hidBuffer.ToString();
            _hidBuffer.Clear();

            // Only fire callback if the scanned text looks like a real barcode
            // (at least 3 characters — eliminates accidental Enter presses)
            if (scannedText.Length >= 3)
            {
                _onScannedCallback?.Invoke(scannedText);
            }

            e.Handled = true;
            return;
        }

        // Convert key to character
        var ch = KeyToChar(e.Key);
        if (ch != null)
        {
            // Reset timer on each keystroke
            _hidTimer.Stop();
            _hidBuffer.Append(ch.Value);
            _hidTimer.Start();

            // Don't mark as handled — let the character still reach the focused TextBox if any
        }
    }

    private void OnHidTimerTick(object? sender, EventArgs e)
    {
        // Timer expired without new input → this was probably human typing, discard buffer
        _hidTimer.Stop();
        _hidBuffer.Clear();
    }

    /// <summary>
    /// Convert a Key enum to its character representation.
    /// Handles alphanumeric keys and common barcode characters.
    /// </summary>
    private static char? KeyToChar(Key key)
    {
        // Letters
        if (key >= Key.A && key <= Key.Z)
            return (char)('A' + (key - Key.A));

        // Numbers (top row)
        if (key >= Key.D0 && key <= Key.D9)
            return (char)('0' + (key - Key.D0));

        // Numpad
        if (key >= Key.NumPad0 && key <= Key.NumPad9)
            return (char)('0' + (key - Key.NumPad0));

        // Common barcode separator characters
        return key switch
        {
            Key.OemMinus => '-',
            Key.OemPeriod => '.',
            Key.OemPipe => '|',
            Key.Divide => '/',
            Key.Multiply => '*',
            Key.Space => ' ',
            _ => null
        };
    }
}
