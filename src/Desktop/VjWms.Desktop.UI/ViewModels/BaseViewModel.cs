using CommunityToolkit.Mvvm.ComponentModel;

namespace VjWms.Desktop.UI.ViewModels;

/// <summary>
/// Base class for all ViewModels, providing ObservableObject functionality.
/// </summary>
public abstract class BaseViewModel : ObservableObject
{
    private bool _isBusy;
    private string _title = string.Empty;

    /// <summary>
    /// Indicates if the ViewModel is currently performing an asynchronous operation.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// The title of the ViewModel/View.
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
}
