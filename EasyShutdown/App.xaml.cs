using System.Windows;
using EasyShutdown.Scheduler;

namespace EasyShutdown
{
    partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            JobRunner jobRunner = new JobRunner(new SchedulerSettingsManager());
            jobRunner.Run();
        }
    }
}
