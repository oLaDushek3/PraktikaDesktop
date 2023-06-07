using Avalonia.Metadata;
using PraktikaDesktop.Implementation;
using PraktikaDesktop.Interface;
using ReactiveUI;
using System;
using System.Net.Http;
using System.Reactive;
using System.Security.Principal;
using System.Threading;

namespace PraktikaDesktop.ViewModels
{
    public class LoginWindowViewModel : ViewModelBase
    {
        //Fields
        private IWindowService? _windowService;

        private string _login = "";
        private string _password = "";
        private string? _errorMessage;
        //Properties
        public string Login
        {
            get => _login;
            set
            {
                this.RaiseAndSetIfChanged(ref _login, value);
            }
        }
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        //Command
        [DependsOn(nameof(Login)), DependsOn(nameof(Password))]
        bool CanLoginCommand(object view)
        {
            bool validData;
            if (Login == null || Password == null)
                validData = false;
            else
                validData = true;

            return validData;
        }
        public async void LoginCommand(object view)
        {
            HttpResponseMessage response = ApiRequest.Get($"Employee/LoginEmployee/{Login}/{Password}");

            if (response.IsSuccessStatusCode)
            {
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(Login), null);

                _windowService = new MainWindowService();
                _windowService.Show();

                _windowService = new LoginWindowService();
                _windowService.Close(view);


            }
            else
                ErrorMessage = (await response.Content.ReadAsStringAsync()).Trim('"');
        }
    }
}