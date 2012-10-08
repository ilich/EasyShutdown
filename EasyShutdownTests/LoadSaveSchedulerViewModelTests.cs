using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using EasyShutdown.ViewModel;
using System.Windows;
using Moq;
using EasyShutdown.Scheduler;
using System.Threading;

namespace EasyShutdown.Tests
{
    [TestFixture]
    public class LoadSaveSchedulerViewModelTests
    {
        Mock<ISchedulerSettingsManager> mock;

        [SetUp]
        public void Setup()
        {
            mock = new Mock<ISchedulerSettingsManager>();
        }

        [Test]
        public void TestIfCanSaveNoSchedule()
        {
            SchedulerViewModel viewModel = new SchedulerViewModel(mock.Object) { EnableScheduler = false };
            bool canSave = viewModel.SaveCommand.CanExecute();
            Assert.IsTrue(canSave);
        }

        [Test]
        public void TestIfCanRunEveryDay()
        {
            SchedulerViewModel viewModel = new SchedulerViewModel(mock.Object) { EnableScheduler = true };
            bool canSave = viewModel.SaveCommand.CanExecute();
            Assert.IsFalse(canSave);

            viewModel.Action = SchedulerViewModel.LOGOUT;
            viewModel.Type = SchedulerViewModel.DAILY;
            canSave = viewModel.SaveCommand.CanExecute();
            Assert.IsFalse(canSave);

            viewModel.SelectedTime = DateTime.Now;
            canSave = viewModel.SaveCommand.CanExecute();
            Assert.IsTrue(canSave);
        }

        [Test]
        public void TestIfCanRunEveryMonth()
        {
            Mock<ISchedulerSettingsManager> mock = new Mock<ISchedulerSettingsManager>();
            SchedulerViewModel viewModel = new SchedulerViewModel(mock.Object)
            { 
                EnableScheduler = true ,
                Action = SchedulerViewModel.RESTART,
                Type = SchedulerViewModel.MONTHLY
            };

            bool canSave = viewModel.SaveCommand.CanExecute();
            Assert.IsFalse(canSave);

            viewModel.SelectedDayOfMonth = 150;
            viewModel.SelectedTime = DateTime.Now;
            canSave = viewModel.SaveCommand.CanExecute();
            Assert.IsFalse(canSave);

            viewModel.SelectedDayOfMonth = 0;
            Assert.IsFalse(canSave);

            viewModel.SelectedDayOfMonth = 31;
            canSave = viewModel.SaveCommand.CanExecute();
            Assert.IsTrue(canSave);

            viewModel.SelectedDayOfMonth = 1;
            canSave = viewModel.SaveCommand.CanExecute();
            Assert.IsTrue(canSave);
        }

        [Test]
        public void TestIfCanRunEveryYearOrOnce()
        {
            SchedulerViewModel viewModel = new SchedulerViewModel(mock.Object)
            {
                EnableScheduler = true,
                Action = SchedulerViewModel.RESTART,
                Type = SchedulerViewModel.YEARLY
            };

            bool canSave = viewModel.SaveCommand.CanExecute();
            Assert.IsFalse(canSave);

            viewModel.SelectedDate = DateTime.Now;
            viewModel.SelectedTime = DateTime.Now;
            canSave = viewModel.SaveCommand.CanExecute();
            Assert.IsTrue(canSave);

            viewModel.Type = SchedulerViewModel.ONCE;
            canSave = viewModel.SaveCommand.CanExecute();
            Assert.IsTrue(canSave);
        }

        [Test]
        public void TestNoActionSettings()
        {
            mock.Setup(mgr => mgr.Load()).Returns(
                new SchedulerSettings
                {
                    Type = ScheduleType.None
                });

            SchedulerViewModel viewModel = new SchedulerViewModel(mock.Object);
            Assert.IsFalse(viewModel.EnableScheduler);
        }

        [Test]
        public void TestSaveNoActionSettings()
        {
            SchedulerSettings settings = new SchedulerSettings();
            mock.Setup(mgr => mgr.Save(It.IsAny<SchedulerSettings>())).Callback<SchedulerSettings>(s =>
                {
                    Assert.AreEqual(ScheduleType.None, s.Type);
                });

            SchedulerViewModel viewModel = new SchedulerViewModel(mock.Object);
            viewModel.EnableScheduler = false;
            viewModel.SaveSettings();
        }

        [Test]
        public void TestRestartDailySettings()
        {
            mock.Setup(mgr => mgr.Load()).Returns(
                new SchedulerSettings
                {
                    Type = ScheduleType.Daily,
                    Action = ScheduledAction.Restart,
                    Time = new DateTime(1900, 1, 1, 20, 30, 00),
                    AskToRunAction = true
                });

            SchedulerViewModel viewModel = new SchedulerViewModel(mock.Object);
            Assert.IsTrue(viewModel.EnableScheduler);
            Assert.AreEqual(SchedulerViewModel.RESTART, viewModel.Action);
            Assert.AreEqual(SchedulerViewModel.DAILY, viewModel.Type);
            Assert.AreEqual(new TimeSpan(20, 30, 00), viewModel.SelectedTime.Value.TimeOfDay);
            Assert.IsTrue(viewModel.AskToRunAction);
        }

        [Test]
        public void TestSaveRestartDailySettings()
        {
            SchedulerSettings settings = new SchedulerSettings();
            mock.Setup(mgr => mgr.Save(It.IsAny<SchedulerSettings>())).Callback<SchedulerSettings>(s =>
                {
                    Assert.AreEqual(ScheduleType.Daily, s.Type);
                    Assert.AreEqual(ScheduledAction.Restart, s.Action);
                    Assert.AreEqual(new TimeSpan(20, 30, 00), s.Time.Value.TimeOfDay);
                });

            SchedulerViewModel viewModel = new SchedulerViewModel(mock.Object);
            viewModel.EnableScheduler = true;
            viewModel.Action = SchedulerViewModel.RESTART;
            viewModel.Type = SchedulerViewModel.DAILY;
            viewModel.SelectedTime = new DateTime(1, 1, 1, 20, 30, 40);
            viewModel.SaveSettings();
        }

        [Test]
        public void TestLogOutMonthlySettings()
        {
            mock.Setup(mgr => mgr.Load()).Returns(
                new SchedulerSettings
                {
                    Type = ScheduleType.Monthly,
                    Action = ScheduledAction.LogOut,
                    Time = new DateTime(1900, 1, 1, 20, 30, 00),
                    DayOfMonth = 20
                });

            SchedulerViewModel viewModel = new SchedulerViewModel(mock.Object);
            Assert.IsTrue(viewModel.EnableScheduler);
            Assert.AreEqual(SchedulerViewModel.LOGOUT, viewModel.Action);
            Assert.AreEqual(SchedulerViewModel.MONTHLY, viewModel.Type);
            Assert.AreEqual(new TimeSpan(20, 30, 00), viewModel.SelectedTime.Value.TimeOfDay);
            Assert.AreEqual(20, viewModel.SelectedDayOfMonth);
            Assert.IsFalse(viewModel.AskToRunAction);
        }

        [Test]
        public void TestSaveLogOutMonthlySettings()
        {
            SchedulerSettings settings = new SchedulerSettings();
            mock.Setup(mgr => mgr.Save(It.IsAny<SchedulerSettings>())).Callback<SchedulerSettings>(s =>
            {
                Assert.AreEqual(ScheduleType.Monthly, s.Type);
                Assert.AreEqual(ScheduledAction.LogOut, s.Action);
                Assert.AreEqual(new TimeSpan(20, 30, 00), s.Time.Value.TimeOfDay);
                Assert.AreEqual(20, s.DayOfMonth);
            });

            SchedulerViewModel viewModel = new SchedulerViewModel(mock.Object);
            viewModel.EnableScheduler = true;
            viewModel.Action = SchedulerViewModel.LOGOUT;
            viewModel.Type = SchedulerViewModel.MONTHLY;
            viewModel.SelectedTime = new DateTime(1, 1, 1, 20, 30, 40);
            viewModel.SelectedDayOfMonth = 20;
            viewModel.SaveSettings();
        }

        [Test]
        public void TestShutdownOnceSettings()
        {
            RunYearlyOrRunOnceTest(true);
        }

        [Test]
        public void TestSaveShutdownOnceSettings()
        {
            RunSaveYearlyOrRunOnceSettingsTest(true);
        }

        [Test]
        public void TestShutdownYearlySettings()
        {
            RunYearlyOrRunOnceTest(false);
        }

        [Test]
        public void TestSaveShutdownYearlySettings()
        {
            RunSaveYearlyOrRunOnceSettingsTest(false);
        }

        private void RunYearlyOrRunOnceTest(bool isRunOnce)
        {
            mock.Setup(mgr => mgr.Load()).Returns(
               new SchedulerSettings
               {
                   Type = isRunOnce ? ScheduleType.Once : ScheduleType.Yearly,
                   Action = ScheduledAction.Shutdown,
                   Time = new DateTime(2013, 1, 29, 20, 30, 00),
                   DayOfMonth = 20
               });
            SchedulerViewModel viewModel = new SchedulerViewModel(mock.Object);
            Assert.IsTrue(viewModel.EnableScheduler);
            Assert.AreEqual(SchedulerViewModel.SHUTDOWN, viewModel.Action);
            if (isRunOnce)
            {
                Assert.AreEqual(SchedulerViewModel.ONCE, viewModel.Type);
            }
            else
            {
                Assert.AreEqual(SchedulerViewModel.YEARLY, viewModel.Type);
            }

            Assert.AreEqual(new DateTime(2013, 1, 29), viewModel.SelectedDate.Value.Date);
            Assert.AreEqual(new TimeSpan(20, 30, 00), viewModel.SelectedTime.Value.TimeOfDay);
            Assert.IsFalse(viewModel.AskToRunAction);
        }

        public void RunSaveYearlyOrRunOnceSettingsTest(bool isRunOnce)
        {
            SchedulerSettings settings = new SchedulerSettings();
            mock.Setup(mgr => mgr.Save(It.IsAny<SchedulerSettings>())).Callback<SchedulerSettings>(s =>
            {
                if (isRunOnce)
                {
                    Assert.AreEqual(ScheduleType.Once, s.Type);
                }
                else
                {
                    Assert.AreEqual(ScheduleType.Yearly, s.Type);
                }

                Assert.AreEqual(ScheduledAction.Shutdown, s.Action);
                Assert.AreEqual(new DateTime(2013, 1, 29), s.Time.Value.Date);
                Assert.AreEqual(new TimeSpan(20, 30, 00), s.Time.Value.TimeOfDay);
            });

            SchedulerViewModel viewModel = new SchedulerViewModel(mock.Object);
            viewModel.EnableScheduler = true;
            viewModel.Action = SchedulerViewModel.SHUTDOWN;
            viewModel.Type = isRunOnce ? SchedulerViewModel.ONCE : SchedulerViewModel.YEARLY;
            viewModel.SelectedTime = new DateTime(1, 1, 1, 20, 30, 40);
            viewModel.SelectedDate = new DateTime(2013, 1, 29, 20, 30, 40);
            viewModel.SaveSettings();
        }
    }
}
