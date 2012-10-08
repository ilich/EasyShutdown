using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EasyShutdown.Scheduler;

namespace EasyShutdown.ViewModel
{
    class SchedulerViewModel : BaseViewModal
    {
        internal const string LOGOUT = "Log off";

        internal const string RESTART = "Restart";

        internal const string SHUTDOWN = "Shut down";

        internal const string ONCE = "Once";

        internal const string DAILY = "Every day";

        internal const string MONTHLY = "Every month";

        internal const string YEARLY = "Every year";

        private static readonly IEnumerable<string> actions = new[] { LOGOUT, RESTART, SHUTDOWN };

        private static readonly IEnumerable<string> types = new[] { ONCE, DAILY, MONTHLY, YEARLY };

        private static readonly IEnumerable<int> daysOfMonth = Enumerable.Range(1, 31);

        private bool enableScheduler;

        private string action;

        private string type;

        private DateTime? selectedDate;

        private DateTime? selectedTime;

        private int? selectedDayOfMonth;

        private bool isRunOnce;

        private bool isRunEveryDay;

        private bool isRunEveryMonth;

        private bool isRunEveryYear;

        private bool askToRunAction;

        private ISchedulerSettingsManager settingsManager;

        public SchedulerViewModel(Window view, ISchedulerSettingsManager settingsManager)
            : base(view)
        {
            SaveCommand = new DelegateCommand(Save, CanSave);
            CancelCommand = new DelegateCommand(Cancel);

            this.settingsManager = settingsManager;
            LoadSettings();
        }

        internal SchedulerViewModel(ISchedulerSettingsManager settingsManager)
        {
            // Special constructor for unit-tests
            SaveCommand = new DelegateCommand(Save, CanSave);
            CancelCommand = new DelegateCommand(() => { });

            this.settingsManager = settingsManager;
            LoadSettings();
        }

        public bool AskToRunAction
        {
            get { return askToRunAction; }
            set 
            { 
                askToRunAction = value;
                RaisePropertyChanged(() => AskToRunAction);
            }
        }

        public bool IsRunEveryYear
        {
            get { return isRunEveryYear; }
            set 
            { 
                isRunEveryYear = value;
                RaisePropertyChanged(() => IsRunEveryYear);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsRunEveryMonth
        {
            get { return isRunEveryMonth; }
            set 
            { 
                isRunEveryMonth = value;
                RaisePropertyChanged(() => IsRunEveryMonth);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsRunEveryDay
        {
            get { return isRunEveryDay; }
            set 
            { 
                isRunEveryDay = value;
                RaisePropertyChanged(() => IsRunEveryDay);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsRunOnce
        {
            get { return isRunOnce; }
            set 
            { 
                isRunOnce = value;
                RaisePropertyChanged(() => IsRunOnce);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public int? SelectedDayOfMonth
        {
            get { return selectedDayOfMonth; }
            set
            {
                selectedDayOfMonth = value;
                RaisePropertyChanged(() => SelectedDayOfMonth);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public DateTime? SelectedTime
        {
            get { return selectedTime; }
            set 
            { 
                selectedTime = value;
                RaisePropertyChanged(() => SelectedTime);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public DateTime? SelectedDate
        {
            get { return selectedDate; }
            set 
            { 
                selectedDate = value;
                RaisePropertyChanged(() => SelectedDate);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public string Type
        {
            get { return type; }
            set 
            { 
                ResetScheduleType();
                switch (value)
                {
                    case ONCE:
                        IsRunOnce = true;
                        break;

                    case DAILY:
                        IsRunEveryDay = true;
                        break;

                    case MONTHLY:
                        IsRunEveryMonth = true;
                        break;

                    case YEARLY:
                        IsRunEveryYear = true;
                        break;

                    case null:
                        // Special case. Reset value.
                        break;

                    default:
                        throw new ArgumentException();
                }

                type = value;
                RaisePropertyChanged(() => Type);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public string Action
        {
            get { return action; }
            set 
            { 
                action = value;
                RaisePropertyChanged(() => Action);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public bool EnableScheduler
        {
            get { return enableScheduler; }
            set 
            { 
                enableScheduler = value;
                RaisePropertyChanged(() => EnableScheduler);

                if (!value)
                {
                    ResetAll();
                }

                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public IEnumerable<string> Actions
        {
            get { return actions; }
        }

        public IEnumerable<string> Types
        {
            get { return types; }
        }

        public IEnumerable<int> DaysOfMonth
        {
            get { return SchedulerViewModel.daysOfMonth; }
        } 

        public DelegateCommand SaveCommand
        {
            get;
            set;
        }

        public DelegateCommand CancelCommand
        {
            get;
            set;
        }

        private ISchedulerSettingsManager SettingsManager
        {
            get 
            {
                if (settingsManager == null)
                {
                    throw new InvalidOperationException("Scheduler Settings Manager is not set.");
                }

                return settingsManager; 
            }
        }

        internal void SaveSettings()
        {
            // The method is internal to be able to test settings saving without
            // dependencies to Window object

            SchedulerSettings settings = new SchedulerSettings();
            if (!EnableScheduler)
            {
                settings.Type = ScheduleType.None;
                SettingsManager.Save(settings);
                return;
            }

            switch (Action)
            {
                case SHUTDOWN:
                    settings.Action = ScheduledAction.Shutdown;
                    break;

                case LOGOUT:
                    settings.Action = ScheduledAction.LogOut;
                    break;

                case RESTART:
                    settings.Action = ScheduledAction.Restart;
                    break;

                default:
                    throw new InvalidOperationException(string.Format("Unknown Action: {0}", Action));
            }

            if (Type == DAILY)
            {
                settings.Type = ScheduleType.Daily;
                DateTime time = SelectedTime ?? DateTime.MinValue;
                settings.Time = new DateTime(1, 1, 1, time.Hour, time.Minute, 0);
            }
            else if (Type == MONTHLY)
            {
                settings.Type = ScheduleType.Monthly;
                DateTime time = SelectedTime ?? DateTime.MinValue;
                settings.Time = new DateTime(1, 1, 1, time.Hour, time.Minute, 0);
                settings.DayOfMonth = SelectedDayOfMonth ?? 1;
            }
            else if (Type == YEARLY || Type == ONCE)
            {
                settings.Type = Type == YEARLY ? ScheduleType.Yearly : ScheduleType.Once;
                DateTime date = SelectedDate ?? DateTime.MinValue;
                DateTime time = SelectedTime ?? DateTime.MinValue;
                settings.Time = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);
            }
            else
            {
                throw new InvalidOperationException(string.Format("Unknown Schedule Type: {0}", Type));
            }

            settings.AskToRunAction = AskToRunAction;
            SettingsManager.Save(settings);
        }

        private void LoadSettings()
        {
            SchedulerSettings settings = SettingsManager.Load();
            if (settings == null)
            {
                return;
            }

            if (settings.Type == ScheduleType.None)
            {
                EnableScheduler = false;
                return;
            }

            EnableScheduler = true;

            switch (settings.Action)
            {
                case ScheduledAction.LogOut:
                    Action = LOGOUT;
                    break;

                case ScheduledAction.Shutdown:
                    Action = SHUTDOWN;
                    break;

                case ScheduledAction.Restart:
                    Action = RESTART;
                    break;
            }

            if (settings.Type == ScheduleType.Daily)
            {
                Type = DAILY;
                SelectedTime = settings.Time;
            }
            else if (settings.Type == ScheduleType.Monthly)
            {
                Type = MONTHLY;
                SelectedTime = settings.Time;
                SelectedDayOfMonth = settings.DayOfMonth;
            }
            else if (settings.Type == ScheduleType.Yearly || settings.Type == ScheduleType.Once)
            {
                Type = settings.Type == ScheduleType.Yearly ? YEARLY : ONCE;
                SelectedDate = SelectedTime = settings.Time;
            }

            AskToRunAction = settings.AskToRunAction;
        }

        private void Save()
        {
            if (!CanSave())
            {
                return;
            }

            SaveSettings();
            View.Close();
        }

        private bool CanSave()
        {
            if (!EnableScheduler)
            {
                return true;
            }

            if (Type == null || Action == null)
            {
                return false;
            }

            if (IsRunEveryDay)
            {
                return SelectedTime != null;
            }
            else if (IsRunEveryMonth)
            {
                return SelectedDayOfMonth != null &&
                    (SelectedDayOfMonth >= 1 && SelectedDayOfMonth <= 31) && 
                    SelectedTime != null;
            }
            else if (IsRunOnce || IsRunEveryYear)
            {
                return SelectedDate != null && SelectedTime != null;
            }

            return false;
        }

        private void Cancel()
        {
            View.Close();
        }

        private void ResetAll()
        {
            ResetScheduleType();
            SelectedDayOfMonth = null;
            SelectedDate = null;
            SelectedTime = null;
            Type = null;
            Action = null;
            AskToRunAction = false;
        }

        private void ResetScheduleType()
        {
            IsRunOnce = false;
            IsRunEveryDay = false;
            IsRunEveryMonth = false;
            IsRunEveryYear = false;
        }

        private Dictionary<string, string> Validate()
        {
            var errors = new Dictionary<string, string>();
            errors.Add("Type", "ERROR!");
            return errors;
        }
    }
}
