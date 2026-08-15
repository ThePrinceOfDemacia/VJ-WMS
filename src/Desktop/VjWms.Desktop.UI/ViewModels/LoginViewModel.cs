using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VjWms.Desktop.Infrastructure.SQLite;
using VjWms.Desktop.UI.Views;

namespace VjWms.Desktop.UI.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly Func<string, LocalDbContext> _dbFactory;

    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _statusText = "Đang kiểm tra kết nối...";
    [ObservableProperty] private bool _isOnline;

    public LoginViewModel(Func<string, LocalDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        Title = "Đăng nhập";
        _ = CheckServerConnectionAsync();
    }

    private async Task CheckServerConnectionAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            var response = await client.GetAsync("http://localhost:5000/api/sync/health");

            if (response.IsSuccessStatusCode)
            {
                IsOnline = true;
                StatusText = "Kết nối máy chủ: ONLINE";
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
        IsOnline = false;
        StatusText = "Chế độ: OFFLINE";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ShowError("Vui lòng nhập tên đăng nhập và mật khẩu.");
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            // Simulate brief delay
            await Task.Delay(500);

            // Create directories for the user
            var userDir = Path.Combine(App.AppDataPath, "users", Username);
            Directory.CreateDirectory(userDir);
            Directory.CreateDirectory(Path.Combine(userDir, "attachments"));

            // Initialize local db for this user
            var dbPath = Path.Combine(userDir, "local.db");
            using var db = _dbFactory(dbPath);

            // Phase 1: simple local auth against seed data or hardcoded fallback
            var user = db.CachedUsers.FirstOrDefault(u => u.Username == Username);
            bool authenticated = false;

            if (user != null)
            {
                // In Phase 1 we just compare plain text seeded hash
                authenticated = user.PasswordHash == Password;
            }
            else
            {
                // Fallback for Phase 0 compatibility just in case db is empty
                authenticated = Username == "admin" && Password == "admin123";
            }

            if (authenticated)
            {
                Log.Information("User {Username} logged in successfully.", Username);
                var shell = new ShellWindow(Username, IsOnline);
                shell.Show();
                
                // Note: The actual window close will be handled in the View's code-behind
                // by publishing a message or passing an Action, but for simplicity here 
                // we'll rely on the LoginWindow catching a successful login event if needed.
                // Since this is MVVM, we can use an Action callback.
                OnLoginSuccess?.Invoke();
            }
            else
            {
                ShowError("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Login error");
            ShowError($"Lỗi hệ thống: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    public Action? OnLoginSuccess { get; set; }
}
