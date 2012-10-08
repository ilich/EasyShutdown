using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyShutdown.Scheduler
{
    public class SchedulerSettings
    {
        private int? dayOfMonth;

        public ScheduleType Type
        {
            get;
            set;
        }

        public DateTime? Time
        {
            get;
            set;
        }

        public bool AskToRunAction
        {
            get;
            set;
        }

        public ScheduledAction? Action
        {
            get;
            set;
        }

        public int? DayOfMonth
        {
            get { return dayOfMonth; }
            set
            {
                if (value != null && (value < 1 || value > 31))
                {
                    throw new ArgumentException("value");
                }

                dayOfMonth = value;
            }
        }
    }
}
