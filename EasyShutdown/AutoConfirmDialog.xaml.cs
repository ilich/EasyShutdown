using System;
using System.Windows;
using System.Windows.Threading;

namespace EasyShutdown
{
    partial class AutoConfirmDialog : Window
    {
        private DispatcherTimer timer;

        public AutoConfirmDialogViewModal ViewModal
        {
            get { return (AutoConfirmDialogViewModal)DataContext; }
        }

        private AutoConfirmDialog(string dialogText, 
                                  string caption = null, 
                                  string action = null, 
                                  int seconds = 0)
        {
            if (string.IsNullOrWhiteSpace(dialogText))
            {
                throw new ArgumentException("dialogText cannot be null or white space.");
            }

            InitializeComponent();

            lblDialogText.Content = dialogText;
            Title = caption ?? string.Empty;

            if (seconds > 0)
            {
                lblTimeLeft.Visibility = Visibility.Visible;

                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Tick += new EventHandler(OnTimer);
            }

            DataContext = new AutoConfirmDialogViewModal(this, timer, action, seconds);

            if (seconds > 0)
            {
                timer.Start();
            }

            lblTimeLeft.Content = ViewModal.GetTimerText();
        }

        public static MessageBoxResult Show(string dialogText, 
                                            string caption = null, 
                                            string action = null, 
                                            int seconds = 0)
        {
            AutoConfirmDialog dialog = new AutoConfirmDialog(dialogText, caption, action, seconds);
            if (dialog.ShowDialog() == true)
            {
                return MessageBoxResult.Yes;
            }
            else
            {
                return MessageBoxResult.No;
            }
        }

        private void OnTimer(object sender, EventArgs e)
        {
            ViewModal.TimerCommand.Execute(null);
            lblTimeLeft.Content = ViewModal.GetTimerText();
        }
    }
}
