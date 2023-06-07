using ReactiveUI;

namespace PraktikaDesktop.ViewModels.Dialog
{
    public class BaseDialogViewModel : ViewModelBase
    {
        //Fields
        private ViewModelBase _currentDialogView;

        //Properties
        public ViewModelBase CurrentDialogView
        {
            get => _currentDialogView;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentDialogView, value);
            }
        }

        public BaseDialogViewModel(ViewModelBase currentDialogView)
        {
            _currentDialogView = currentDialogView;
        }
    }
}
