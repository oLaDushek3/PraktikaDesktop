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
    public class EditOrderViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;
        private ObservableCollection<SupplyProduct> _oldSupplyProducts = new();
        private List<SupplyProduct> _newSupplyProducts = new();
        private List<SupplyProduct> _removedSupplyProducts = new();

        private Order _editableOrder = new();

        private decimal _amount;
        private decimal _amountForProducts;
        private decimal? _delivery;
        private decimal? _assembly;

        private Buyer _selectedBuyer;

        public List<string> _statusList = new List<string> { "Сформирован", "Оплачен", "Выполнен" };
        private string _selectedStatus;
        #endregion Fields

        #region Properties
        public readonly OrderViewModel OrderViewModel;

        public ObservableCollection<SupplyProduct> OldSupplyProducts
        {
            get => _oldSupplyProducts;
            set => this.RaiseAndSetIfChanged(ref _oldSupplyProducts, value);
        }

        public Order EditableOrder
        {
            get => _editableOrder;
            set => this.RaiseAndSetIfChanged(ref _editableOrder, value);
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                this.RaiseAndSetIfChanged(ref _amount, value);
            }
        }
        public decimal? Delivery
        {
            get => _delivery;
            set
            {
                Amount -= Convert.ToDecimal(Delivery);
                Amount += Convert.ToDecimal(value);
                this.RaiseAndSetIfChanged(ref _delivery, value);
            }
        }
        public decimal? Assembly
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
            set => this.RaiseAndSetIfChanged(ref _selectedBuyer, value);
        }

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

        #region Command
        public void ClickSaveCommand()
        {
             EditableOrder.Amount = Amount;
            EditableOrder.Delivery = Delivery;
            EditableOrder.Assembly = Assembly;

            EditableOrder.Employee = null;
            EditableOrder.EmployeeId = OrderViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId;

            EditableOrder.Buyer = null;
            EditableOrder.BuyerId = SelectedBuyer.BuyerId;

            EditableOrder.Status = SelectedStatus;
            if (SelectedStatus == "Выполнен")
            {
                foreach (SupplyProduct supplyProduct in OldSupplyProducts)
                {
                    supplyProduct.Status = "Продан";
                    _response = ApiRequest.Put("Supply/UpdateSupplyProduct" + $"/{OrderViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId}", supplyProduct);
                }
            }
            else
            {
                foreach (SupplyProduct supplyProduct in OldSupplyProducts)
                {
                    supplyProduct.Status = "Зарезервирован";
                    _response = ApiRequest.Put("Supply/UpdateSupplyProduct" + $"/{OrderViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId}", supplyProduct);
                }
            }

            foreach (SupplyProduct supplyProduct in _removedSupplyProducts)
            {
                supplyProduct.Status = "В наличии";
                _response = ApiRequest.Put("Supply/UpdateSupplyProduct" + $"/{OrderViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId}", supplyProduct);
                OldSupplyProducts.Remove(supplyProduct);
            }

            EditableOrder.SupplyProducts = OldSupplyProducts.ToList();
            _response = ApiRequest.Put("Order/UpdateOrder" + $"/{OrderViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId}", EditableOrder);

            OrderViewModel.GoBackCommand(false);
            OrderViewModel.RefreshList(EditableOrder.OrderId);
        }

        [DependsOn(nameof(OldSupplyProducts)), DependsOn(nameof(SelectedBuyer))]
        bool CanClickSaveCommand(object? parameter)
        {
            if (OldSupplyProducts.Count != 0 && SelectedBuyer != null)
                return true;
            else
                return false;
        }

        public async void EditSupplyProductsCommand()
        {
            SelectProductToOrderDialogViewModel dialogView = new(this, OldSupplyProducts);
            await OrderViewModel.CurrentMainWindowViewModel.ShowDialog(dialogView);
        }
        public async void SelectBuyerCommand()
        {
            SelectBuyerToSupplyDialogViewModel dialogView = new(this);
            await OrderViewModel.CurrentMainWindowViewModel.ShowDialog(dialogView);
        }
        #endregion Command

        //Constructor
        public EditOrderViewModel(OrderViewModel orderViewModel)
        {
            OrderViewModel = orderViewModel;

            _response = ApiRequest.Get("Order/GetOrderById/" + OrderViewModel.SelectedOrder.OrderId);
            EditableOrder = _response.Content.ReadAsAsync<Order>().Result;
            OldSupplyProducts = new ObservableCollection<SupplyProduct>(EditableOrder.SupplyProducts);

            Amount = EditableOrder.Amount;
            Delivery = EditableOrder.Delivery;
            Assembly = EditableOrder.Assembly;
            foreach (SupplyProduct product in OldSupplyProducts)
                _amountForProducts += ProductPriceConverter.Convert(product);

            SelectedStatus = EditableOrder.Status;

            _response = ApiRequest.Get("Buyer/GetBuyerById/" + EditableOrder.Buyer.BuyerId);
            SelectedBuyer = _response.Content.ReadAsAsync<Buyer>().Result;
        }

        //Methods
        public void UpdateSupplyProducts(ObservableCollection<SupplyProduct> allSupplyProducts, List<SupplyProduct> newSupplyProducts, List<SupplyProduct> removedSupplyProducts)
        {
            OldSupplyProducts = null;
            OldSupplyProducts = new ObservableCollection<SupplyProduct>(allSupplyProducts);
            _newSupplyProducts = new List<SupplyProduct>(newSupplyProducts);
            _removedSupplyProducts = new List<SupplyProduct>(removedSupplyProducts);

            Amount -= _amountForProducts;
            _amountForProducts = 0;
            foreach (SupplyProduct product in allSupplyProducts)
                _amountForProducts += ProductPriceConverter.Convert(product);

            foreach (SupplyProduct product in _removedSupplyProducts)
                _amountForProducts -= ProductPriceConverter.Convert(product);

            Amount += _amountForProducts;
        }
    }
}