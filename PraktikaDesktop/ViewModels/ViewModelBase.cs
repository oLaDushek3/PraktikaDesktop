using ReactiveUI;
using System;

namespace PraktikaDesktop.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public virtual void CloseDialog(bool dialogResult)
        {
            throw new NotImplementedException();
        }
    }
}