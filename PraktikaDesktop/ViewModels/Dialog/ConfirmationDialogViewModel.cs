using ReactiveUI;

namespace PraktikaDesktop.ViewModels.Dialog
{
    public class ConfirmationDialogViewModel : ViewModelBase
    {
        //Fields
        ViewModelBase _mainWindowViewModel;

        private string _message;

        //Properties
        public string Message
        {
            get => _message;
            set
            {
                this.RaiseAndSetIfChanged(ref _message, value);
            }
        }
        //Commands execution
        private void ClickYesCommand()
        {
            _mainWindowViewModel.CloseDialog(true);
        }
        private void ClickNoCommand()
        {
            _mainWindowViewModel.CloseDialog(false);
        }

        //Constructor
        public ConfirmationDialogViewModel(ViewModelBase mainWindowViewModel, string message)
        {
            Message = $"\n" + message;
            _mainWindowViewModel = mainWindowViewModel;
        }
    }
}
