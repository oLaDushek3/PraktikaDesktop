using Avalonia.Metadata;
using PraktikaDesktop.Models;
using PraktikaDesktop.ViewModels.Dialog;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;

namespace PraktikaDesktop.ViewModels
{
    public class ProductViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;

        private readonly ObservableAsPropertyHelper<bool> _productsEmpty;
        private ObservableCollection<Product> _displayedProducts;
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

        //Selected product
        private bool _productIsSelected = false;
        private Product? _selectedProduct;
        private readonly ObservableAsPropertyHelper<bool> _productPriceCategorysEmpty;

        private ViewModelBase _currentChildView;
        private bool _animationAction = false;
        #endregion Fields

        #region Properties
        public readonly MainWindowViewModel CurrentMainWindowViewModel;

        public bool ProductsEmpty => _productsEmpty.Value;
        public ObservableCollection<Product> DisplayedProducts
        {
            get => _displayedProducts;
            set
            {
                this.RaiseAndSetIfChanged(ref _displayedProducts, value);
            }
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

        //Selected product
        public bool ProductIsSelected
        {
            get => _productIsSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _productIsSelected, value);
            }
        }
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedProduct, value);
                ProductIsSelected = SelectedProduct == null ? false : true;
            }
        }
        public bool ProductPriceCategorysEmpty => _productPriceCategorysEmpty.Value;

        public ViewModelBase CurrentChildView
        {
            get => _currentChildView;
            set => this.RaiseAndSetIfChanged(ref _currentChildView, value);
        }
        private bool AnimationAction
        {
            get => _animationAction;
            set => this.RaiseAndSetIfChanged(ref _animationAction, value);
        }
        #endregion Properties

        #region Commands
        public void EditCommand()
        {
            EditProductViewModel editProductViewModel = new EditProductViewModel(this);
            CurrentChildView = editProductViewModel;
            AnimationAction = true;
        }
        [DependsOn(nameof(SelectedProduct)), DependsOn(nameof(AnimationAction))]
        bool CanEditCommand(object? obj)
        {
            if (SelectedProduct != null && !AnimationAction)
                return true;
            else
                return false;
        }

        public void AddCommand()
        {
            AddProductViewModel addProductViewModel = new AddProductViewModel(this);
            CurrentChildView = addProductViewModel;
            AnimationAction = true;
        }
        [DependsOn(nameof(AnimationAction))]
        bool CanAddCommand(object? obj)
        {
            return !AnimationAction;
        }

        public async void GoBackCommand(bool callConfirmation)
        {
            if (callConfirmation)
            {
                ConfirmationDialogViewModel confirmationDialogViewModel = new(CurrentMainWindowViewModel, "Изменения не сохранятся");
                bool result = await CurrentMainWindowViewModel.ShowDialog(confirmationDialogViewModel);

                if (result)
                    AnimationAction = false;
            }
            else
                AnimationAction = false;

        }
        [DependsOn(nameof(AnimationAction))]
        bool CanGoBackCommand(object? obj)
        {
            return AnimationAction;
        }

        public async void DeleteProductCommand(Product deletedProduct)
        {
            ConfirmationDialogViewModel confirmationDialogViewModel = new ConfirmationDialogViewModel(CurrentMainWindowViewModel, "Связанные данные будут удалены");
            bool result = await CurrentMainWindowViewModel.ShowDialog(confirmationDialogViewModel);

            if (result)
            {
                _response = ApiRequest.Delete($"Product/DeleteProduct/{deletedProduct.ProductId}");

                if (_response.IsSuccessStatusCode)
                {
                    if (SelectedProduct == deletedProduct)
                        SelectedProduct = null;
                    DisplayedProducts.Remove(deletedProduct);
                }
                else
                {
                    InformationDialogViewModel informationDialogViewModel = new(CurrentMainWindowViewModel, "Не удалось выполнить запрос");
                    await CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
                }
            }
        }

        public void ExpandSortingCommand()
        {
            if (SortingHeight == 45)
                SortingHeight = 90;
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
        #endregion Commands

        //Constructor
        public ProductViewModel(MainWindowViewModel mainWindowViewModel)
        {
            CurrentMainWindowViewModel = mainWindowViewModel;
            RefreshList(null);

            _response = ApiRequest.Get("Color/GetColors");
            _colorsList = _response.Content.ReadAsAsync<List<Color>>().Result;

            _response = ApiRequest.Get("ProductGroup/GetProductGroups");
            _productGroups = _response.Content.ReadAsAsync<List<ProductGroup>>().Result;

            _productsEmpty = this.WhenAnyValue(vm => vm.DisplayedProducts.Count)
                .Select(t => t == 0 ? true : false)
                .ToProperty(this, vm => vm.ProductsEmpty);

            _productPriceCategorysEmpty = this.WhenAnyValue(vm => vm.SelectedProduct.ProductPriceCategories.Count)
                .Select(t => t == 0 ? true : false)
                .ToProperty(this, vm => vm.ProductPriceCategorysEmpty);

            this.WhenAnyValue(vm => vm.SearchString).Subscribe(_ => Search());
            this.WhenAnyValue(vm => vm.SortSelectedColor).Subscribe(_ => SortByColor());
            this.WhenAnyValue(vm => vm.SortSelectedProductType, vm => vm.SortSelectedProductGroup).Subscribe(_ => SortByGroup());
        }

        //Methods
        public void RefreshList(int? selectedItemId)
        {
            _response = ApiRequest.Get("Product/GetProducts");
            DisplayedProducts = _response.Content.ReadAsAsync<ObservableCollection<Product>>().Result;

            _response = ApiRequest.Get("Product/GetProducts");
            _allProducts = _response.Content.ReadAsAsync<List<Product>>().Result;

            if (selectedItemId != null)
                SelectedProduct = DisplayedProducts.Where(s => s.ProductId == selectedItemId).First();
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
    }
}