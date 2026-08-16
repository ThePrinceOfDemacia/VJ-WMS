namespace VjWms.Desktop.UI.Services.Scanner;

/// <summary>
/// Abstraction for dual-mode barcode/QR scanning:
/// Mode 1 (Screen QR): Load image file → decode with ZXing.Net
/// Mode 2 (HID USB): Listen for keyboard input from portable barcode scanner
/// </summary>
public interface IScannerService
{
    /// <summary>
    /// Mode 1: Open file dialog, pick an image, decode QR/barcode from it.
    /// Returns decoded text or null if cancelled/failed.
    /// </summary>
    Task<string?> ScanFromImageAsync();

    /// <summary>
    /// Mode 2: Start listening for HID USB scanner keyboard input.
    /// When a barcode is scanned, the callback is invoked with the decoded text.
    /// </summary>
    void StartHidListening(Action<string> onScanned);

    /// <summary>
    /// Stop HID USB scanner listening mode.
    /// </summary>
    void StopHidListening();

    /// <summary>
    /// Whether HID listening mode is currently active.
    /// </summary>
    bool IsHidListening { get; }
}
