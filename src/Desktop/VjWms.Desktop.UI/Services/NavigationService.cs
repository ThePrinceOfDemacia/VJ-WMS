using System;
using VjWms.Desktop.UI.ViewModels;

namespace VjWms.Desktop.UI.Services;

/// <summary>
/// Interface for ViewModels that can receive navigation parameters.
/// </summary>
public interface IParameterReceiver
{
    void ReceiveParameter(object parameter);
}

/// <summary>
/// Interface for navigating between view models.
/// </summary>
public interface INavigationService
{
    BaseViewModel? CurrentViewModel { get; }
    event Action? CurrentViewModelChanged;
    void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;
    void NavigateTo<TViewModel>(object? parameter) where TViewModel : BaseViewModel;
}

/// <summary>
/// Manages navigation between ViewModels within the ShellWindow.
/// Uses a factory function to resolve ViewModel instances from DI.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly Func<Type, BaseViewModel> _viewModelFactory;
    private BaseViewModel? _currentViewModel;

    public NavigationService(Func<Type, BaseViewModel> viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;
    }

    public BaseViewModel? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            _currentViewModel = value;
            CurrentViewModelChanged?.Invoke();
        }
    }

    public event Action? CurrentViewModelChanged;

    public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
    {
        CurrentViewModel = _viewModelFactory(typeof(TViewModel));
    }

    public void NavigateTo<TViewModel>(object? parameter) where TViewModel : BaseViewModel
    {
        var vm = _viewModelFactory(typeof(TViewModel));
        if (parameter != null && vm is IParameterReceiver receiver)
        {
            receiver.ReceiveParameter(parameter);
        }
        CurrentViewModel = vm;
    }
}
