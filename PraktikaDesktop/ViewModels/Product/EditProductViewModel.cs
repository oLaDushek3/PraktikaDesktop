using Avalonia.Metadata;
using DynamicData.Binding;
using PraktikaDesktop.Models;
using PraktikaDesktop.ViewModels.Dialog;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;

namespace PraktikaDesktop.ViewModels
{
    public class EditProductViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;
        private Product _editabledProduct;

        //Price and price category
        public ObservableCollection<ProductPriceCategory> _displayedProductPriceCategory;
        private ObservableCollection<PriceCategory> _priceCategoryList;
        private bool _priceCategoryActive = false;
        private PriceCategory _selectedPriceCategory;
        private string _priceCategoryPrice;
        private string _price;

        private List<Color> _colorList;
        private Color _selectedColor;

        private List<ProductGroup> _productGroupList;
        private ProductGroup _selectedProductGroup;
        private ProductType _selectedProductType;
        #endregion Fields

        #region Properties
        public Product EditabledProduct
        {
            get => _editabledProduct;
            set
            {
                this.RaiseAndSetIfChanged(ref _editabledProduct, value);
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
        public Color SelectedColor
        {
            get => _selectedColor;
            set => this.RaiseAndSetIfChanged(ref _selectedColor, value);
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
        public ProductType SelectedProductType
        {
            get => _selectedProductType;
            set => this.RaiseAndSetIfChanged(ref _selectedProductType, value);
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
                    ProductId = EditabledProduct.ProductId,
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
            SelectedColor = null;
        }

        public async void ClickSaveCommand()
        {
            if(EditabledProduct.Name.Length == 0)
            {
                InformationDialogViewModel informationDialogViewModel = new(ProductViewModel.CurrentMainWindowViewModel, "Название не может быть пустым");
                await ProductViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
            }
            else
            {
                EditabledProduct.ProductType = null;
                EditabledProduct.ProductTypeId = SelectedProductType.ProductTypeId;

                if (PriceCategoryActive)
                {
                    EditabledProduct.Price = null;

                    if (SelectedColor != null)
                    {
                        EditabledProduct.Color = null;
                        EditabledProduct.ColorId = SelectedColor.ColorId;
                    }
                    else
                    {
                        EditabledProduct.ColorId = null;
                    }

                    _response = ApiRequest.Delete("Product/DeleteProductPriceCategory/" + EditabledProduct.ProductId);
                    foreach (var priceCategory in DisplayedProductPriceCategory)
                    {
                        priceCategory.PriceCategoryId = priceCategory.PriceCategory.PriceCategoryId;
                        priceCategory.PriceCategory = null;

                        priceCategory.ProductId = EditabledProduct.ProductId;
                        priceCategory.Product = null;

                        _response = ApiRequest.Post("Product/InsertProductPriceCategory", priceCategory);
                    }
                    _response = ApiRequest.Put("Product/UpdateProduct", EditabledProduct);
                    ProductViewModel.GoBackCommand(false);
                    ProductViewModel.RefreshList(EditabledProduct.ProductId);
                }
                else
                {
                    if (SelectedColor == null)
                    {
                        InformationDialogViewModel informationDialogViewModel = new(ProductViewModel.CurrentMainWindowViewModel, "Уточните цвет или ценовые категории для тканей");
                        await ProductViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
                    }
                    else
                    {
                        EditabledProduct.Color = null;
                        EditabledProduct.ColorId = SelectedColor.ColorId;

                        EditabledProduct.Price = Convert.ToDecimal(Price);

                        _response = ApiRequest.Delete("Product/DeleteProductPriceCategory/" + EditabledProduct.ProductId);

                        _response = ApiRequest.Put("Product/UpdateProduct", EditabledProduct);
                        ProductViewModel.GoBackCommand(false);
                        ProductViewModel.RefreshList(EditabledProduct.ProductId);
                    }
                }
            }
        }
        [DependsOn(nameof(DisplayedProductPriceCategory)), DependsOn(nameof(Price)), DependsOn(nameof(PriceCategoryActive))]
        bool CanClickSaveCommand(object? parameter)
        {
            if (PriceCategoryActive)
            {
                if(DisplayedProductPriceCategory.Count == 0)
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
        public EditProductViewModel(ProductViewModel supplyViewModel)
        {            
            ProductViewModel = supplyViewModel;

            //Getting data
            _response = ApiRequest.Get("Product/GetProductById/" + ProductViewModel.SelectedProduct.ProductId);
            EditabledProduct = _response.Content.ReadAsAsync<Product>().Result;

            _response = ApiRequest.Get("Product/GetPriceCategories");
            PriceCategoryList = _response.Content.ReadAsAsync<ObservableCollection<PriceCategory>>().Result;

            DisplayedProductPriceCategory = new ObservableCollection<ProductPriceCategory>(EditabledProduct.ProductPriceCategories);
            if (DisplayedProductPriceCategory.Count != 0)
            {
                PriceCategoryActive = true;
                foreach (ProductPriceCategory availablePriceCategory in DisplayedProductPriceCategory)
                {
                    PriceCategoryList.Remove(PriceCategoryList.Where(p => p.Category == availablePriceCategory.PriceCategory.Category).First());
                }
            }
            else
                Price = EditabledProduct.Price.ToString();

            _response = ApiRequest.Get("Color/GetColors");
            ColorList = _response.Content.ReadAsAsync<List<Color>>().Result;

            _response = ApiRequest.Get("ProductGroup/GetProductGroups");
            ProductGroupList = _response.Content.ReadAsAsync<List<ProductGroup>>().Result;

            //Set data
            if (EditabledProduct.Color != null)
                SelectedColor = ColorList.Where(c => c.Name == EditabledProduct.Color.Name).First();

            SelectedProductGroup = ProductGroupList.Where(p => p.Name == EditabledProduct.ProductType.ProductGroup.Name).First();
            SelectedProductType = SelectedProductGroup.ProductTypes.Where(p => p.Name == EditabledProduct.ProductType.Name).First();
        }
    }
}
