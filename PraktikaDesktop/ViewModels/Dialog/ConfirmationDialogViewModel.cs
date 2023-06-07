using System.Windows.Input;

namespace PraktikaDesktop.ViewModels.Dialog
{
    public class ConfirmationDialogViewModel : ViewModelBase
    {
        //Fields
        MainWindowViewModel _mainWindowViewModel;
        //Commands execution
        private void ClickYesCommand()
        {
            _mainWindowViewModel.DialogResult = true;
            _mainWindowViewModel.CloseDialog();
        }
        private void ClickNoCommand()
        {
            _mainWindowViewModel.DialogResult = false;
            _mainWindowViewModel.CloseDialog();
        }

        //Constructor
        public ConfirmationDialogViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }
    }
}
