using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VjWms.Desktop.Domain.Entities;
using VjWms.Desktop.Infrastructure.SQLite;

namespace VjWms.Desktop.UI.ViewModels.MasterData;

public partial class ProductListViewModel : BaseViewModel
{
    private readonly LocalDbContext _db;

    [ObservableProperty] private string _searchText = "";

    public ObservableCollection<CachedProduct> Products { get; } = new();

    public ProductListViewModel(LocalDbContext db)
    {
        _db = db;
        Title = "Danh sách Sản phẩm";
        LoadProducts();
    }

    private void LoadProducts()
    {
        Products.Clear();
        var query = _db.CachedProducts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(p => p.ProductCode.Contains(SearchText) || p.ProductName.Contains(SearchText));
        }

        foreach (var p in query.OrderBy(p => p.ProductCode).ToList())
        {
            Products.Add(p);
        }
    }

    [RelayCommand]
    private void Refresh() => LoadProducts();

    partial void OnSearchTextChanged(string value) => LoadProducts();
}
