using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using EasyShutdown.ViewModel;
using System.Windows;
using EasyShutdown.Scheduler;
using Moq;

namespace EasyShutdown.Tests
{
    [TestFixture]
    public class SchedulerViewModelTests
    {
        private SchedulerViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            Mock<ISchedulerSettingsManager> mock = new Mock<ISchedulerSettingsManager>();
            viewModel = new SchedulerViewModel(mock.Object)
            {
                Action = SchedulerViewModel.LOGOUT,
                Type = SchedulerViewModel.ONCE,
                SelectedTime = DateTime.Now,
                SelectedDate = DateTime.Now
            };
        }

        [Test]
        public void TestRunOnceAction()
        {
            viewModel.Type = SchedulerViewModel.ONCE;
            Assert.IsTrue(viewModel.IsRunOnce);
        }

        [Test]
        public void TestRunEveryDayAction()
        {
            viewModel.Type = SchedulerViewModel.DAILY;
            Assert.IsTrue(viewModel.IsRunEveryDay);
        }

        [Test]
        public void TestRunEveryMonthAction()
        {
            viewModel.Type = SchedulerViewModel.MONTHLY;
            Assert.IsTrue(viewModel.IsRunEveryMonth);
        }

        [Test]
        public void TestRunEveryYearAction()
        {
            viewModel.Type = SchedulerViewModel.YEARLY;
            Assert.IsTrue(viewModel.IsRunEveryYear);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWrongRunPeriod()
        {
            viewModel.Type = "WRONG";
        }

        [Test]
        public void TestResetRunPeriod()
        {
            viewModel.Type = null;
            Assert.IsFalse(viewModel.IsRunOnce);
            Assert.IsFalse(viewModel.IsRunEveryDay);
            Assert.IsFalse(viewModel.IsRunEveryMonth);
            Assert.IsFalse(viewModel.IsRunEveryYear);
        }

        [Test]
        public void TestDisableScheduler()
        {
            viewModel.EnableScheduler = false;
            Assert.IsFalse(viewModel.IsRunOnce);
            Assert.IsFalse(viewModel.IsRunEveryDay);
            Assert.IsFalse(viewModel.IsRunEveryMonth);
            Assert.IsFalse(viewModel.IsRunEveryYear);
            Assert.IsNull(viewModel.SelectedDayOfMonth);
            Assert.IsNull(viewModel.SelectedDate);
            Assert.IsNull(viewModel.SelectedTime);
            Assert.IsNull(viewModel.Type);
            Assert.IsNull(viewModel.Action);
            Assert.IsFalse(viewModel.AskToRunAction);
        }
    }
}
