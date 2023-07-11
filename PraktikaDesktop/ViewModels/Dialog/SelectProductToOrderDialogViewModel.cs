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
    public class SelectProductToOrderDialogViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;
        private readonly EditOrderViewModel _editOrderViewModel;
        private readonly AddOrderViewModel _addOrderViewModel;
        private readonly MainWindowViewModel _mainWindowViewModel;

        private ObservableCollection<SupplyProduct> _oldSupplyProducts;
        private List<SupplyProduct> _newSupplyProducts = new();
        private List<SupplyProduct> _removedSupplyProducts = new();

        //Supply product
        private ObservableCollection<SupplyProduct> _availableSupplyProduct;
        private readonly ObservableAsPropertyHelper<bool> _supplyProductEmpty;
        private SupplyProduct? _selectedSupplyProduct = null;
        private List<SupplyProduct> _allSupplyProducts;

        //Костыль!!!
        private double _sortingHeight = 45;

        //Search
        private string _searchString;
        private List<SupplyProduct> _searchSupplyProducts;

        //Color sort
        private List<Color> _colorsList;
        private Color? _sortSelectedColor;
        private List<SupplyProduct> _sortByColorSupplyProducts;

        //Product group sort
        private List<ProductGroup> _productGroups;
        private ProductGroup _sortSelectedProductGroup;
        private ProductType _sortSelectedProductType;
        private List<SupplyProduct> _sortByGroupSupplyProducts;

        //Textile search
        public string _searchTextileString;
        private List<SupplyProduct> _searchTextileSupplyProducts;
        #endregion Fields

        #region Properties
        //Products
        public ObservableCollection<SupplyProduct> OldSupplyProducts
        {
            get => _oldSupplyProducts;
            set => this.RaiseAndSetIfChanged(ref _oldSupplyProducts, value);
        }
        public ObservableCollection<SupplyProduct> AvailableSupplyProduct
        {
            get => _availableSupplyProduct;
            set => this.RaiseAndSetIfChanged(ref _availableSupplyProduct, value);
        }
        public bool SupplyProductEmpty => _supplyProductEmpty.Value;
        public SupplyProduct? SelectedSupplyProduct
        {
            get => _selectedSupplyProduct;
            set => this.RaiseAndSetIfChanged(ref _selectedSupplyProduct, value);
        }

        //Костыль!!!
        public double SortingHeight
        {
            get => _sortingHeight;
            set => this.RaiseAndSetIfChanged(ref _sortingHeight, value);
        }

        //Search
        public string SearchString
        {
            get => _searchString;
            set => this.RaiseAndSetIfChanged(ref _searchString, value);
        }

        //Color sorrt
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

        //Product group sort
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

        //Textile search
        public string SearchTextileString
        {
            get => _searchTextileString;
            set => this.RaiseAndSetIfChanged(ref _searchTextileString, value);
        }
        #endregion Properties

        #region Commands
        public void ClickCancelCommand()
        {
            _mainWindowViewModel.CloseDialog(false);
        }

        public void ClickAcceptCommand()
        {
            if (_addOrderViewModel != null)
            {
                _addOrderViewModel.UpdateSupplyProducts(OldSupplyProducts, _newSupplyProducts);

                _mainWindowViewModel.CloseDialog(true);
            }
            else
            {
                _editOrderViewModel.UpdateSupplyProducts(OldSupplyProducts, _newSupplyProducts, _removedSupplyProducts);

                _mainWindowViewModel.CloseDialog(true);
            }
        }
        [DependsOn(nameof(OldSupplyProducts)), DependsOn(nameof(_newSupplyProducts))]
        bool CanClickAcceptCommand(object parameter)
        {
            if (OldSupplyProducts.Count != 0 && _removedSupplyProducts.Count != OldSupplyProducts.Count)
                return true;
            else
                return false;
        }

        public void ExpandSortingCommand()
        {
            if (SortingHeight == 45)
                SortingHeight = 85;
            else
                SortingHeight = 45;
        }

        public void ClearSearchCommand()
        {
            SearchString = null;
            _searchSupplyProducts = null;
            Merger();
        }
        public void ClearSortByColorCommand()
        {
            SortSelectedColor = null;
            _sortByColorSupplyProducts = null;
            Merger();
        }
        public void ClearSearchByGroupCommand()
        {
            SortSelectedProductGroup = null;
            _sortByGroupSupplyProducts = null;
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
            _searchTextileSupplyProducts = null;
            SearchTextile();
        }

        public void ClickChooseCommand()
        {
            SelectedSupplyProduct.Status = "Зарезервирован";
            SelectedSupplyProduct.ListStatus = "new";

            _newSupplyProducts.Add(SelectedSupplyProduct);
            OldSupplyProducts.Add(SelectedSupplyProduct);
            AvailableSupplyProduct.Remove(SelectedSupplyProduct);
            _allSupplyProducts.Remove(SelectedSupplyProduct);

            SelectedSupplyProduct = null;

            //Костыль!!!!
            ObservableCollection<SupplyProduct> supplyProducts = OldSupplyProducts;
            OldSupplyProducts = null;
            OldSupplyProducts = supplyProducts;
        }

        public async void DeleteSupplyProductCommand(SupplyProduct supplyProduct)
        {
            if (supplyProduct.ListStatus == "old")
            {
                bool result = await ShowConfirmationDialog(null);

                if (result)
                {
                    supplyProduct.Status = "В наличии";
                    supplyProduct.ListStatus = "removed";
                    _removedSupplyProducts.Add(supplyProduct);
                }
            }
            else if (supplyProduct.ListStatus == "removed")
            {
                supplyProduct.Status = "Зарезервирован";
                supplyProduct.ListStatus = "old";
                _removedSupplyProducts.Remove(supplyProduct);
            }
            else if (supplyProduct.ListStatus == "new")
            {
                supplyProduct.Status = "В наличии";
                supplyProduct.ListStatus = null;
                OldSupplyProducts.Remove(supplyProduct);
                _newSupplyProducts.Remove(supplyProduct);
                AvailableSupplyProduct.Add(supplyProduct);
                _allSupplyProducts.Add(supplyProduct);
            }

            //Костыль!!!!
            ObservableCollection<SupplyProduct> supplyProducts = OldSupplyProducts;
            OldSupplyProducts = null;
            OldSupplyProducts = supplyProducts;
            supplyProducts = AvailableSupplyProduct;
            AvailableSupplyProduct = null;
            AvailableSupplyProduct = supplyProducts;
        }
        #endregion Commands

        //Constructor
        public SelectProductToOrderDialogViewModel(ViewModelBase callerViewModel, ObservableCollection<SupplyProduct> oldSupplyProducts)
        {
            OldSupplyProducts = new ObservableCollection<SupplyProduct>(oldSupplyProducts);
            _newSupplyProducts.AddRange(OldSupplyProducts.Where(s => s.ListStatus == "new").ToList());
            _removedSupplyProducts.AddRange(OldSupplyProducts.Where(s => s.ListStatus == "removed").ToList());

            foreach (SupplyProduct supplyProduct in oldSupplyProducts)
            {
                supplyProduct.ListStatus ??= "old";
            }

            if (callerViewModel as AddOrderViewModel != null)
            {
                _addOrderViewModel = callerViewModel as AddOrderViewModel;
                _mainWindowViewModel = _addOrderViewModel.OrderViewModel.CurrentMainWindowViewModel;
            }
            else
            {
                _editOrderViewModel = callerViewModel as EditOrderViewModel;
                _mainWindowViewModel = _editOrderViewModel.OrderViewModel.CurrentMainWindowViewModel;
            }

            //Getting data
            _response = ApiRequest.Get("Supply/GetSupplyProducts");
            AvailableSupplyProduct = new ObservableCollection<SupplyProduct>((_response.Content.ReadAsAsync<List<SupplyProduct>>().Result).Where(p => p.Status == "В наличии").ToList());
            _allSupplyProducts = AvailableSupplyProduct.ToList();
            foreach (SupplyProduct sp in _allSupplyProducts)
            {
                foreach(SupplyProduct sp2 in OldSupplyProducts)
                {
                    if(sp.SupplyProductsId == sp2.SupplyProductsId)
                        AvailableSupplyProduct.Remove(sp);
                }
            }
            _allSupplyProducts = AvailableSupplyProduct.ToList();

            _response = ApiRequest.Get("Color/GetColors");
            _colorsList = _response.Content.ReadAsAsync<List<Color>>().Result;

            _response = ApiRequest.Get("ProductGroup/GetProductGroups");
            _productGroups = _response.Content.ReadAsAsync<List<ProductGroup>>().Result;

            //Set tracking
            _supplyProductEmpty = this.WhenAnyValue(vm => vm.AvailableSupplyProduct.Count)
                .Select(t => t == 0 ? true : false)
                .ToProperty(this, vm => vm.SupplyProductEmpty);

            this.WhenAnyValue(vm => vm.SearchString).Subscribe(_ => Search());
            this.WhenAnyValue(vm => vm.SortSelectedColor).Subscribe(_ => SortByColor());
            this.WhenAnyValue(vm => vm.SortSelectedProductType, vm => vm.SortSelectedProductGroup).Subscribe(_ => SortByGroup());

            this.WhenAnyValue(vm => vm.SearchTextileString).Subscribe(_ => SearchTextile());
        }

        //Methods
        private void Search()
        {
            if (SearchString != "" && SearchString != null)
                _searchSupplyProducts = _allSupplyProducts.Where(p => p.Product.Name.ToLower().Contains(SearchString.ToLower()) | p.Product.Dimensions.ToString().Contains(SearchString.ToLower())).ToList();
            else
                _searchSupplyProducts = null;

            Merger();
        }
        private void SearchTextile()
        {
            if (SearchTextileString != "" && SearchTextileString != null)
                _searchTextileSupplyProducts = _allSupplyProducts.Where(p => p.Textile != null && p.Textile.Name.ToLower().Contains(SearchTextileString.ToLower())).ToList();
            else
                _searchTextileSupplyProducts = null;
            Merger();
        }
        private void SortByColor()
        {
            if (SortSelectedColor != null)
                _sortByColorSupplyProducts = _allSupplyProducts.Where(p => p.Product.Color != null && p.Product.Color.ColorId == SortSelectedColor.ColorId).ToList();
            Merger();
        }
        private void SortByGroup()
        {
            if (SortSelectedProductType != null)
                _sortByGroupSupplyProducts = _allSupplyProducts.Where(p => p.Product.ProductType.ProductTypeId == SortSelectedProductType.ProductTypeId).ToList();
            else if (SortSelectedProductGroup != null)
                _sortByGroupSupplyProducts = _allSupplyProducts.Where(p => p.Product.ProductType.ProductGroup.ProductGroupId == SortSelectedProductGroup.ProductGroupId).ToList();

            Merger();
        }
        private void Merger()
        {
            AvailableSupplyProduct = new ObservableCollection<SupplyProduct>(_allSupplyProducts);

            if (_searchSupplyProducts != null && _sortByColorSupplyProducts != null && _sortByGroupSupplyProducts != null && _searchTextileSupplyProducts != null)
            {
                AvailableSupplyProduct = new ObservableCollection<SupplyProduct>(_searchSupplyProducts.Intersect(_sortByColorSupplyProducts.Intersect(_sortByGroupSupplyProducts.Intersect(_searchTextileSupplyProducts))).ToList());
            }

            if (_sortByColorSupplyProducts != null)
            {
                AvailableSupplyProduct = new ObservableCollection<SupplyProduct>(_allSupplyProducts.Intersect(_sortByColorSupplyProducts).ToList());

                if (_searchSupplyProducts != null)
                {
                    AvailableSupplyProduct = new ObservableCollection<SupplyProduct>(AvailableSupplyProduct.Intersect(_searchSupplyProducts).ToList());
                }

                if (_sortByGroupSupplyProducts != null)
                {
                    AvailableSupplyProduct = new ObservableCollection<SupplyProduct>(AvailableSupplyProduct.Intersect(_sortByGroupSupplyProducts).ToList());
                }

                if (_searchTextileSupplyProducts != null)
                {
                    AvailableSupplyProduct = new ObservableCollection<SupplyProduct>(AvailableSupplyProduct.Intersect(_searchTextileSupplyProducts).ToList());
                }
            }
            else
            {
                if (_searchSupplyProducts != null)
                {
                    AvailableSupplyProduct = new ObservableCollection<SupplyProduct>(AvailableSupplyProduct.Intersect(_searchSupplyProducts).ToList());
                }

                if (_sortByGroupSupplyProducts != null)
                {
                    AvailableSupplyProduct = new ObservableCollection<SupplyProduct>(AvailableSupplyProduct.Intersect(_sortByGroupSupplyProducts).ToList());
                }

                if (_searchTextileSupplyProducts != null)
                {
                    AvailableSupplyProduct = new ObservableCollection<SupplyProduct>(AvailableSupplyProduct.Intersect(_searchTextileSupplyProducts).ToList());
                }
            }

            if (_searchSupplyProducts == null && _sortByColorSupplyProducts == null && _sortByGroupSupplyProducts == null && _searchTextileSupplyProducts == null)
            {
                AvailableSupplyProduct = new ObservableCollection<SupplyProduct>(_allSupplyProducts);
            }
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
