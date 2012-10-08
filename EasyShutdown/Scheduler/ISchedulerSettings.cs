using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyShutdown.Scheduler
{
    public interface ISchedulerSettingsManager
    {
        SchedulerSettings Load();

        void Save(SchedulerSettings settings);
    }
}
