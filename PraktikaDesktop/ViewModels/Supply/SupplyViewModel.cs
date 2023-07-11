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
    public class SupplyViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;

        private readonly ObservableAsPropertyHelper<bool> _supplysEmpty;
        private ObservableCollection<Supply> _displayedSupplies;
        private List<Supply> _allSupplies;

        private string _searchString;
        private List<Supply> _searchSupplies;

        private DateTime? _sortFirstDate = null;
        private DateTime? _sortSecondDate = null;
        private List<Supply> _sortSupplies;

        private bool _supplyIsSelected = false;
        private Supply? _selectedSupply;

        private ViewModelBase _currentChildView;
        private bool _animationAction = false;
        #endregion Fields

        #region Properties
        public readonly MainWindowViewModel CurrentMainWindowViewModel;

        public bool SupplysEmpty => _supplysEmpty.Value;
        public ObservableCollection<Supply> DisplayedSupplies
        {
            get => _displayedSupplies;
            set
            {
                this.RaiseAndSetIfChanged(ref _displayedSupplies, value);
            }
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

        public bool SupplyIsSelected
        {
            get => _supplyIsSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _supplyIsSelected, value);
            }
        }
        public Supply? SelectedSupply
        {
            get => _selectedSupply;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedSupply, value);
                SupplyIsSelected = SelectedSupply == null ? false : true;
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
            EditSupplyViewModel editSupplyViewModel = new EditSupplyViewModel(this);
            CurrentChildView = editSupplyViewModel;
            AnimationAction = true;
        }
        [DependsOn(nameof(SelectedSupply)), DependsOn(nameof(AnimationAction))]
        bool CanEditCommand(object? obj)
        {
            if (SelectedSupply != null && !AnimationAction)
                return true;
            else
                return false;
        }

        public void AddCommand()
        {
            AddSupplyViewModel addSupplyViewModel = new AddSupplyViewModel(this);
            CurrentChildView = addSupplyViewModel;
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

        public async void DeleteSupplyCommand(Supply deletedSupply)
        {
            List<int> deletedOrder = new();
            foreach(SupplyProduct supplyProduct in deletedSupply.SupplyProducts)
            {
                if(supplyProduct.Orders.Count != 0)
                    deletedOrder.Add(supplyProduct.Orders[0].OrderId);
            }
            if (deletedOrder.Count == 0)
            {
                ConfirmationDialogViewModel confirmationDialogViewModel = new ConfirmationDialogViewModel(CurrentMainWindowViewModel, "Связанные данные будут удалены");
                bool result = await CurrentMainWindowViewModel.ShowDialog(confirmationDialogViewModel);
                if (result)
                {
                    _response = ApiRequest.Delete($"Supply/DeleteSupply/{deletedSupply.SupplyId}/{CurrentMainWindowViewModel.LoginEmployee.EmployeeId}");

                    if (_response.IsSuccessStatusCode)
                    {
                        if (SelectedSupply == deletedSupply)
                            SelectedSupply = null;
                        DisplayedSupplies.Remove(deletedSupply);
                    }
                    else
                    {
                        InformationDialogViewModel informationDialogViewModel = new(CurrentMainWindowViewModel, "Не удалось выполнить запрос");
                        await CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
                    }
                }
            }
            else
            {
                string ordersId = "";
                foreach(int id in deletedOrder)
                    ordersId += id + " ";
                ConfirmationDialogViewModel confirmationDialogViewModel = new ConfirmationDialogViewModel(CurrentMainWindowViewModel, "Запись связана с заказами №" + ordersId);
                bool result = await CurrentMainWindowViewModel.ShowDialog(confirmationDialogViewModel);

                if (result)
                {
                    foreach(int id in deletedOrder)
                        _response = ApiRequest.Delete($"Order/DeleteOrder/{id}/{CurrentMainWindowViewModel.LoginEmployee.EmployeeId}");

                    _response = ApiRequest.Delete($"Supply/DeleteSupply/{deletedSupply.SupplyId}/{CurrentMainWindowViewModel.LoginEmployee.EmployeeId}");

                    if (SelectedSupply == deletedSupply)
                        SelectedSupply = null;
                    DisplayedSupplies.Remove(deletedSupply);
                }
            }
        }

        public void ClearSearchCommand()
        {
            SearchString = null;
            _searchSupplies = null;
            Merger();
        }
        public void ClearSortCommand()
        {
            SortFirstDate = null;
            SortSecondDate = null;
            _sortSupplies = null;
            Merger();
        }
        #endregion Command

        //Constructor
        public SupplyViewModel(MainWindowViewModel mainWindowViewModel)
        {
            CurrentMainWindowViewModel = mainWindowViewModel;
            RefreshList(null);

            _supplysEmpty = this.WhenAnyValue(vm => vm.DisplayedSupplies.Count)
                .Select(t => t == 0 ? true : false)
                .ToProperty(this, vm => vm.SupplysEmpty);

            this.WhenAnyValue(vm => vm.SearchString).Subscribe(_ => Search());
            this.WhenAnyValue(vm => vm.SortFirstDate, vm => vm.SortSecondDate).Subscribe(_ => SortByDate());
        }

        //Methods
        public void RefreshList(int? selectedItemId)
        {
            _response = ApiRequest.Get("Supply/GetSupplys");
            DisplayedSupplies = _response.Content.ReadAsAsync<ObservableCollection<Supply>>().Result;

            _response = ApiRequest.Get("Supply/GetSupplys");
            _allSupplies = _response.Content.ReadAsAsync<List<Supply>>().Result;

            if (selectedItemId != null)
                SelectedSupply = DisplayedSupplies.Where(s => s.SupplyId == selectedItemId).First();
        }

        private void Search()
        {
            if (SearchString != "" && SearchString != null)
                _searchSupplies = _allSupplies.Where(s => s.Date.ToShortDateString().Contains(SearchString.ToLower()) | s.SupplyId.ToString().Contains(SearchString.ToLower())).ToList();
            else
                _searchSupplies = null;

            Merger();
        }
        private void SortByDate()
        {
            if (SortFirstDate != null && SortSecondDate != null)
                _sortSupplies = _allSupplies.Where(s => s.Date >= SortFirstDate & s.Date <= SortSecondDate).ToList();
            Merger();
        }
        private void Merger()
        {
            DisplayedSupplies = new ObservableCollection<Supply>(_allSupplies);

            if (_searchSupplies != null && _sortSupplies != null)
            {
                DisplayedSupplies = new ObservableCollection<Supply>(_searchSupplies.Intersect(_sortSupplies).ToList());
            }

            if (_sortSupplies != null)
            {
                DisplayedSupplies = new ObservableCollection<Supply>(_allSupplies.Intersect(_sortSupplies).ToList());

                if (_searchSupplies != null)
                {
                    DisplayedSupplies = new ObservableCollection<Supply>(DisplayedSupplies.Intersect(_searchSupplies).ToList());
                }

                if (_sortSupplies != null)
                {
                    DisplayedSupplies = new ObservableCollection<Supply>(DisplayedSupplies.Intersect(_sortSupplies).ToList());
                }
            }
            else
            {
                if (_searchSupplies != null)
                {
                    DisplayedSupplies = new ObservableCollection<Supply>(DisplayedSupplies.Intersect(_searchSupplies).ToList());
                }

                if (_sortSupplies != null)
                {
                    DisplayedSupplies = new ObservableCollection<Supply>(DisplayedSupplies.Intersect(_sortSupplies).ToList());
                }
            }

            if (_searchSupplies == null && _sortSupplies == null)
            {
                DisplayedSupplies = new ObservableCollection<Supply>(_allSupplies);
            }
        }
    }
}