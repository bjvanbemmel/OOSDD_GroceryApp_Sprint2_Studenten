using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;

namespace Grocery.App.ViewModels
{
    [QueryProperty(nameof(GroceryList), nameof(GroceryList))]
    public partial class GroceryListItemsViewModel : BaseViewModel
    {
        private readonly IGroceryListItemsService _groceryListItemsService;
        private readonly IProductService _productService;
        public ObservableCollection<GroceryListItem> MyGroceryListItems { get; set; } = [];
        public ObservableCollection<Product> AvailableProducts { get; set; } = [];

        [ObservableProperty]
        GroceryList groceryList = new(0, "None", DateOnly.MinValue, "", 0);

        public GroceryListItemsViewModel(IGroceryListItemsService groceryListItemsService, IProductService productService)
        { 
            _groceryListItemsService = groceryListItemsService;
            _productService = productService;
            Load(groceryList.Id);
        }

        private void Load(int id)
        {
            MyGroceryListItems.Clear();
            foreach (var item in _groceryListItemsService.GetAllOnGroceryListId(id)) MyGroceryListItems.Add(item);
            GetAvailableProducts();
        }

        private void GetAvailableProducts()
        {
            AvailableProducts.Clear();
            var products = _productService.GetAll().Where(x => x.Stock > 0);
            var groceryListItems = _groceryListItemsService.GetAllOnGroceryListId(GroceryList.Id);
            products.Where(x => !groceryListItems.Any(y => y.ProductId == x.Id)).ToList().ForEach(product => AvailableProducts.Add(product));
        }

        partial void OnGroceryListChanged(GroceryList value)
        {
            Load(value.Id);
        }

        [RelayCommand]
        public async Task ChangeColor()
        {
            Dictionary<string, object> paramater = new() { { nameof(GroceryList), GroceryList } };
            await Shell.Current.GoToAsync($"{nameof(ChangeColorView)}?Name={GroceryList.Name}", true, paramater);
        }

        [RelayCommand]
        public void AddProduct(Product product)
        {
            if (product is null)
            {
                return;
            }

            if (product.Id <= 0)
            {
                return;
            }

            if (!AvailableProducts.Contains(product))
            {
                return;
            }

            var item = new GroceryListItem(0, GroceryList.Id, product.Id, 1);
            _groceryListItemsService.Add(item);

            product.Stock -= 1;
            _productService.Update(product);

            AvailableProducts.Remove(product);
            OnGroceryListChanged(GroceryList);
        }
    }
}
