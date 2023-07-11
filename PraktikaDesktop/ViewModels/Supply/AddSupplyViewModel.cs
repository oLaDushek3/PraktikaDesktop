using Avalonia.Metadata;
using PraktikaDesktop.Models;
using PraktikaDesktop.ViewModels.Dialog;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;

namespace PraktikaDesktop.ViewModels
{
    public class AddSupplyViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;
        private ObservableCollection<SupplyProduct> _allSupplyProducts = new();
        private List<SupplyProduct> _newSupplyProducts = new();

        private Supply _newSupply = new();
        #endregion Fields

        #region Properties
        public readonly SupplyViewModel SupplyViewModel;

        public ObservableCollection<SupplyProduct> AllSupplyProducts
        {
            get => _allSupplyProducts;
            set => this.RaiseAndSetIfChanged(ref _allSupplyProducts, value);
        }

        public Supply NewSupply
        {
            get => _newSupply;
            set
            {
                this.RaiseAndSetIfChanged(ref _newSupply, value);
            }
        }
        #endregion Properties

        #region Command
        public void ClickSaveCommand()
        {
            _response = ApiRequest.Post("Supply/InsertSupply" + $"/{SupplyViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId}", NewSupply);

            _response = ApiRequest.Get("Supply/GetSupplys");
            NewSupply = (_response.Content.ReadAsAsync<List<Supply>>().Result).Last();

            foreach (SupplyProduct newSupplyProduct in _newSupplyProducts)
            {
                newSupplyProduct.SupplyId = NewSupply.SupplyId;
                newSupplyProduct.ProductId = newSupplyProduct.Product.ProductId;
                newSupplyProduct.Product = null;

                if (newSupplyProduct.Textile != null)
                {
                    newSupplyProduct.TextileId = newSupplyProduct.Textile.TextileId;
                    newSupplyProduct.Textile = null;
                }

                _response = ApiRequest.Post("Supply/InsertSupplyProduct" + $"/{SupplyViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId}", newSupplyProduct);
            }

            SupplyViewModel.GoBackCommand(false);
            SupplyViewModel.RefreshList(NewSupply.SupplyId);
        }

        [DependsOn(nameof(AllSupplyProducts))]
        bool CanClickSaveCommand(object? parameter)
        {
            if (AllSupplyProducts.Count != 0)
                return true;
            else
                return false;
        }

        public async void EditSupplyProductsCommand()
        {
            SelectProductsToSupplyDialogViewModel dialogView = new(this, AllSupplyProducts);
            await SupplyViewModel.CurrentMainWindowViewModel.ShowDialog(dialogView);
        }
        #endregion Command

        //Constructor
        public AddSupplyViewModel(SupplyViewModel supplyViewModel)
        {
            SupplyViewModel = supplyViewModel;
        }

        //Methods
        public void UpdateSupplyProducts(ObservableCollection<SupplyProduct> allSupplyProducts, List<SupplyProduct> newSupplyProducts)
        {
            AllSupplyProducts = null;
            AllSupplyProducts = allSupplyProducts;
            _newSupplyProducts = newSupplyProducts;
        }
    }
}
