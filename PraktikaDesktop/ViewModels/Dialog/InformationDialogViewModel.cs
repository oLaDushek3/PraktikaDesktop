using ReactiveUI;

namespace PraktikaDesktop.ViewModels.Dialog
{
    public class InformationDialogViewModel : ViewModelBase
    {
        //Fields
        private readonly MainWindowViewModel _mainWindowViewModel;

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
        //Commands
        private void ClickOkCommand()
        {
            _mainWindowViewModel.CloseDialog(true);
        }

        //Constructor
        public InformationDialogViewModel(MainWindowViewModel mainWindowViewModel, string message)
        {
            Message = $"\n" + message;
            _mainWindowViewModel = mainWindowViewModel;
        }
    }
}