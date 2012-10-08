using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;

namespace EasyShutdown.Scheduler
{
    class JobRunner
    {
        private ISchedulerSettingsManager settingsManager;

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

        public void Run()
        {
            // TODO
        }
    }
}
