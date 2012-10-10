using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Quartz;
using Quartz.Impl;

namespace EasyShutdown.Scheduler
{
    class JobRunner
    {
        private const string ACTION = "Action";

        private const string CONFIRM = "Confirm";

        private ISchedulerSettingsManager settingsManager;

        private ISchedulerFactory schedulerFactory = new StdSchedulerFactory();

        private IScheduler scheduler;

        public JobRunner(ISchedulerSettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;
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

        public void Start()
        {
            SchedulerSettings settings = SettingsManager.Load();
            if (settings.Type == ScheduleType.None || 
                settings.Action == null ||
                settings.Time == null)
            {
                return;
            }

            
            scheduler = schedulerFactory.GetScheduler();
            scheduler.Start();

            IJobDetail job = CreateJob(settings);
            ITrigger trigger = CreateTrigger(settings);
            scheduler.ScheduleJob(job, trigger);
        }

        public void Stop()
        {
            if (scheduler != null)
            {
                scheduler.Shutdown();
            }
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        private static ITrigger CreateTrigger(SchedulerSettings settings)
        {
            DateTime actionTime = (DateTime)settings.Time;
            DateTime today = DateTime.Now;

            TriggerBuilder builder = TriggerBuilder.Create();
            if (settings.Type == ScheduleType.Daily)
            {
                builder = builder.WithDailyTimeIntervalSchedule(s => s.OnEveryDay()
                                                                      .StartingDailyAt(new TimeOfDay(actionTime.Hour, actionTime.Minute)));
            }
            else if (settings.Type == ScheduleType.Monthly && settings.DayOfMonth != null)
            {
                DateTime monthlyStart = new DateTime(
                    today.Year, 
                    today.Month, 
                    (int)settings.DayOfMonth, 
                    actionTime.Hour, 
                    actionTime.Minute, 
                    actionTime.Second);

                builder = builder.StartAt(monthlyStart.ToUniversalTime())
                                 .WithCalendarIntervalSchedule(s => s.WithIntervalInMonths(1));
            }
            else if (settings.Type == ScheduleType.Yearly)
            {
                builder = builder.StartAt(actionTime.ToUniversalTime())
                                 .WithCalendarIntervalSchedule(s => s.WithIntervalInYears(1));
            }
            else if (settings.Type == ScheduleType.Once)
            {
                builder = builder.StartAt(actionTime.ToUniversalTime());
            }

            ITrigger trigger = builder.Build();
            return trigger;
        }

        private static IJobDetail CreateJob(SchedulerSettings settings)
        {
            IJobDetail job = JobBuilder.Create<ShutdownJob>()
                                       .UsingJobData(ACTION, (int)settings.Action)
                                       .UsingJobData(CONFIRM, settings.AskToRunAction)
                                       .Build();
            return job;
        }

        private class ShutdownJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                JobDataMap param = context.JobDetail.JobDataMap;
                ScheduledAction action = (ScheduledAction)param.GetInt(ACTION);

                Thread thread = new Thread(() =>
                    {
                        bool confirm = param.GetBoolean(CONFIRM);
                        switch (action)
                        {
                            case ScheduledAction.LogOut:
                                WindowsAPI.LogOut(confirm);
                                break;

                            case ScheduledAction.Restart:
                                WindowsAPI.Restart(confirm);
                                break;

                            case ScheduledAction.Shutdown:
                                WindowsAPI.Shutdown(confirm);
                                break;
                        }
                    });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
        }
    }
}
