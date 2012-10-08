using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyShutdown.Properties;

namespace EasyShutdown.Scheduler
{
    class SchedulerSettingsManager : ISchedulerSettingsManager
    {
        public SchedulerSettings Load()
        {
            Settings profile = Settings.Default;
            SchedulerSettings settings = new SchedulerSettings
            {
                Type = (ScheduleType)profile.ScheduleType,
                Time = profile.ScheduleTime == DateTime.MinValue ? null : (DateTime?)profile.ScheduleTime,
                AskToRunAction = profile.ScheduleAskToRunAction,
                Action = profile.ScheduledAction == 0 ? null : (ScheduledAction?)profile.ScheduledAction,
                DayOfMonth = profile.ScheduleDayOfMonth == 0 ? null : (int?)profile.ScheduleDayOfMonth
            };

            return settings;
        }

        public void Save(SchedulerSettings settings)
        {
            Settings profile = Settings.Default;
            profile.ScheduleType = (int)settings.Type;
            profile.ScheduleTime = settings.Time ?? DateTime.MinValue;
            profile.ScheduleAskToRunAction = settings.AskToRunAction;
            profile.ScheduledAction = settings.Action == null ? 0 : (int)settings.Action;
            profile.ScheduleDayOfMonth = settings.DayOfMonth ?? 0;
        }
    }
}
