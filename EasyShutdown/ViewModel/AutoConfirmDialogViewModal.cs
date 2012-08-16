using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;

namespace EasyShutdown.ViewModel
{
    class AutoConfirmDialogViewModal : BaseViewModal
    {
        private string action;

        private int seconds;

        private DispatcherTimer timer;

        public ICommand YesCommand { get; private set; }

        public ICommand NoCommad { get; private set; }

        public ICommand TimerCommand { get; private set; }

        public ICommand ClosingWindowCommand { get; private set; }

        public AutoConfirmDialogViewModal(Window view, 
                                          DispatcherTimer timer, 
                                          string action = null,
                                          int seconds = 0)
            :base(view)
        {
            this.timer = timer;
            this.seconds = seconds;
            this.action = action;

            YesCommand = new DelegateCommand(OnYes);
            NoCommad = new DelegateCommand(OnNo);
            TimerCommand = new DelegateCommand(OnTimer);
            ClosingWindowCommand = new DelegateCommand(OnClosingWindow);
        }
        
        public string GetTimerText()
        {
            if (timer == null || !timer.IsEnabled)
            {
                return string.Empty;
            }
            else if (string.IsNullOrEmpty(action))
            {
                return string.Format("The question will be confirmed in {0} seconds.", seconds);
            }
            else
            {
                return string.Format("{1} will happen in {0} seconds.", seconds, action);
            }
        }

        private void OnYes()
        {
            ValidateState();

            View.DialogResult = true;
            View.Close();
        }

        private void OnNo()
        {
            ValidateState();

            View.DialogResult = false;
            View.Close();
        }

        private void OnTimer()
        {
            if (timer == null || !timer.IsEnabled)
            {
                return;
            }

            ValidateState();

            seconds--;
            if (seconds != 0)
            {
                return;
            }

            timer.Stop();
            View.DialogResult = true;
            View.Close();
        }

        private void OnClosingWindow()
        {
            if (timer != null && timer.IsEnabled)
            {
                timer.Stop();
            }
        }
    }
}
