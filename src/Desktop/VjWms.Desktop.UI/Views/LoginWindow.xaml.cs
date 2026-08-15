using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using VjWms.Desktop.UI.ViewModels;

namespace VjWms.Desktop.UI.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        
        // Resolve ViewModel from DI
        var vm = App.Services.GetRequiredService<LoginViewModel>();
        vm.OnLoginSuccess = () => 
        {
            App.CurrentUsername = vm.Username; // Set global state for DB context
            this.Close();
        };
        this.DataContext = vm;
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

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (this.DataContext is LoginViewModel vm)
        {
            vm.Password = PasswordBox.Password;
        }
    }

    private void OnPasswordKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && this.DataContext is LoginViewModel vm)
        {
            if (vm.LoginCommand.CanExecute(null))
            {
                vm.LoginCommand.Execute(null);
            }
        }
    }

    private void OnUsernameKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            PasswordBox.Focus();
        }
    }
}
