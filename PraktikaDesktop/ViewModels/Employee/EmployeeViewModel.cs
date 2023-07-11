using PraktikaDesktop.Models;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Metadata;
using PraktikaDesktop.ViewModels.Dialog;

namespace PraktikaDesktop.ViewModels
{
    public class EmployeeViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;

        private readonly ObservableAsPropertyHelper<bool> _listEmpty;
        private ObservableCollection<Employee> _displayedEmployees;
        private List<Employee> _allEmployees;

        private string? _searchString;
        private List<Employee>? _searchEmployees;

        private List<Role> _sorRolestList;
        private Role _selectedSortRole;
        private List<Employee> _sortEmployees;

        private bool _employeeIsSelected = false;
        private Employee? _selectedEmployee;

        private ViewModelBase _currentChildView;
        private bool _animationAction = false;
        #endregion Fields

        #region Properties
        public readonly MainWindowViewModel CurrentMainWindowViewModel;

        public bool ListEmpty => _listEmpty.Value;
        public ObservableCollection<Employee> DisplayedEmployees
        {
            get => _displayedEmployees;
            set
            {
                this.RaiseAndSetIfChanged(ref _displayedEmployees, value);
            }
        }

        public string? SearchString
        {
            get => _searchString;
            set => this.RaiseAndSetIfChanged(ref _searchString, value);
        }

        public List<Role> SorRolestList
        {
            get => _sorRolestList;
            set => this.RaiseAndSetIfChanged(ref _sorRolestList, value);
        }
        public Role SelectedSortRole
        {
            get => _selectedSortRole;
            set => this.RaiseAndSetIfChanged(ref _selectedSortRole, value);
        }

        public bool EmployeeIsSelected
        {
            get => _employeeIsSelected;
            set => this.RaiseAndSetIfChanged(ref _employeeIsSelected, value);
        }
        public Employee? SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedEmployee, value);
                EmployeeIsSelected = SelectedEmployee == null ? false : true;
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
            EditEmployeeViewModel editEmployeeViewModel = new EditEmployeeViewModel(this);
            CurrentChildView = editEmployeeViewModel;
            AnimationAction = true;
        }
        [DependsOn(nameof(SelectedEmployee)), DependsOn(nameof(AnimationAction))]
        bool CanEditCommand(object? obj)
        {
            if (SelectedEmployee != null && !AnimationAction)
                return true;
            else
                return false;
        }

        public void AddCommand()
        {
            AddEmployeeViewModel addEmployeeViewModel = new AddEmployeeViewModel(this);
            CurrentChildView = addEmployeeViewModel;
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

        public async void DeleteEmployeeCommand(Employee deletedEmployee)
        {
            if(deletedEmployee.EmployeeId == CurrentMainWindowViewModel.LoginEmployee.EmployeeId)
            {
                InformationDialogViewModel informationDialogViewModel = new InformationDialogViewModel(CurrentMainWindowViewModel, "Вы не можете удалить свою учетную запись");
                await CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
            }
            else
            {
                ConfirmationDialogViewModel confirmationDialogViewModel = new ConfirmationDialogViewModel(CurrentMainWindowViewModel, "Связанные данные будут удалены");
                bool result = await CurrentMainWindowViewModel.ShowDialog(confirmationDialogViewModel);

                if (result)
                {
                    _response = ApiRequest.Delete($"Employee/DeleteEmployee/{deletedEmployee.EmployeeId}/{CurrentMainWindowViewModel.LoginEmployee.EmployeeId}");

                    if (_response.IsSuccessStatusCode)
                    {
                        if (SelectedEmployee == deletedEmployee)
                            SelectedEmployee = null;
                        DisplayedEmployees.Remove(deletedEmployee);
                    }
                    else
                    {
                        InformationDialogViewModel informationDialogViewModel = new(CurrentMainWindowViewModel, "Не удалось выполнить запрос");
                        await CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
                    }
                }
            }
        }

        public void ClearSearchCommand()
        {
            SearchString = null;
            _searchEmployees = null;
            Merger();
        }
        public void ClearSortCommand()
        {
            SelectedSortRole = null;
            _sortEmployees = null;
            Merger();
        }
        #endregion Command

        //Constructor
        public EmployeeViewModel(MainWindowViewModel mainWindowViewModel)
        {
            CurrentMainWindowViewModel = mainWindowViewModel;
            RefreshList(null);

            _response = ApiRequest.Get("Employee/GetRoles");
            SorRolestList = _response.Content.ReadAsAsync<List<Role>>().Result;

            _listEmpty = this.WhenAnyValue(vm => vm.DisplayedEmployees.Count)
                .Select(t => t == 0 ? true : false)
                .ToProperty(this, vm => vm.ListEmpty);

            this.WhenAnyValue(vm => vm.SearchString).Subscribe(_ => Search());
            this.WhenAnyValue(vm => vm.SelectedSortRole).Subscribe(_ => SortByRole());
        }

        //Methods
        public void RefreshList(int? selectedItemId)
        {
            _response = ApiRequest.Get("Employee/GetEmployees");
            DisplayedEmployees = _response.Content.ReadAsAsync<ObservableCollection<Employee>>().Result;

            _response = ApiRequest.Get("Employee/GetEmployees");
            _allEmployees = _response.Content.ReadAsAsync<List<Employee>>().Result;

            if (selectedItemId != null)
                SelectedEmployee = DisplayedEmployees.Where(s => s.EmployeeId == selectedItemId).First();
        }

        private void SortByRole()
        {

            if (SelectedSortRole != null)
                _sortEmployees = _allEmployees.Where(e => e.Role.RoleId == SelectedSortRole.RoleId).ToList();

            Merger();
        }
        private void Search()
        {
            if (SearchString != "" && SearchString != null)
            {
                _searchEmployees = _allEmployees.Where(e => e.Login.ToLower().Contains(SearchString.ToLower()) || e.FullName.ToLower().Contains(SearchString.ToLower())).ToList();
            }
            else
                _searchEmployees = null;

            Merger();
        }

        private void Merger()
        {
            DisplayedEmployees = new ObservableCollection<Employee>(_allEmployees);

            if (_searchEmployees != null && _sortEmployees != null)
            {
                DisplayedEmployees = new ObservableCollection<Employee>(_searchEmployees.Intersect(_sortEmployees).ToList());
            }

            if (_sortEmployees != null)
            {
                DisplayedEmployees = new ObservableCollection<Employee>(_allEmployees.Intersect(_sortEmployees).ToList());

                if (_searchEmployees != null)
                {
                    DisplayedEmployees = new ObservableCollection<Employee>(DisplayedEmployees.Intersect(_searchEmployees).ToList());
                }

                if (_sortEmployees != null)
                {
                    DisplayedEmployees = new ObservableCollection<Employee>(DisplayedEmployees.Intersect(_sortEmployees).ToList());
                }
            }
            else
            {
                if (_searchEmployees != null)
                {
                    DisplayedEmployees = new ObservableCollection<Employee>(DisplayedEmployees.Intersect(_searchEmployees).ToList());
                }

                if (_sortEmployees != null)
                {
                    DisplayedEmployees = new ObservableCollection<Employee>(DisplayedEmployees.Intersect(_sortEmployees).ToList());
                }
            }

            if (_searchEmployees == null && _sortEmployees == null)
            {
                DisplayedEmployees = new ObservableCollection<Employee>(_allEmployees);
            }
        }
    }
}