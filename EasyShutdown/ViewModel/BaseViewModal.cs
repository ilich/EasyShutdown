using System;
using System.Windows;

namespace EasyShutdown.ViewModel
{
    abstract class BaseViewModal
    {
        public Window View { get; set; }

        public BaseViewModal(Window view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            View = view;
        }

        protected void ValidateState()
        {
            if (View == null)
            {
                throw new NullReferenceException("View is not set.");
            }
        }
    }
}
