using PraktikaDesktop.Implementation;
using PraktikaDesktop.Interface;
using PraktikaDesktop.Models;
using PraktikaDesktop.ViewModels.Dialog;
using ReactiveUI;
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

        private bool _admin;

        //Properties
        public Employee LoginEmployee
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

        public bool Admin
        {
            get => _admin;
            set => this.RaiseAndSetIfChanged(ref _admin, value);
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
            SupplyViewModel viewModel = new SupplyViewModel(this);
            CurrentChildView = viewModel;

            Caption = "Поставки";
            Icon = "Truck";
        }
        public void ShowOrderViewCommand()
        {
            OrderViewModel viewModel = new OrderViewModel(this);
            CurrentChildView = viewModel;

            Caption = "Заказы";
            Icon = "Package";
        }
        public void ShowBuyerCommand()
        {
            BuyerViewModel viewModel = new BuyerViewModel(this);
            CurrentChildView = viewModel;

            Caption = "Покупатели";
            Icon = "AccountMultiple";
        }
        public void ShowProductViewCommand()
        {
            ProductViewModel viewModel = new ProductViewModel(this);
            CurrentChildView = viewModel;

            Caption = "Продукция";
            Icon = "PackageVariantClosed";
        }
        public void ShowEmployeeViewCommand()
        {
            EmployeeViewModel viewModel = new EmployeeViewModel(this);
            CurrentChildView = viewModel;

            Caption = "Сотрудники";
            Icon = "Users";
        }
        public void ShowSalesAndRemainsViewCommand()
        {
            SalesAndRemainsViewModel viewModel = new SalesAndRemainsViewModel();
            CurrentChildView = viewModel;

            Caption = "Продажи и остатки";
            Icon = "PercentBox";
        }

        //Constructor
        public MainWindowViewModel()
        {
            LoadUser();
            ShowOrderViewCommand();
        }

        //Methods
        private void LoadUser()
        {
            _response = ApiRequest.Get($"Employee/GetEmployeeByLogin/{Thread.CurrentPrincipal.Identity.Name}");
            LoginEmployee = _response.Content.ReadAsAsync<Employee>().Result;
            if (LoginEmployee.Role.AccessLevel == 1)
                Admin = true;
            else
                Admin = false;
        }

        #region Dialog
        //Dialog fields
        private ViewModelBase? _dialogView;
        private bool _mainEnable = true;
        private bool _dimmingEffectEnable = false;

        //Dialog properties
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

        //Dialog methods
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
        public override void CloseDialog(bool dialogResult)
        {
            DialogResult = dialogResult;
            DialogView = null;
            MainEnable = true;
            DimmingEffectEnable = false;

            CloseDialogEvent?.Invoke();
        }
        #endregion Dialog
    }
}