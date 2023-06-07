using PraktikaDesktop.Implementation;
using PraktikaDesktop.Interface;
using PraktikaDesktop.Models;
using PraktikaDesktop.ViewModels.Dialog;
using ReactiveUI;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PraktikaDesktop.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        //Fields
        HttpResponseMessage? _response;
        private Employee _loginEmployee;
        private IWindowService? _windowService;

        private ViewModelBase _currentChildView;
        private string _caption;
        private string _icon;

        //Properties
        private Employee LoginEmployee
        {
            get => _loginEmployee;
            set
            {
                this.RaiseAndSetIfChanged(ref _loginEmployee, value);
            }
        }
        public ViewModelBase CurrentChildView
        {
            get => _currentChildView;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentChildView, value);
            }
        }

        public string Caption
        {
            get => _caption;
            set
            {
                this.RaiseAndSetIfChanged(ref _caption, value);
            }
        }
        public string Icon
        {
            get => _icon;
            set
            {
                this.RaiseAndSetIfChanged(ref _icon, value);
            }
        }

        //Command
        private void ExecuteLogoutCommand(object view)
        {
            _windowService = new LoginWindowService();
            _windowService.Show();

            _windowService = new MainWindowService();
            _windowService.Close(view);

        }
        public void ShowSupplyViewCommand()
        {
            SupplyViewModel viewModel = new SupplyViewModel();
            CurrentChildView = viewModel;

            Caption = "Поставки";
            Icon = "Truck";
        }
        public void ShowOrderViewCommand()
        {
            Caption = "Заказы";
            Icon = "Package";
        }
        public void ShowBuyerCommand()
        {
            Caption = "Покупатели";
            Icon = "AccountMultiple";

        }
        public void ShowProductViewCommand()
        {
            Caption = "Продукция";
            Icon = "PackageVariantClosed";
        }
        public void ShowTextileViewCommand()
        {
            Caption = "Ткани";
            Icon = "PaletteSwatch";

        }
        public void ShowColorViewCommand()
        {
            Caption = "Цвета";
            Icon = "Palette";
        }
        public async void ShowEmployeeViewCommand()
        {
            Caption = "Сотрудники";
            Icon = "Users";

            ConfirmationDialogViewModel viewModel = new ConfirmationDialogViewModel(this);
            bool result = await ShowDialog(viewModel);
            if(result)
                result = false;
        }

        //Constructor
        public MainWindowViewModel()
        {
            //Design.DataContext error
            //Enable on at work
            //LoadUser();
            ShowSupplyViewCommand();
        }
        private void LoadUser()
        {
            _response = ApiRequest.Get($"Employee/GetEmployeeByLogin/{Thread.CurrentPrincipal.Identity.Name}");
            LoginEmployee = _response.Content.ReadAsAsync<Employee>().Result;
        }

        #region Dialog
        //Fields
        private ViewModelBase? _dialogView;
        private bool _mainEnable = true;
        private bool _dimmingEffectEnable = false;

        //Properties
        public ViewModelBase? DialogView
        {
            get => _dialogView;
            set
            {
                this.RaiseAndSetIfChanged(ref _dialogView, value);
            }
        }
        public bool MainEnable
        {
            get => _mainEnable;
            set
            {
                this.RaiseAndSetIfChanged(ref _mainEnable, value);
            }
        }
        public bool DimmingEffectEnable
        {
            get => _dimmingEffectEnable;
            set
            {
                this.RaiseAndSetIfChanged(ref _dimmingEffectEnable, value);
            }
        }

        public delegate void CloseDialogDelegate();
        public event CloseDialogDelegate CloseDialogEvent;
        public bool DialogResult;

        //Methods
        public Task<bool> ShowDialog(ViewModelBase currentDialogView)
        {
            BaseDialogViewModel baseDialogViewModel = new(currentDialogView);
            DialogView = baseDialogViewModel;

            MainEnable = false;
            DimmingEffectEnable = true;

            var completion = new TaskCompletionSource<bool>();

            CloseDialogEvent += () => completion.TrySetResult(DialogResult);
            return completion.Task;
        }
        public void CloseDialog()
        {
            DialogView = null;
            MainEnable = true;
            DimmingEffectEnable = false;

            CloseDialogEvent?.Invoke();
        }
        #endregion Dialog
    }
}