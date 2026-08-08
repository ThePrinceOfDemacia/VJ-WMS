using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Serilog;

namespace VjWms.Desktop.UI.Views;

public partial class LoginWindow : Window
{
    private bool _isOnline = false;
    private bool _isLoggingIn = false;
    private readonly string _serverUrl = "http://localhost:5000";

    public LoginWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (!_isLoggingIn)
        {
            Environment.Exit(0);
        }
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        UsernameBox.Focus();
        await CheckServerConnectionAsync();
    }

    private async Task CheckServerConnectionAsync()
    {
        try
        {
            StatusText.Text = FindResource("CheckingConnection") as string;
            StatusDot.Fill = (SolidColorBrush)FindResource("WarningBrush");

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            var response = await client.GetAsync($"{_serverUrl}/api/sync/health");

            if (response.IsSuccessStatusCode)
            {
                _isOnline = true;
                StatusDot.Fill = (SolidColorBrush)FindResource("OnlineBrush");
                StatusText.Text = FindResource("ServerConnected") as string;
                Log.Information("Server connection established: {Url}", _serverUrl);
            }
            else
            {
                SetOffline();
            }
        }
        catch
        {
            SetOffline();
        }
    }

    private void SetOffline()
    {
        _isOnline = false;
        StatusDot.Fill = (SolidColorBrush)FindResource("OfflineBrush");
        StatusText.Text = FindResource("ServerOffline") as string;
        Log.Information("Server unreachable, switching to offline mode");
    }

    private async void OnLoginClick(object sender, RoutedEventArgs e)
    {
        var username = UsernameBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ErrorText.Text = FindResource("EnterCredentialsMsg") as string;
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        LoginButton.IsEnabled = false;
        LoginButton.Content = FindResource("LoginBtnLoading") as string;
        ErrorText.Visibility = Visibility.Collapsed;

        try
        {
            bool authenticated;

            if (_isOnline)
            {
                authenticated = await OnlineLoginAsync(username, password);
            }
            else
            {
                authenticated = OfflineLogin(username, password);
            }

            if (authenticated)
            {
                Log.Information("User {Username} logged in successfully (online={IsOnline})", username, _isOnline);
                _isLoggingIn = true;

                // Ensure user directory exists
                var userId = username; // In real implementation, use server-assigned user ID
                var userDir = Path.Combine(App.AppDataPath, "users", userId);
                Directory.CreateDirectory(userDir);
                Directory.CreateDirectory(Path.Combine(userDir, "attachments"));

                // Open main shell
                var shell = new ShellWindow(username, _isOnline);
                shell.Show();
                this.Close();
            }
            else
            {
                ErrorText.Text = FindResource("WrongCredentialsMsg") as string;
                ErrorText.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Login error for user {Username}", username);
            ErrorText.Text = string.Format(FindResource("LoginErrorMsg") as string, ex.Message);
            ErrorText.Visibility = Visibility.Visible;
        }
        finally
        {
            LoginButton.IsEnabled = true;
            LoginButton.Content = FindResource("LoginBtn") as string;
        }
    }

    private async Task<bool> OnlineLoginAsync(string username, string password)
    {
        // TODO: Call server /api/sync/auth endpoint
        // For Phase 0 alpha, use hardcoded admin/admin123
        await Task.Delay(500); // Simulate network call
        return username == "admin" && password == "admin123";
    }

    private bool OfflineLogin(string username, string password)
    {
        // TODO: Check against user_registry.db cached credentials
        // For Phase 0 alpha, use hardcoded admin/admin123
        return username == "admin" && password == "admin123";
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OnLoginClick(sender, e);
        }
    }

    private void OnVietnameseClick(object sender, MouseButtonEventArgs e)
    {
        LangViBtn.Foreground = (SolidColorBrush)FindResource("PrimaryBrush");
        LangViBtn.FontWeight = FontWeights.SemiBold;
        LangEnBtn.Foreground = (SolidColorBrush)FindResource("TextMutedBrush");
        LangEnBtn.FontWeight = FontWeights.Normal;
        App.SetLanguage("vi");
    }

    private void OnEnglishClick(object sender, MouseButtonEventArgs e)
    {
        LangEnBtn.Foreground = (SolidColorBrush)FindResource("PrimaryBrush");
        LangEnBtn.FontWeight = FontWeights.SemiBold;
        LangViBtn.Foreground = (SolidColorBrush)FindResource("TextMutedBrush");
        LangViBtn.FontWeight = FontWeights.Normal;
        App.SetLanguage("en");
    }
}
