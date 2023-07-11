using Avalonia.Metadata;
using PraktikaDesktop.Models;
using PraktikaDesktop.ViewModels.Dialog;
using PraktikaDesktop.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;

namespace PraktikaDesktop.ViewModels
{
    public class OrderViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;

        private readonly ObservableAsPropertyHelper<bool> _supplysEmpty;
        private ObservableCollection<Order> _displayedOrders;
        private List<Order> _allOrders;

        //Sort
        private double _sortingHeight = 45;

        private string _searchString;
        private List<Order>? _searchOrders;

        private DateTime? _sortFirstDate = null;
        private DateTime? _sortSecondDate = null;
        private List<Order>? _sortByDateOrders;

        private List<string> _sortStatusList = new List<string> { "Сформирован", "Оплачен", "Выполнен" };
        private string _selectedStatus;
        private List<Order>? _sortByStatusOrders;

        //Selected order
        private bool _supplyIsSelected = false;
        private Order? _selectedOrder;

        private ViewModelBase _currentChildView;
        private bool _animationAction = false;
        #endregion Fields

        #region Properties
        public readonly MainWindowViewModel CurrentMainWindowViewModel;

        public bool OrdersEmpty => _supplysEmpty.Value;
        public ObservableCollection<Order> DisplayedOrders
        {
            get => _displayedOrders;
            set
            {
                this.RaiseAndSetIfChanged(ref _displayedOrders, value);
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

        public DateTime? SortFirstDate
        {
            get => _sortFirstDate;
            set => this.RaiseAndSetIfChanged(ref _sortFirstDate, value);
        }
        public DateTime? SortSecondDate
        {
            get => _sortSecondDate;
            set => this.RaiseAndSetIfChanged(ref _sortSecondDate, value);
        }

        public List<string> SortStatusList
        {
            get => _sortStatusList;
            set => this.RaiseAndSetIfChanged(ref _sortStatusList, value);
        }
        public string? SelectedStatus
        {
            get => _selectedStatus;
            set => this.RaiseAndSetIfChanged(ref _selectedStatus, value);
        }

        //Selected order
        public bool OrderIsSelected
        {
            get => _supplyIsSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _supplyIsSelected, value);
            }
        }
        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedOrder, value);
                OrderIsSelected = SelectedOrder == null ? false : true;
            }
        }

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

        #region Command
        public void EditCommand()
        {
            EditOrderViewModel editOrderViewModel = new EditOrderViewModel(this);
            CurrentChildView = editOrderViewModel;
            AnimationAction = true;
        }
        [DependsOn(nameof(SelectedOrder)), DependsOn(nameof(AnimationAction))]
        bool CanEditCommand(object? obj)
        {
            if (SelectedOrder != null && !AnimationAction)
                return true;
            else
                return false;
        }

        public void AddCommand()
        {
            AddOrderViewModel addOrderViewModel = new AddOrderViewModel(this);
            CurrentChildView = addOrderViewModel;
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

        public async void DeleteOrderCommand(Order deletedOrder)
        {
            ConfirmationDialogViewModel confirmationDialogViewModel = new ConfirmationDialogViewModel(CurrentMainWindowViewModel, "Связанные данные будут удалены");
            bool result = await CurrentMainWindowViewModel.ShowDialog(confirmationDialogViewModel);

            if (result)
            {
                _response = ApiRequest.Delete($"Order/DeleteOrder/{deletedOrder.OrderId}/{CurrentMainWindowViewModel.LoginEmployee.EmployeeId}");

                if (_response.IsSuccessStatusCode)
                {
                    if (SelectedOrder == deletedOrder)
                        SelectedOrder = null;
                    DisplayedOrders.Remove(deletedOrder);
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
            _searchOrders = null;
            Merger();
        }
        public void ClearSortByDateCommand()
        {
            SortFirstDate = null;
            SortSecondDate = null;
            _sortByDateOrders = null;
            Merger();
        }
        public void ClearSortByStatusCommand()
        {
            SelectedStatus = null;
            _sortByStatusOrders = null;
            Merger();
        }
        #endregion Command

        //Constructor
        public OrderViewModel(MainWindowViewModel mainWindowViewModel)
        {
            CurrentMainWindowViewModel = mainWindowViewModel;
            RefreshList(null);

            _supplysEmpty = this.WhenAnyValue(vm => vm.DisplayedOrders.Count)
                .Select(t => t == 0 ? true : false)
                .ToProperty(this, vm => vm.OrdersEmpty);

            this.WhenAnyValue(vm => vm.SearchString).Subscribe(_ => Search());
            this.WhenAnyValue(vm => vm.SortFirstDate, vm => vm.SortSecondDate).Subscribe(_ => SortByDate());
            this.WhenAnyValue(vm => vm.SelectedStatus).Subscribe(_ => SortByStatus());
        }

        //Methods
        public void RefreshList(int? selectedItemId)
        {
            _response = ApiRequest.Get("Order/GetOrders");
            DisplayedOrders = _response.Content.ReadAsAsync<ObservableCollection<Order>>().Result;

            _response = ApiRequest.Get("Order/GetOrders");
            _allOrders = _response.Content.ReadAsAsync<List<Order>>().Result;

            if (selectedItemId != null)
                SelectedOrder = DisplayedOrders.Where(s => s.OrderId == selectedItemId).First();
        }

        private void Search()
        {
            if (SearchString != "" && SearchString != null)
                _searchOrders = _allOrders.Where(s => s.Date.ToShortDateString().Contains(SearchString.ToLower()) | s.OrderId.ToString().Contains(SearchString.ToLower())).ToList();
            else
                _searchOrders = null;

            Merger();
        }
        private void SortByDate()
        {
            if (SortFirstDate != null && SortSecondDate != null)
                _sortByDateOrders = _allOrders.Where(s => s.Date >= SortFirstDate & s.Date <= SortSecondDate).ToList();
            Merger();
        }
        private void SortByStatus()
        {
            if (SelectedStatus != null)
                _sortByStatusOrders = _allOrders.Where(s => s.Status == SelectedStatus).ToList();
            Merger();

        }
        private void Merger()
        {
            DisplayedOrders = new ObservableCollection<Order>(_allOrders);

            if (_searchOrders != null && _sortByDateOrders != null && _sortByStatusOrders != null)
            {
                DisplayedOrders = new ObservableCollection<Order>(_searchOrders.Intersect(_sortByDateOrders.Intersect(_sortByStatusOrders)).ToList());
            }

            if (_sortByDateOrders != null)
            {
                DisplayedOrders = new ObservableCollection<Order>(_allOrders.Intersect(_sortByDateOrders).ToList());

                if (_searchOrders != null)
                {
                    DisplayedOrders = new ObservableCollection<Order>(DisplayedOrders.Intersect(_searchOrders).ToList());
                }

                if (_sortByStatusOrders != null)
                {
                    DisplayedOrders = new ObservableCollection<Order>(DisplayedOrders.Intersect(_sortByStatusOrders).ToList());
                }
            }
            else
            {
                if (_searchOrders != null)
                {
                    DisplayedOrders = new ObservableCollection<Order>(DisplayedOrders.Intersect(_searchOrders).ToList());
                }

                if (_sortByStatusOrders != null)
                {
                    DisplayedOrders = new ObservableCollection<Order>(DisplayedOrders.Intersect(_sortByStatusOrders).ToList());
                }
            }

            if (_searchOrders == null && _sortByDateOrders == null && _sortByStatusOrders == null)
            {
                DisplayedOrders = new ObservableCollection<Order>(_allOrders);
            }
        }
    }
}