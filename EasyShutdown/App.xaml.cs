using System.Windows;
using EasyShutdown.Scheduler;

namespace EasyShutdown
{
    partial class App : Application
    {
        public JobRunner Scheduler { get; private set; }

        public App()
        {
            Scheduler = new JobRunner(new SchedulerSettingsManager());
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Scheduler.Start();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Scheduler.Stop();
        }
    }
}
