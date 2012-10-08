using System;
using System.Windows;
using Microsoft.Practices.Prism.ViewModel;

namespace EasyShutdown.ViewModel
{
    abstract class BaseViewModal : NotificationObject
    {
        public BaseViewModal(Window view)
        {
            View = view;
        }

        internal BaseViewModal()
        {
            // Special constructor for unit-tests
        }

        public Window View { get; set; }

        protected void ValidateState()
        {
            if (View == null)
            {
                throw new InvalidOperationException("View is not set.");
            }
        }
    }
}
