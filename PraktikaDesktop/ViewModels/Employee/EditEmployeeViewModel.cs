using PraktikaDesktop.Models;
using PraktikaDesktop.ViewModels.Dialog;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace PraktikaDesktop.ViewModels
{
    public class EditEmployeeViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;

        private Employee _editableEmployee = new();
        private List<Role> _rolesList;
        private Role _selectedRole;

        private string _newPassword = "";
        #endregion Fields

        #region Properties
        private readonly EmployeeViewModel EmployeeViewModel;

        public Employee EditableEmployee
        {
            get => _editableEmployee;
            set => this.RaiseAndSetIfChanged(ref _editableEmployee, value);
        }
        public List<Role> RolesList
        {
            get => _rolesList;
            set => this.RaiseAndSetIfChanged(ref _rolesList, value);
        }
        public Role SelectedRole
        {
            get => _selectedRole;
            set => this.RaiseAndSetIfChanged(ref _selectedRole, value);
        }

        public string NewPassword
        {
            get => _newPassword;
            set => this.RaiseAndSetIfChanged(ref _newPassword, value);
        }
        #endregion Properties


        #region Command
        public async void ClickSaveCommand()
        {
            if (EditableEmployee.FullName.Length == 0 || EditableEmployee.Login.Length == 0 || EditableEmployee.Role == null)
            {
                InformationDialogViewModel informationDialogViewModel = new(EmployeeViewModel.CurrentMainWindowViewModel, "Не все поля заполнены");
                await EmployeeViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
            }
            else
            {
                if(NewPassword.Length != 0)
                    EditableEmployee.Password = NewPassword;

                EditableEmployee.RoleId = SelectedRole.RoleId;
                EditableEmployee.Role = null;

                _response = ApiRequest.Put("Employee/UpdateEmployee" + $"/{EmployeeViewModel.CurrentMainWindowViewModel.LoginEmployee.EmployeeId}", EditableEmployee);

                EmployeeViewModel.GoBackCommand(false);
                EmployeeViewModel.RefreshList(EditableEmployee.EmployeeId);
            }
        }
        #endregion Command

        //Constructor
        public EditEmployeeViewModel(EmployeeViewModel employeeViewModel)
        {
            EmployeeViewModel = employeeViewModel;
            EditableEmployee = EmployeeViewModel.SelectedEmployee;

            _response = ApiRequest.Get("Employee/GetRoles");
            RolesList = _response.Content.ReadAsAsync<List<Role>>().Result;
            SelectedRole = RolesList.Where(r => r.RoleId == EditableEmployee.RoleId).First();
        }
    }
}
