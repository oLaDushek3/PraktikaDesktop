using Avalonia.Metadata;
using PraktikaDesktop.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace PraktikaDesktop.ViewModels.Dialog
{
    public class SelectProductsToSupplyDialogViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;
        private readonly EditSupplyViewModel _editSupplyViewModel;
        private readonly AddSupplyViewModel _addSupplyViewModel;
        private readonly MainWindowViewModel _mainWindowViewModel;

        private ObservableCollection<SupplyProduct> _allSupplyProducts;
        private List<SupplyProduct> _newSupplyProducts = new();
        private List<SupplyProduct> _removedSupplyProducts = new();

        //Products
        private ObservableCollection<Product> _displayedProducts;
        private readonly ObservableAsPropertyHelper<bool> _productsEmpty;
        private Product? _selectedProduct = null;
        private List<Product> _allProducts;

        //Sort
        private double _sortingHeight = 45;

        private string _searchString;
        private List<Product> _searchProducts;

        private List<Color> _colorsList;
        private Color? _sortSelectedColor;
        private List<Product> _sortByColorProducts;
        private List<Product> _sortByGroupProducts;

        private List<ProductGroup> _productGroups;
        private ProductGroup _sortSelectedProductGroup;
        private ProductType _sortSelectedProductType;

        //Textile
        private ObservableCollection<Textile> _displayedTextiles;
        private Textile? _selectedTextile;
        public List<Textile> _allTextiles;
        public string _searchTextileString;

        private bool _emptinessProductPriceCategorys;
        private string _emptinessProductPriceCategoryMessage;
        #endregion Fields

        #region Properties
        //Products
        public ObservableCollection<SupplyProduct> AllSupplyProducts
        {
            get => _allSupplyProducts;
            set => this.RaiseAndSetIfChanged(ref _allSupplyProducts, value);
        }
        public ObservableCollection<Product> DisplayedProducts
        {
            get => _displayedProducts;
            set => this.RaiseAndSetIfChanged(ref _displayedProducts, value);
        }
        public bool ProductsEmpty => _productsEmpty.Value;
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set => this.RaiseAndSetIfChanged(ref _selectedProduct, value);
        }

        //Sort
        public double SortingHeight
        {
            get => _sortingHeight;
            set => this.RaiseAndSetIfChanged(ref _sortingHeight, value);
        }

        public string SearchString
        {
            get => _searchString;
            set => this.RaiseAndSetIfChanged(ref _searchString, value);
        }

        public List<Color> ColorsList
        {
            get => _colorsList;
            set => this.RaiseAndSetIfChanged(ref _colorsList, value);
        }
        public Color SortSelectedColor
        {
            get => _sortSelectedColor;
            set => this.RaiseAndSetIfChanged(ref _sortSelectedColor, value);
        }

        public List<ProductGroup> ProductGroups
        {
            get => _productGroups;
            set => this.RaiseAndSetIfChanged(ref _productGroups, value);
        }
        public ProductGroup SortSelectedProductGroup
        {
            get => _sortSelectedProductGroup;
            set => this.RaiseAndSetIfChanged(ref _sortSelectedProductGroup, value);
        }
        public ProductType SortSelectedProductType
        {
            get => _sortSelectedProductType;
            set => this.RaiseAndSetIfChanged(ref _sortSelectedProductType, value);
        }

        //Textile
        private ObservableCollection<Textile> DisplayedTextiles
        {
            get => _displayedTextiles;
            set => this.RaiseAndSetIfChanged(ref _displayedTextiles, value);
        }
        private Textile? SelectedTextile
        {
            get => _selectedTextile;
            set => this.RaiseAndSetIfChanged(ref _selectedTextile, value);
        }
        public string SearchTextileString
        {
            get => _searchTextileString;
            set => this.RaiseAndSetIfChanged(ref _searchTextileString, value);
        }

        public string EmptinessProductPriceCategoryMessage
        {
            get => _emptinessProductPriceCategoryMessage;
            set => this.RaiseAndSetIfChanged(ref _emptinessProductPriceCategoryMessage, value);
        }

        public bool EmptinessProductPriceCategorys
        {
            get => _emptinessProductPriceCategorys;
            set => this.RaiseAndSetIfChanged(ref _emptinessProductPriceCategorys, value);
        }
        #endregion Properties

        #region Commands
        public void ClickCancelCommand()
        {
            _mainWindowViewModel.CloseDialog(false);
        }

        public void ClickAcceptCommand()
        {
            if (_editSupplyViewModel != null)
            {
                _editSupplyViewModel.UpdateSupplyProducts(AllSupplyProducts, _newSupplyProducts, _removedSupplyProducts);

                _mainWindowViewModel.CloseDialog(true);
            }
            else
            {
                _addSupplyViewModel.UpdateSupplyProducts(AllSupplyProducts, _newSupplyProducts);

                _mainWindowViewModel.CloseDialog(true);
            }

        }
        [DependsOn(nameof(AllSupplyProducts)), DependsOn(nameof(_newSupplyProducts))]
        bool CanClickAcceptCommand(object parameter)
        {
            if (AllSupplyProducts.Count != 0 && _removedSupplyProducts.Count != AllSupplyProducts.Count)
                return true;
            else
                return false;
        }

        public void ExpandSortingCommand()
        {
            if(SortingHeight == 45)
                SortingHeight = 85;
            else
                SortingHeight = 45;
        }

        public void ClearSearchCommand()
        {
            SearchString = null;
            _searchProducts = null;
            Merger();
        }
        public void ClearSortByColorCommand()
        {
            SortSelectedColor = null;
            _sortByColorProducts = null;
            Merger();
        }
        public void ClearSearchByGroupCommand()
        {
            SortSelectedProductGroup = null;
            _sortByGroupProducts = null;
            Merger();
        }
        public void ClearSearchByTypeCommand()
        {
            SortSelectedProductType = null;
            Merger();
        }

        public void ClearTextileSearchCommand()
        {
            SearchTextileString = null;
            SearchTextile();
        }

        public void ClickChooseCommand()
        {
            SupplyProduct newSupplyProduct = new();
            newSupplyProduct.Product = SelectedProduct;
            newSupplyProduct.Status = "В наличии";
            newSupplyProduct.ListStatus = "new";

            if (SelectedProduct.ProductPriceCategories.Count != 0)
                newSupplyProduct.Textile = SelectedTextile;

            _newSupplyProducts.Add(newSupplyProduct);
            AllSupplyProducts.Add(newSupplyProduct);

            SelectedProduct = null;
            SelectedTextile = null;

            //Костыль!!!!
            ObservableCollection<SupplyProduct> supplyProducts = AllSupplyProducts;
            AllSupplyProducts = null;
            AllSupplyProducts = supplyProducts;
        }
        [DependsOn(nameof(SelectedProduct)), DependsOn(nameof(SelectedTextile))]
        bool CanClickChooseCommand(object parameter)
        {
            if(SelectedProduct == null)
                return false;
            bool result;

            if (SelectedProduct.ProductPriceCategories.Count != 0)
                result = SelectedTextile != null ? true : false;
            else
                result = SelectedProduct != null ? true : false;
            return result;
        }

        public async void DeleteSupplyProductCommand (SupplyProduct supplyProduct)
        {
            if(supplyProduct.ListStatus == "old")
            {
                bool result;
                if (supplyProduct.Orders.Count != 0)
                {
                    result = await ShowConfirmationDialog("Эта запись связяна с заказов №" + (supplyProduct.Orders.First()).OrderId);
                }
                else
                {
                    result = await ShowConfirmationDialog(null);
                }

                if (result)
                {
                    supplyProduct.ListStatus = "removed";
                    _removedSupplyProducts.Add(supplyProduct);
                }
            }
            else if (supplyProduct.ListStatus == "removed")
            {
                supplyProduct.ListStatus = "old";
                _removedSupplyProducts.Remove(supplyProduct);
            }
            else if (supplyProduct.ListStatus == "new")
            {
                AllSupplyProducts.Remove(supplyProduct);
                _newSupplyProducts.Remove(supplyProduct);
            }

            //Костыль!!!!
            ObservableCollection<SupplyProduct> supplyProducts = AllSupplyProducts;
            AllSupplyProducts = null;
            AllSupplyProducts = supplyProducts;
        }
        #endregion Commands

        //Constructor
        public SelectProductsToSupplyDialogViewModel(ViewModelBase callerViewModel, ObservableCollection<SupplyProduct> allSupplyProducts)
        {
            AllSupplyProducts = new ObservableCollection<SupplyProduct>(allSupplyProducts);
            _newSupplyProducts.AddRange(AllSupplyProducts.Where(s => s.ListStatus == "new").ToList());
            _removedSupplyProducts.AddRange(AllSupplyProducts.Where(s => s.ListStatus == "removed").ToList());

            foreach (SupplyProduct supplyProduct in AllSupplyProducts)
            {
                if(supplyProduct.ListStatus == null)
                    supplyProduct.ListStatus = "old";
            }

            if (callerViewModel as EditSupplyViewModel != null)
            {
                _editSupplyViewModel = callerViewModel as EditSupplyViewModel;
                _mainWindowViewModel = _editSupplyViewModel.SupplyViewModel.CurrentMainWindowViewModel;
            }
            else
            {
                _addSupplyViewModel = callerViewModel as AddSupplyViewModel;
                _mainWindowViewModel = _addSupplyViewModel.SupplyViewModel.CurrentMainWindowViewModel;
            }

            //Getting data
            _response = ApiRequest.Get("Product/GetProducts");
            DisplayedProducts = _response.Content.ReadAsAsync<ObservableCollection<Product>>().Result;

            _response = ApiRequest.Get("Product/GetProducts");
            _allProducts = _response.Content.ReadAsAsync<List<Product>>().Result;

            _response = ApiRequest.Get("Color/GetColors");
            _colorsList = _response.Content.ReadAsAsync<List<Color>>().Result;

            _response = ApiRequest.Get("ProductGroup/GetProductGroups");
            _productGroups = _response.Content.ReadAsAsync<List<ProductGroup>>().Result;

            _response = ApiRequest.Get("Textile/GetTextiles");
            DisplayedTextiles = _response.Content.ReadAsAsync<ObservableCollection<Textile>>().Result;

            _response = ApiRequest.Get("Textile/GetTextiles");
            _allTextiles = _response.Content.ReadAsAsync<List<Textile>>().Result;

            //Set tracking
            _productsEmpty = this.WhenAnyValue(vm => vm.DisplayedProducts.Count)
                .Select(t => t == 0 ? true : false)
                .ToProperty(this, vm => vm.ProductsEmpty);

            this.WhenAnyValue(vm => vm.SelectedProduct).Subscribe(_ => SelectProduct());

            this.WhenAnyValue(vm => vm.SearchString).Subscribe(_ => Search());
            this.WhenAnyValue(vm => vm.SortSelectedColor).Subscribe(_ => SortByColor());
            this.WhenAnyValue(vm => vm.SortSelectedProductType, vm => vm.SortSelectedProductGroup).Subscribe(_ => SortByGroup());

            this.WhenAnyValue(vm => vm.SearchTextileString).Subscribe(_ => SearchTextile());
        }

        //Methods
        private void SelectProduct()
        {
            SelectedTextile = null;
            EmptinessProductPriceCategorys = false;
            if (_selectedProduct == null)
            {
                EmptinessProductPriceCategorys = true;
                EmptinessProductPriceCategoryMessage = "Выберите продукцию";
            }
            if (_selectedProduct != null)
            {
                if (_selectedProduct.ProductPriceCategories.Count == 0)
                {
                    EmptinessProductPriceCategorys = true;
                    EmptinessProductPriceCategoryMessage = "Продукция без ткани";
                }
            }
        }

        private void Search()
        {
            if (SearchString != "" && SearchString != null)
                _searchProducts = _allProducts.Where(p => p.Name.ToLower().Contains(SearchString.ToLower()) | p.Dimensions.ToString().Contains(SearchString.ToLower())).ToList();
            else
                _searchProducts = null;

            Merger();
        }
        private void SortByColor()
        {
            if (SortSelectedColor != null)
                _sortByColorProducts = _allProducts.Where(p => p.Color != null && p.Color.ColorId == SortSelectedColor.ColorId).ToList();
            Merger();
        }
        private void SortByGroup()
        {
            if (SortSelectedProductType != null)
                _sortByGroupProducts = _allProducts.Where(p => p.ProductType.ProductTypeId == SortSelectedProductType.ProductTypeId).ToList();
            else if (SortSelectedProductGroup != null)
                _sortByGroupProducts = _allProducts.Where(p => p.ProductType.ProductGroup.ProductGroupId == SortSelectedProductGroup.ProductGroupId).ToList();

            Merger();
        }
        private void Merger()
        {
            DisplayedProducts = new ObservableCollection<Product>(_allProducts);

            if (_searchProducts != null && _sortByColorProducts != null && _sortByGroupProducts != null)
            {
                DisplayedProducts = new ObservableCollection<Product>(_searchProducts.Intersect(_sortByColorProducts.Intersect(_sortByGroupProducts)).ToList());
            }

            if (_sortByColorProducts != null)
            {
                DisplayedProducts = new ObservableCollection<Product>(_allProducts.Intersect(_sortByColorProducts).ToList());

                if (_searchProducts != null)
                {
                    DisplayedProducts = new ObservableCollection<Product>(DisplayedProducts.Intersect(_searchProducts).ToList());
                }

                if (_sortByGroupProducts != null)
                {
                    DisplayedProducts = new ObservableCollection<Product>(DisplayedProducts.Intersect(_sortByGroupProducts).ToList());
                }
            }
            else
            {
                if (_searchProducts != null)
                {
                    DisplayedProducts = new ObservableCollection<Product>(DisplayedProducts.Intersect(_searchProducts).ToList());
                }

                if (_sortByGroupProducts != null)
                {
                    DisplayedProducts = new ObservableCollection<Product>(DisplayedProducts.Intersect(_sortByGroupProducts).ToList());
                }
            }

            if (_searchProducts == null && _sortByColorProducts == null && _sortByGroupProducts == null)
            {
                DisplayedProducts = new ObservableCollection<Product>(_allProducts);
            }
        }

        private void SearchTextile()
        {
            if (SearchTextileString != "" && SearchTextileString != null)
                DisplayedTextiles = new ObservableCollection<Textile>(_allTextiles.Where(p => p.Name.ToLower().Contains(SearchTextileString.ToLower())).ToList());
            else
                DisplayedTextiles = new ObservableCollection<Textile>(_allTextiles);
        }

        #region Dialog
        //Dialog fields
        private ViewModelBase? _dialogView;
        private bool _confirmationDialogDisplayed = false; 
        
        //Dialog properties
        public ViewModelBase? DialogView
        {
            get => _dialogView;
            set
            {
                this.RaiseAndSetIfChanged(ref _dialogView, value);
            }
        }

        public bool ConfirmationDialogDisplayed
        {
            get => _confirmationDialogDisplayed;
            set
            {
                this.RaiseAndSetIfChanged(ref _confirmationDialogDisplayed, value);
            }
        }

        public delegate void CloseDialogDelegate();
        public event CloseDialogDelegate CloseDialogEvent;
        public bool DialogResult;

        //Dialog methods
        public Task<bool> ShowConfirmationDialog(string? message)
        {
            ConfirmationDialogViewModel confirmationDialogViewModel = new ConfirmationDialogViewModel(this, message);
            BaseDialogViewModel baseDialogViewModel = new(confirmationDialogViewModel);
            DialogView = baseDialogViewModel;

            ConfirmationDialogDisplayed = true;

            var completion = new TaskCompletionSource<bool>();

            CloseDialogEvent += () => completion.TrySetResult(DialogResult);
            return completion.Task;
        }
        public override void CloseDialog(bool dialogResult)
        {
            DialogResult = dialogResult;
            DialogView = null;
            ConfirmationDialogDisplayed = false;

            CloseDialogEvent?.Invoke();
        }
        #endregion Dialog
    }
}