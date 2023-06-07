using PraktikaDesktop.Interface;
using PraktikaDesktop.Views;

namespace PraktikaDesktop.Implementation
{
    public class MainWindowService : IWindowService
    {
        public void Close(object window)
        {
            (window as MainWindow).Close();
        }

        public void Show()
        {
            MainWindow newMainWindow = new MainWindow();
            newMainWindow.Show();
        }
    }
}
