using System;
using System.Windows;
using System.Windows.Threading;

namespace EasyShutdown
{
    partial class MainWindow : Window
    {
        private DispatcherTimer timer;

        public MainWindowViewModel ViewModal
        {
            get { return (MainWindowViewModel)DataContext; }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel(this);

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += (sender, e) => ViewModal.CheckFullScreenModeCommand.Execute(null);
            timer.Start();
        }
    }
}
