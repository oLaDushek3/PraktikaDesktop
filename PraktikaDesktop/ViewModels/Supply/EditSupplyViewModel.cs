using PraktikaDesktop.Models;
using PraktikaDesktop.ViewModels.Dialog;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace PraktikaDesktop.ViewModels
{
    public class EditSupplyViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;
        private ObservableCollection<SupplyProduct> _allSupplyProducts;
        private List<SupplyProduct> _newSupplyProducts = new();
        private List<SupplyProduct> _removedSupplyProducts = new();

        private Supply _editabledSupply;
        #endregion Fields

        #region Properties
        public readonly SupplyViewModel SupplyViewModel;

        public ObservableCollection<SupplyProduct> AllSupplyProducts
        {
            get => _allSupplyProducts;
            set => this.RaiseAndSetIfChanged(ref _allSupplyProducts, value);
        }

        public Supply EditabledSupply
        {
            get => _editabledSupply;
            set
            {
                this.RaiseAndSetIfChanged(ref _editabledSupply, value);
            }
        }
        #endregion Properties

        #region Command
        public void ClickSaveCommand()
        {
            _response = ApiRequest.Put("Supply/UpdateSupply" + $"/{SupplyViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId}", EditabledSupply);

            foreach(SupplyProduct removedSupplyProduct in _removedSupplyProducts)
                _response = ApiRequest.Delete("Supply/DeleteSupplyProduct" + removedSupplyProduct.SupplyProductsId + $"/{SupplyViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId}");

            foreach (SupplyProduct newSupplyProduct in _newSupplyProducts)
            {
                newSupplyProduct.SupplyId = EditabledSupply.SupplyId;
                newSupplyProduct.ProductId = newSupplyProduct.Product.ProductId;
                newSupplyProduct.Product = null;
                
                if(newSupplyProduct.Textile != null)
                {
                    newSupplyProduct.TextileId = newSupplyProduct.Textile.TextileId;
                    newSupplyProduct.Textile = null;
                }

                _response = ApiRequest.Post("Supply/InsertSupplyProduct" + $"/{SupplyViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId}", newSupplyProduct);
            }

            SupplyViewModel.GoBackCommand(false);
            SupplyViewModel.RefreshList(EditabledSupply.SupplyId);
        }

        public async void EditSupplyProductsCommand()
        {
            SelectProductsToSupplyDialogViewModel dialogView = new(this, AllSupplyProducts);
            await SupplyViewModel.CurrentMainWindowViewModel.ShowDialog(dialogView);
        }
        #endregion Command

        //Constructor
        public EditSupplyViewModel(SupplyViewModel supplyViewModel)
        {
            SupplyViewModel = supplyViewModel;
            _response = ApiRequest.Get("Supply/GetSupplyById/" + SupplyViewModel.SelectedSupply.SupplyId);
            EditabledSupply = _response.Content.ReadAsAsync<Supply>().Result;
            AllSupplyProducts = new ObservableCollection<SupplyProduct>(EditabledSupply.SupplyProducts);
        }

        //Methods
        public void UpdateSupplyProducts(ObservableCollection<SupplyProduct> allSupplyProducts, List<SupplyProduct> newSupplyProducts, List<SupplyProduct> removedSupplyProducts)
        {
            AllSupplyProducts = null;
            AllSupplyProducts = allSupplyProducts;
            _newSupplyProducts = newSupplyProducts;
            _removedSupplyProducts = removedSupplyProducts;
        }
    }
}
