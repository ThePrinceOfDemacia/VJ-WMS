using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using VjWms.Desktop.UI.ViewModels;

namespace VjWms.Desktop.UI.Views;

public partial class ShellWindow : Window
{
    private bool _isLoggingOut = false;

    public ShellWindow(string username, bool isOnline)
    {
        InitializeComponent();
        
        var vm = App.Services.GetRequiredService<ShellViewModel>();
        vm.Username = username;
        vm.IsOnline = isOnline;
        this.DataContext = vm;

        SetupUI(username, isOnline);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (!_isLoggingOut)
        {
            Environment.Exit(0);
        }
    }

    private void SetupUI(string username, bool isOnline)
    {
        // User info
        UserInitial.Text = username.Length > 0 ? username[0].ToString().ToUpper() : "?";

        // Status bar
        if (isOnline)
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

    private void OnLogoutClick(object sender, RoutedEventArgs e)
    {
        _isLoggingOut = true;
        var login = new LoginWindow();
        login.Show();
        this.Close();
    }
}
