using System.Windows;
using System.Windows.Media;

namespace VjWms.Desktop.UI.Views;

public partial class ShellWindow : Window
{
    private readonly string _username;
    private readonly bool _isOnline;
    private bool _isLoggingOut = false;

    public ShellWindow(string username, bool isOnline)
    {
        InitializeComponent();
        _username = username;
        _isOnline = isOnline;

        SetupUI();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (!_isLoggingOut)
        {
            Environment.Exit(0);
        }
    }

    private void SetupUI()
    {
        // User info
        UserNameText.Text = _username;
        UserInitial.Text = _username.Length > 0 ? _username[0].ToString().ToUpper() : "?";

        // Status bar
        if (_isOnline)
        {
            StatusBarDot.Fill = (SolidColorBrush)FindResource("OnlineBrush");
            StatusBarText.Text = FindResource("StatusBarOnline") as string;
        }
        else
        {
            StatusBarDot.Fill = (SolidColorBrush)FindResource("OfflineBrush");
            StatusBarText.Text = FindResource("StatusBarOffline") as string;
        }
    }

    private void OnNavClick(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is string page)
        {
            CurrentPageText.Text = page;
            // TODO: Navigate to page content
        }
    }

    private void OnLogoutClick(object sender, RoutedEventArgs e)
    {
        _isLoggingOut = true;
        var login = new LoginWindow();
        login.Show();
        this.Close();
    }
}
