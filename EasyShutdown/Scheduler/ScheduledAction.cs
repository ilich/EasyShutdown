using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyShutdown.Scheduler
{
    public enum ScheduledAction
    {
        Shutdown = 1,
        Restart,
        LogOut
    }
}
