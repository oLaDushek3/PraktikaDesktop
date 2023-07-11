using Avalonia.Metadata;
using DynamicData;
using DynamicData.Binding;
using PraktikaDesktop.Converters;
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
    public class AddOrderViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;
        private ObservableCollection<SupplyProduct> _allSupplyProducts = new();
        private List<SupplyProduct> _newSupplyProducts = new();

        private Order _newOrder = new();

        private decimal _amount;
        private decimal _amountForProducts;
        private decimal _delivery;
        private decimal _assembly;

        private Buyer _selectedBuyer;
        #endregion Fields

        #region Properties
        public readonly OrderViewModel OrderViewModel;

        public ObservableCollection<SupplyProduct> AllSupplyProducts
        {
            get => _allSupplyProducts;
            set => this.RaiseAndSetIfChanged(ref _allSupplyProducts, value);
        }

        public Order NewOrder
        {
            get => _newOrder;
            set
            {
                this.RaiseAndSetIfChanged(ref _newOrder, value);
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                this.RaiseAndSetIfChanged(ref _amount, value);
            }
        }
        public decimal Delivery
        {
            get => _delivery;
            set
            {
                Amount -= Convert.ToDecimal(Delivery);
                Amount += Convert.ToDecimal(value);
                this.RaiseAndSetIfChanged(ref _delivery, value);
            }
        }
        public decimal Assembly
        {
            get => _assembly;
            set
            {
                Amount -= Convert.ToDecimal(Assembly);
                Amount += Convert.ToDecimal(value);
                this.RaiseAndSetIfChanged(ref _assembly, value);
            }
        }

        public Buyer SelectedBuyer
        {
            get => _selectedBuyer;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedBuyer, value);
            }
        }
        #endregion Properties

        #region Command
        public void ClickSaveCommand()
        {
            NewOrder.Amount = Amount;
            NewOrder.Delivery = Delivery;
            NewOrder.Assembly = Assembly;
            NewOrder.Status = "Сформирован";
            NewOrder.EmployeeId = OrderViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId;
            NewOrder.BuyerId = SelectedBuyer.BuyerId;

            foreach(SupplyProduct supplyProduct in _newSupplyProducts)
            {
                supplyProduct.Status = "Зарезервирован";
                _response = ApiRequest.Put("Supply/UpdateSupplyProduct" + $"/{OrderViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId}", supplyProduct);
            }

            NewOrder.SupplyProducts = AllSupplyProducts.ToList();
            _response = ApiRequest.Post("Order/InsertOrder" + $"/{OrderViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId}", NewOrder);

            _response = ApiRequest.Get("Order/GetOrders");
            NewOrder = (_response.Content.ReadAsAsync<List<Order>>().Result).Last();

            OrderViewModel.GoBackCommand(false);
            OrderViewModel.RefreshList(NewOrder.OrderId);
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
            SelectProductToOrderDialogViewModel dialogView = new(this, AllSupplyProducts);
            await OrderViewModel.CurrentMainWindowViewModel.ShowDialog(dialogView);
        }
        public async void SelectBuyerCommand()
        {
            SelectBuyerToSupplyDialogViewModel dialogView = new(this);
            await OrderViewModel.CurrentMainWindowViewModel.ShowDialog(dialogView);
        }
        #endregion Command

        //Constructor
        public AddOrderViewModel(OrderViewModel orderViewModel)
        {
            OrderViewModel = orderViewModel;
        }

        //Methods
        public void UpdateSupplyProducts(ObservableCollection<SupplyProduct> allSupplyProducts, List<SupplyProduct> newSupplyProducts)
        {
            AllSupplyProducts = null;
            AllSupplyProducts = new ObservableCollection<SupplyProduct>(allSupplyProducts);
            _newSupplyProducts = new List<SupplyProduct>(newSupplyProducts);

            Amount -= _amountForProducts;
            foreach (SupplyProduct product in allSupplyProducts)
                _amountForProducts = ProductPriceConverter.Convert(product);

            Amount += _amountForProducts;
        }
    }
}
