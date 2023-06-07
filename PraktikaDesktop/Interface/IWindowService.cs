using Avalonia.Controls;

namespace PraktikaDesktop.Interface
{
    public interface IWindowService
    {
        void Close(object window);
        void Show();
    }
}
