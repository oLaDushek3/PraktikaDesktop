using PraktikaDesktop.Interface;
using PraktikaDesktop.Views;

namespace PraktikaDesktop.Implementation
{
    public class LoginWindowService : IWindowService
    {
        public void Close(object window)
        {
            (window as LoginWindow).Close();
        }

        public void Show()
        {
            LoginWindow newLoginWindow = new LoginWindow();
            newLoginWindow.Show();
        }
    }
}
