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

namespace PraktikaDesktop.ViewModels
{
    public class SalesAndRemainsViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;

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
        private string _searchTextileString;
        private List<SupplyProduct> _searchTextileSupplyProducts;

        //Status sort
        private List<string> _statusList = new List<string> { "В наличии", "Зарезервирован", "Продан" };
        private string _selectedStatus;
        private List<SupplyProduct> _sortByStatusSupplyProducts;
        #endregion Fields

        #region Properties
        //Products
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

        //Status sort
        public List<string> StatusList
        {
            get => _statusList;
            set => this.RaiseAndSetIfChanged(ref _statusList, value);
        }
        public string SelectedStatus
        {
            get => _selectedStatus;
            set => this.RaiseAndSetIfChanged(ref _selectedStatus, value);
        }
        #endregion Properties

        #region Commands
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
        public void ClearSortByStatusCommand()
        {
            SelectedStatus = null;
            _sortByStatusSupplyProducts = null;
            Merger();
        }
        #endregion Commands

        //Constructor
        public SalesAndRemainsViewModel()
        {
            //Getting data
            _response = ApiRequest.Get("Supply/GetSupplyProducts");
            AvailableSupplyProduct = _response.Content.ReadAsAsync<ObservableCollection<SupplyProduct>>().Result;
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
            this.WhenAnyValue(vm => vm.SelectedStatus).Subscribe(_ => SortByStatus());
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
        private void SortByStatus()
        {
            if (SelectedStatus != null)
                _sortByStatusSupplyProducts = _allSupplyProducts.Where(p => p.Status == SelectedStatus).ToList();
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

            if (_searchSupplyProducts != null && _sortByColorSupplyProducts != null && _sortByGroupSupplyProducts != null && _searchTextileSupplyProducts != null && _sortByStatusSupplyProducts != null)
            {
                AvailableSupplyProduct = new ObservableCollection<SupplyProduct>(_searchSupplyProducts.Intersect(_sortByColorSupplyProducts.Intersect(_sortByGroupSupplyProducts.Intersect(_searchTextileSupplyProducts.Intersect(_sortByStatusSupplyProducts)))).ToList());
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

                if (_sortByStatusSupplyProducts != null)
                {
                    AvailableSupplyProduct = new ObservableCollection<SupplyProduct>(AvailableSupplyProduct.Intersect(_sortByStatusSupplyProducts).ToList());
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

                if (_sortByStatusSupplyProducts != null)
                {
                    AvailableSupplyProduct = new ObservableCollection<SupplyProduct>(AvailableSupplyProduct.Intersect(_sortByStatusSupplyProducts).ToList());
                }
            }

            if (_searchSupplyProducts == null && _sortByColorSupplyProducts == null && _sortByGroupSupplyProducts == null && _searchTextileSupplyProducts == null && _sortByStatusSupplyProducts == null)
            {
                AvailableSupplyProduct = new ObservableCollection<SupplyProduct>(_allSupplyProducts);
            }
        }
    }
}
