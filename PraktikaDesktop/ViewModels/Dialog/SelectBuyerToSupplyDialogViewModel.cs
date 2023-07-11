using Avalonia.Metadata;
using PraktikaDesktop.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;

namespace PraktikaDesktop.ViewModels.Dialog
{
    public class SelectBuyerToSupplyDialogViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;
        private readonly EditOrderViewModel _editOrderViewModel;
        private readonly AddOrderViewModel _addOrderViewModel;

        private readonly ObservableAsPropertyHelper<bool> _listEmpty;
        private ObservableCollection<Buyer> _displayedBuyers;
        private List<Buyer> _allBuyers;

        private string? _searchString;
        private List<Buyer>? _searchBuyers;

        private List<string> _sorTypestList = new List<string>() { "Физ. лица", "Юр. лица" };
        private string _selectedSortType;
        private List<Buyer> _sortBuyers;

        private bool _buyerIsSelected = false;
        private Buyer? _selectedBuyer;

        private ViewModelBase _currentChildView;
        private bool _animationAction = false;
        #endregion Fields

        #region Properties
        public readonly MainWindowViewModel CurrentMainWindowViewModel;

        public bool ListEmpty => _listEmpty.Value;
        public ObservableCollection<Buyer> DisplayedBuyers
        {
            get => _displayedBuyers;
            set
            {
                this.RaiseAndSetIfChanged(ref _displayedBuyers, value);
            }
        }

        public string? SearchString
        {
            get => _searchString;
            set => this.RaiseAndSetIfChanged(ref _searchString, value);
        }

        public List<string> SorTypestList
        {
            get => _sorTypestList;
            set => this.RaiseAndSetIfChanged(ref _sorTypestList, value);
        }
        public string SelectedSortType
        {
            get => _selectedSortType;
            set => this.RaiseAndSetIfChanged(ref _selectedSortType, value);
        }

        public bool BuyerIsSelected
        {
            get => _buyerIsSelected;
            set => this.RaiseAndSetIfChanged(ref _buyerIsSelected, value);
        }
        public Buyer? SelectedBuyer
        {
            get => _selectedBuyer;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedBuyer, value);
                BuyerIsSelected = SelectedBuyer == null ? false : true;
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
            EditBuyerViewModel editBuyerViewModel = new EditBuyerViewModel(this);
            CurrentChildView = editBuyerViewModel;
            AnimationAction = true;
        }
        [DependsOn(nameof(SelectedBuyer)), DependsOn(nameof(AnimationAction))]
        bool CanEditCommand(object? obj)
        {
            if (SelectedBuyer != null && !AnimationAction)
                return true;
            else
                return false;
        }

        public void AddCommand()
        {
            AddBuyerViewModel addBuyerViewModel = new AddBuyerViewModel(this);
            CurrentChildView = addBuyerViewModel;
            AnimationAction = true;
        }
        [DependsOn(nameof(AnimationAction))]
        bool CanAddCommand(object? obj)
        {
            return !AnimationAction;
        }

        public async void GoBackCommand(bool callConfirmation)
        {
            AnimationAction = false;
        }
        [DependsOn(nameof(AnimationAction))]
        bool CanGoBackCommand(object? obj)
        {
            return AnimationAction;
        }

        public async void DeleteBuyerCommand(Buyer deletedBuyer)
        {
            ConfirmationDialogViewModel confirmationDialogViewModel = new ConfirmationDialogViewModel(CurrentMainWindowViewModel, "Связанные данные будут удалены");
            bool result = await CurrentMainWindowViewModel.ShowDialog(confirmationDialogViewModel);

            if (result)
            {
                _response = ApiRequest.Delete($"Buyer/DeleteBuyer/{deletedBuyer.BuyerId}");

                if (_response.IsSuccessStatusCode)
                {
                    if (SelectedBuyer == deletedBuyer)
                        SelectedBuyer = null;
                    DisplayedBuyers.Remove(deletedBuyer);
                }
                else
                {
                    InformationDialogViewModel informationDialogViewModel = new(CurrentMainWindowViewModel, "Не удалось выполнить запрос");
                    await CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
                }
            }
        }

        public void ClearSearchCommand()
        {
            SearchString = null;
            _searchBuyers = null;
            Merger();
        }

        public void ClickCancelCommand()
        {
            CurrentMainWindowViewModel.CloseDialog(false);
        }
        public void ClickSelectCommand()
        {
            EditBuyerViewModel editBuyerViewModel = new EditBuyerViewModel(this);
            CurrentChildView = editBuyerViewModel;

            if (_addOrderViewModel != null)
                _addOrderViewModel.SelectedBuyer = SelectedBuyer;
            else
                _editOrderViewModel.SelectedBuyer = SelectedBuyer;

            AnimationAction = true;
            CurrentMainWindowViewModel.CloseDialog(true);

        }
        [DependsOn(nameof(SelectedBuyer))]
        bool CanSelectCommand(object? obj)
        {
            if (SelectedBuyer != null)
                return true;
            else
                return false;
        }
        #endregion Command

        //Constructor
        public SelectBuyerToSupplyDialogViewModel(ViewModelBase callerViewModel)
        {
            if (callerViewModel as AddOrderViewModel != null)
            {
                _addOrderViewModel = callerViewModel as AddOrderViewModel;
                CurrentMainWindowViewModel = _addOrderViewModel.OrderViewModel.CurrentMainWindowViewModel;
            }
            else
            {
                _editOrderViewModel = callerViewModel as EditOrderViewModel;
                CurrentMainWindowViewModel = _editOrderViewModel.OrderViewModel.CurrentMainWindowViewModel;
            }
            RefreshList(null);

            _listEmpty = this.WhenAnyValue(vm => vm.DisplayedBuyers.Count)
                .Select(t => t == 0 ? true : false)
                .ToProperty(this, vm => vm.ListEmpty);

            this.WhenAnyValue(vm => vm.SearchString).Subscribe(_ => Search());
            this.WhenAnyValue(vm => vm.SelectedSortType).Subscribe(_ => SortByType());

            SelectedSortType = "Физ. лица";
        }
        //Methods
        public void RefreshList(int? selectedItemId)
        {
            _response = ApiRequest.Get("Buyer/GetBuyers");
            DisplayedBuyers = _response.Content.ReadAsAsync<ObservableCollection<Buyer>>().Result;

            _response = ApiRequest.Get("Buyer/GetBuyers");
            _allBuyers = _response.Content.ReadAsAsync<List<Buyer>>().Result;

            SortByType();

            if (selectedItemId != null)
                SelectedBuyer = DisplayedBuyers.Where(s => s.BuyerId == selectedItemId).First();
        }

        private void SortByType()
        {
            if (SelectedSortType == "Физ. лица")
                _sortBuyers = _allBuyers.Where(b => b.Individual != null).ToList();
            else
                _sortBuyers = _allBuyers.Where(b => b.LegalEntity != null).ToList();

            Merger();
        }
        private void Search()
        {
            if (SearchString != "" && SearchString != null)
            {
                if (SelectedSortType == "Физ. лица")
                    _searchBuyers = _allBuyers.Where(s => s.Individual != null && (s.Address.ToLower().Contains(SearchString.ToLower()) | s.BuyerId.ToString().Contains(SearchString.ToLower()) |
                    s.Individual.FullName.ToLower().Contains(SearchString.ToLower()) | s.Individual.Phone.ToLower().Contains(SearchString.ToLower()) | s.Individual.SeriesPassportNumber.ToLower().Contains(SearchString.ToLower()))).ToList();
                else
                    _searchBuyers = _allBuyers.Where(s => s.LegalEntity != null && (s.Address.Contains(SearchString.ToLower()) | s.BuyerId.ToString().Contains(SearchString.ToLower()) |
                    s.LegalEntity.Organization.ToLower().Contains(SearchString.ToLower()) | s.LegalEntity.Phone.ToLower().Contains(SearchString.ToLower()) | s.LegalEntity.Bank.ToLower().Contains(SearchString.ToLower()) |
                    s.LegalEntity.CheckingAccount.ToLower().Contains(SearchString.ToLower()) | s.LegalEntity.CorrespondentAccount.ToLower().Contains(SearchString.ToLower()) |
                    s.LegalEntity.Bic.ToLower().Contains(SearchString.ToLower()) | s.LegalEntity.Rrc.ToLower().Contains(SearchString.ToLower()) | s.LegalEntity.Tin.ToLower().Contains(SearchString.ToLower()))).ToList();
            }
            else
                _searchBuyers = null;

            Merger();
        }

        private void Merger()
        {
            DisplayedBuyers = new ObservableCollection<Buyer>(_allBuyers);

            if (_searchBuyers != null && _sortBuyers != null)
            {
                DisplayedBuyers = new ObservableCollection<Buyer>(_searchBuyers.Intersect(_sortBuyers).ToList());
            }

            if (_sortBuyers != null)
            {
                DisplayedBuyers = new ObservableCollection<Buyer>(_allBuyers.Intersect(_sortBuyers).ToList());

                if (_searchBuyers != null)
                {
                    DisplayedBuyers = new ObservableCollection<Buyer>(DisplayedBuyers.Intersect(_searchBuyers).ToList());
                }

                if (_sortBuyers != null)
                {
                    DisplayedBuyers = new ObservableCollection<Buyer>(DisplayedBuyers.Intersect(_sortBuyers).ToList());
                }
            }
            else
            {
                if (_searchBuyers != null)
                {
                    DisplayedBuyers = new ObservableCollection<Buyer>(DisplayedBuyers.Intersect(_searchBuyers).ToList());
                }

                if (_sortBuyers != null)
                {
                    DisplayedBuyers = new ObservableCollection<Buyer>(DisplayedBuyers.Intersect(_sortBuyers).ToList());
                }
            }

            if (_searchBuyers == null && _sortBuyers == null)
            {
                DisplayedBuyers = new ObservableCollection<Buyer>(_allBuyers);
            }
        }

    }
}
