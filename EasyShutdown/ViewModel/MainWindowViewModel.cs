using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using EasyShutdown.Properties;
using EasyShutdown.View;
using EasyShutdown.Scheduler;

namespace EasyShutdown.ViewModel
{
    class MainWindowViewModel : BaseViewModal
    {
        private const int TIMEOUT = 10;

        public MainWindowViewModel(Window view)
            : base(view)
        {
            ExitCommand = new DelegateCommand(Exit);
            CheckFullScreenModeCommand = new DelegateCommand(CheckFullScreenMode);
            MoveWindowCommand = new DelegateCommand(MoveWindow);
            RestoreWindowPositionCommand = new DelegateCommand(RestoreWindowPosition);
            SaveWindowPositionCommand = new DelegateCommand(SaveWindowPosition);
            ShowHideMenuCommand = new DelegateCommand<Border>(ShowHideMenu);
            LogoffCommand = new DelegateCommand(Logoff);
            RestartCommand = new DelegateCommand(Restart);
            ShutdownCommand = new DelegateCommand(Shutdown);
            OpenSchedulerCommand = new DelegateCommand(OpenScheduler);
        }

        public string Username
        {
            get { return string.Format(@"{0}\{1}", Environment.UserDomainName, Environment.UserName); }
        }

        public ICommand LogoffCommand { get; private set; }

        public ICommand RestartCommand { get; private set; }

        public ICommand ShutdownCommand { get; private set; }

        public ICommand ShowHideMenuCommand { get; private set; }

        public ICommand OpenSchedulerCommand { get; private set; }

        public ICommand ExitCommand { get; private set; }

        public ICommand MoveWindowCommand { get; private set; }

        public ICommand SaveWindowPositionCommand { get; private set; }

        public ICommand RestoreWindowPositionCommand { get; private set; }

        public ICommand CheckFullScreenModeCommand { get; private set; }

        private void OpenScheduler()
        {
            SchedulerDialog view = new SchedulerDialog();
            SchedulerViewModel viewModel = new SchedulerViewModel(view, new SchedulerSettingsManager());
            view.DataContext = viewModel;
            view.ShowDialog();
        }

        private void ShowHideMenu(Border menu)
        {
            ValidateState();

            menu.Visibility = menu.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CheckFullScreenMode()
        {
            ValidateState();

            if (WindowsAPI.IsFullScreenMode())
            {
                View.Hide();
            }
            else
            {
                View.Show();
            }
        }

        private void SaveWindowPosition()
        {
            ValidateState();

            Settings.Default.WindowLeft = View.Left;
            Settings.Default.WindowTop = View.Top;
            Settings.Default.Save();
        }

        private void RestoreWindowPosition()
        {
            ValidateState();

            double left = Settings.Default.WindowLeft;
            double top = Settings.Default.WindowTop;
            if (left > 0 && top > 0)
            {
                View.Left = left;
                View.Top = top;
            }
        }

        private void MoveWindow()
        {
            ValidateState();

            View.DragMove();
        }

        private void Exit()
        {
            ValidateState();

            Application.Current.Shutdown();
        }

        private void Logoff()
        {
            ValidateState();
            SaveWindowPosition();

            MessageBoxResult answer = AutoConfirmDialog.Show("Do you want to log off?", "EasyShutdown", "Log off", TIMEOUT);
            if (answer == MessageBoxResult.Yes)
            {
                WindowsAPI.Logoff();
            }
        }

        private void Restart()
        {
            ValidateState();
            SaveWindowPosition();

            MessageBoxResult answer = AutoConfirmDialog.Show("Do you want to restart your computer?", "EasyShutdown", "Restart", TIMEOUT);
            if (answer == MessageBoxResult.Yes)
            {
                WindowsAPI.Restart();
            }
        }

        private void Shutdown()
        {
            ValidateState();
            SaveWindowPosition();

            MessageBoxResult answer = AutoConfirmDialog.Show("Do you want to shut down your computer?", "EasyShutdown", "Shut down", TIMEOUT);
            if (answer == MessageBoxResult.Yes)
            {
                WindowsAPI.Shutdown();
            }
        }
    }
}
