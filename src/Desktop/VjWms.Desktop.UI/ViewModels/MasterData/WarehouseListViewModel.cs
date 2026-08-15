using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VjWms.Desktop.Domain.Entities;
using VjWms.Desktop.Infrastructure.SQLite;

namespace VjWms.Desktop.UI.ViewModels.MasterData;

public partial class WarehouseListViewModel : BaseViewModel
{
    private readonly LocalDbContext _db;

    [ObservableProperty] private string _searchText = "";

    public ObservableCollection<CachedWarehouse> Warehouses { get; } = new();

    public WarehouseListViewModel(LocalDbContext db)
    {
        _db = db;
        Title = "Danh sách Kho";
        LoadWarehouses();
    }

    private void LoadWarehouses()
    {
        Warehouses.Clear();
        var query = _db.CachedWarehouses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(w => w.Code.Contains(SearchText) || w.Name.Contains(SearchText));
        }

        foreach (var w in query.OrderBy(w => w.Code).ToList())
        {
            Warehouses.Add(w);
        }
    }

    [RelayCommand]
    private void Refresh() => LoadWarehouses();

    partial void OnSearchTextChanged(string value) => LoadWarehouses();
}
