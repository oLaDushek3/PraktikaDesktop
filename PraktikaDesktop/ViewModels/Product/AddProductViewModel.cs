using Avalonia.Metadata;
using PraktikaDesktop.Models;
using PraktikaDesktop.ViewModels.Dialog;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;

namespace PraktikaDesktop.ViewModels
{
    public class AddProductViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;
        private Product _newProduct = new();

        //Price and price category
        public ObservableCollection<ProductPriceCategory> _displayedProductPriceCategory = new();
        private ObservableCollection<PriceCategory> _priceCategoryList;
        private bool _priceCategoryActive = false;
        private PriceCategory _selectedPriceCategory;
        private string _priceCategoryPrice;
        private string _price = "";

        private List<Color> _colorList;

        private List<ProductGroup> _productGroupList;
        private ProductGroup _selectedProductGroup;
        #endregion Fields

        #region Properties
        public Product NewProduct
        {
            get => _newProduct;
            set
            {
                this.RaiseAndSetIfChanged(ref _newProduct, value);
            }
        }
        public readonly ProductViewModel ProductViewModel;

        //Price and price category
        public ObservableCollection<ProductPriceCategory> DisplayedProductPriceCategory
        {
            get => _displayedProductPriceCategory;
            set => this.RaiseAndSetIfChanged(ref _displayedProductPriceCategory, value);
        }
        public ObservableCollection<PriceCategory> PriceCategoryList
        {
            get => _priceCategoryList;
            set => this.RaiseAndSetIfChanged(ref _priceCategoryList, value);
        }
        public bool PriceCategoryActive
        {
            get => _priceCategoryActive;
            set => this.RaiseAndSetIfChanged(ref _priceCategoryActive, value);
        }
        public PriceCategory SelectedPriceCategory
        {
            get => _selectedPriceCategory;
            set => this.RaiseAndSetIfChanged(ref _selectedPriceCategory, value);
        }
        public string PriceCategoryPrice
        {
            get => _priceCategoryPrice;
            set
            {
                this.RaiseAndSetIfChanged(ref _priceCategoryPrice, value);
            }
        }
        public string Price
        {
            get => _price;
            set
            {
                this.RaiseAndSetIfChanged(ref _price, value);
            }
        }

        public List<Color> ColorList
        {
            get => _colorList;
            set => this.RaiseAndSetIfChanged(ref _colorList, value);
        }

        public List<ProductGroup> ProductGroupList
        {
            get => _productGroupList;
            set => this.RaiseAndSetIfChanged(ref _productGroupList, value);
        }
        public ProductGroup SelectedProductGroup
        {
            get => _selectedProductGroup;
            set => this.RaiseAndSetIfChanged(ref _selectedProductGroup, value);
        }
        #endregion Properties

        #region Command
        public async void AddProductPriceCategoryCommand()
        {
            if (!int.TryParse(PriceCategoryPrice, out int _))
            {
                InformationDialogViewModel informationDialogViewModel = new(ProductViewModel.CurrentMainWindowViewModel, "Цена должна быть число");
                await ProductViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
            }
            else
            {
                ProductPriceCategory newProductPriceCategory = new ProductPriceCategory
                {
                    Price = Convert.ToDecimal(PriceCategoryPrice),
                    PriceCategory = SelectedPriceCategory,
                };
                PriceCategoryPrice = "";
                SelectedPriceCategory = null;

                DisplayedProductPriceCategory.Add(newProductPriceCategory);
                PriceCategoryList.Remove(newProductPriceCategory.PriceCategory);

                //КОСТЫль
                ObservableCollection<ProductPriceCategory> buffer = DisplayedProductPriceCategory;
                DisplayedProductPriceCategory = null;
                DisplayedProductPriceCategory = buffer;
            }
        }
        [DependsOn(nameof(SelectedPriceCategory)), DependsOn(nameof(PriceCategoryPrice))]
        bool CanAddProductPriceCategoryCommand(object? parameter)
        {
            if (SelectedPriceCategory != null && PriceCategoryPrice != null)
                return true;
            else
                return false;
        }

        public void DeleteProductPriceCategoryCommand(ProductPriceCategory productPriceCategory)
        {
            DisplayedProductPriceCategory.Remove(productPriceCategory);
            PriceCategoryList.Add(productPriceCategory.PriceCategory);

            //КОСТЫль
            ObservableCollection<ProductPriceCategory> buffer = DisplayedProductPriceCategory;
            DisplayedProductPriceCategory = null;
            DisplayedProductPriceCategory = buffer;
        }

        public void ClearSelectedColorCommand()
        {
            NewProduct.Color = null;
        }

        public async void ClickSaveCommand()
        {
            if (NewProduct.Name.Length == 0)
            {
                InformationDialogViewModel informationDialogViewModel = new(ProductViewModel.CurrentMainWindowViewModel, "Название не может быть пустым");
                await ProductViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
            }
            else
            {
                NewProduct.ProductTypeId = NewProduct.ProductType.ProductTypeId;
                NewProduct.ProductType = null;

                if (PriceCategoryActive)
                {
                    NewProduct.Price = null;

                    if (NewProduct.Color != null)
                    {
                        NewProduct.ColorId = NewProduct.Color.ColorId;
                        NewProduct.Color = null;
                    }
                    else
                    {
                        NewProduct.ColorId = null;
                    }

                    _response = ApiRequest.Post("Product/InsertProduct", NewProduct);
                    _response = ApiRequest.Get("Product/GetProducts");
                    NewProduct = (_response.Content.ReadAsAsync<List<Product>>().Result).Last();

                    foreach (var priceCategory in DisplayedProductPriceCategory)
                    {
                        priceCategory.PriceCategoryId = priceCategory.PriceCategory.PriceCategoryId;
                        priceCategory.PriceCategory = null;

                        priceCategory.ProductId = NewProduct.ProductId;
                        priceCategory.Product = null;

                        _response = ApiRequest.Post("Product/InsertProductPriceCategory", priceCategory);
                    }
                    ProductViewModel.GoBackCommand(false);
                    ProductViewModel.RefreshList(NewProduct.ProductId);
                }
                else
                {
                    if (NewProduct.Color == null)
                    {
                        InformationDialogViewModel informationDialogViewModel = new(ProductViewModel.CurrentMainWindowViewModel, "Уточните цвет или ценовые категории для тканей");
                        await ProductViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
                    }
                    else
                    {
                        NewProduct.ColorId = NewProduct.Color.ColorId;
                        NewProduct.Color = null;

                        NewProduct.Price = Convert.ToDecimal(Price);

                        _response = ApiRequest.Post("Product/InsertProduct", NewProduct);
                        _response = ApiRequest.Get("Product/GetProducts");
                        NewProduct = (_response.Content.ReadAsAsync<List<Product>>().Result).Last();

                        ProductViewModel.GoBackCommand(false);
                        ProductViewModel.RefreshList(NewProduct.ProductId);
                    }
                }
            }
        }
        [DependsOn(nameof(DisplayedProductPriceCategory)), DependsOn(nameof(Price)), DependsOn(nameof(PriceCategoryActive))]
        bool CanClickSaveCommand(object? parameter)
        {
            if (PriceCategoryActive)
            {
                if (DisplayedProductPriceCategory.Count == 0)
                    return false;
                else
                    return true;
            }
            else
            {
                if (Price.Length == 0)
                    return false;
                else
                    return true;
            }
        }
        #endregion Command

        //Constructor
        public AddProductViewModel(ProductViewModel supplyViewModel)
        {
            ProductViewModel = supplyViewModel;

            //Getting data
            _response = ApiRequest.Get("Product/GetPriceCategories");
            PriceCategoryList = _response.Content.ReadAsAsync<ObservableCollection<PriceCategory>>().Result;

            _response = ApiRequest.Get("Color/GetColors");
            ColorList = _response.Content.ReadAsAsync<List<Color>>().Result;

            _response = ApiRequest.Get("ProductGroup/GetProductGroups");
            ProductGroupList = _response.Content.ReadAsAsync<List<ProductGroup>>().Result;
        }
    }
}
