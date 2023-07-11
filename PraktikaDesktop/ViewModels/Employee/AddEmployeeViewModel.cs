using PraktikaDesktop.Models;
using PraktikaDesktop.ViewModels.Dialog;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace PraktikaDesktop.ViewModels
{
    public class AddEmployeeViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;

        private Employee _newEmployee = new();
        private List<Role> _rolesList;
        #endregion Fields

        #region Properties
        private readonly EmployeeViewModel EmployeeViewModel;

        public Employee NewEmployee
        {
            get => _newEmployee;
            set => this.RaiseAndSetIfChanged(ref _newEmployee, value);
        }
        public List<Role> RolesList
        {
            get => _rolesList;
            set => this.RaiseAndSetIfChanged(ref _rolesList, value);
        }
        #endregion Properties


        #region Command
        public async void ClickSaveCommand()
        {
            if(NewEmployee.FullName.Length == 0 || NewEmployee.Login.Length == 0 || NewEmployee.Password.Length == 0 || NewEmployee.Role == null)
            {
                InformationDialogViewModel informationDialogViewModel = new(EmployeeViewModel.CurrentMainWindowViewModel, "Не все поля заполнены");
                await EmployeeViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
            }
            else
            {
                NewEmployee.RoleId = NewEmployee.Role.RoleId;
                NewEmployee.Role = null;

                _response = ApiRequest.Post("Employee/InsertEmployee" + $"/{EmployeeViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId}", NewEmployee);
                _response = ApiRequest.Get("Employee/GetEmployees");
                NewEmployee = (_response.Content.ReadAsAsync<List<Employee>>().Result).Last();

                EmployeeViewModel.GoBackCommand(false);
                EmployeeViewModel.RefreshList(NewEmployee.EmployeeId);
            }
        }
        #endregion Command

        //Constructor
        public AddEmployeeViewModel(EmployeeViewModel employeeViewModel)
        {
            EmployeeViewModel = employeeViewModel;

            _response = ApiRequest.Get("Employee/GetRoles");
            RolesList = _response.Content.ReadAsAsync<List<Role>>().Result;
        }
    }
}
