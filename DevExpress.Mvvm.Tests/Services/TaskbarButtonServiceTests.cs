using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Tests;
using DevExpress.Utils;
using NUnit.Framework;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class TaskbarButtonServiceTests : BaseWpfFixture {
        double clipMarginShift = 0.0;
        bool getThumbnailClipMarginExecute = false;

        protected override void SetUpCore() {
            base.SetUpCore();
            ApplicationJumpListServiceTestsImageSourceHelper.RegisterPackScheme();
            clipMarginShift = 0.0;
            getThumbnailClipMarginExecute = false;
        }
        protected override void TearDownCore() {
            RealWindow.TaskbarItemInfo = null;
            Interaction.GetBehaviors(RealWindow).Clear();
            base.TearDownCore();
        }
        [Test]
        public void SetProgressIndicatorState() {
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            taskbarService.ProgressState = TaskbarItemProgressState.Indeterminate;
            Assert.AreEqual(TaskbarItemProgressState.Indeterminate, RealWindow.TaskbarItemInfo.ProgressState);
            taskbarService.ProgressState = TaskbarItemProgressState.None;
            Assert.AreEqual(TaskbarItemProgressState.None, RealWindow.TaskbarItemInfo.ProgressState);
            taskbarService.ProgressState = TaskbarItemProgressState.Normal;
            Assert.AreEqual(TaskbarItemProgressState.Normal, RealWindow.TaskbarItemInfo.ProgressState);
            taskbarService.ProgressState = TaskbarItemProgressState.Paused;
            Assert.AreEqual(TaskbarItemProgressState.Paused, RealWindow.TaskbarItemInfo.ProgressState);
            taskbarService.ProgressState = TaskbarItemProgressState.Error;
            Assert.AreEqual(TaskbarItemProgressState.Error, RealWindow.TaskbarItemInfo.ProgressState);
        }
        [Test]
        public void SetProgressIndicatorValue() {
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            taskbarService.ProgressValue = 0.0;
            Assert.AreEqual(0, RealWindow.TaskbarItemInfo.ProgressValue);
            taskbarService.ProgressValue = 0.5;
            Assert.AreEqual(0.5, RealWindow.TaskbarItemInfo.ProgressValue);
            taskbarService.ProgressValue = 1.0;
            Assert.AreEqual(1, RealWindow.TaskbarItemInfo.ProgressValue);
        }
        [Test]
        public void AttachToWindowChild() {
            Grid grid = new Grid();
            RealWindow.Content = grid;
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(grid).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            taskbarService.ProgressState = TaskbarItemProgressState.Paused;
            taskbarService.ProgressValue = 0.81;
            Assert.AreEqual(TaskbarItemProgressState.Paused, RealWindow.TaskbarItemInfo.ProgressState);
            Assert.AreEqual(0.81, RealWindow.TaskbarItemInfo.ProgressValue);
        }
        [Test]
        public void LateBoundWindow() {
            Grid grid = new Grid();
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(grid).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            taskbarService.ProgressState = TaskbarItemProgressState.Paused;
            taskbarService.ProgressValue = 0.81;
            RealWindow.Content = grid;
            DispatcherHelper.UpdateLayoutAndDoEvents(RealWindow);
            Assert.AreEqual(TaskbarItemProgressState.Paused, RealWindow.TaskbarItemInfo.ProgressState);
            Assert.AreEqual(0.81, RealWindow.TaskbarItemInfo.ProgressValue);
        }
        [Test]
        public void DoNotResetTaskBarItemInfoProperties() {
            RealWindow.TaskbarItemInfo = new TaskbarItemInfo() { Description = "desc" };
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            taskbarService.ProgressValue = 0.5;
            taskbarService.Description = "desc2";
            Assert.AreEqual("desc2", RealWindow.TaskbarItemInfo.Description);
            Assert.AreEqual(0.5, RealWindow.TaskbarItemInfo.ProgressValue);
        }
        [Test]
        public void AttachServiceToWindowWithTaskbarButtonInfo() {
            ImageSource icon_1 = ApplicationJumpListServiceTestsImageSourceHelper.GetImageSource(AssemblyHelper.GetResourceUri(typeof(TaskbarButtonServiceTests).Assembly, "Mvvm.UI/Tests/Images/demoicon.ico"));
            ImageSource icon_2 = ApplicationJumpListServiceTestsImageSourceHelper.GetImageSource(AssemblyHelper.GetResourceUri(typeof(TaskbarButtonServiceTests).Assembly, "Mvvm.UI/Tests/Images/Code_Central.png"));
            RealWindow.TaskbarItemInfo = new TaskbarItemInfo() {
                ProgressState = TaskbarItemProgressState.Paused,
                ProgressValue = 0.1,
                Description = "desc",
                Overlay = icon_1,
                ThumbButtonInfos = new ThumbButtonInfoCollection { new ThumbButtonInfo() { Description = "thumbButton1" } },
                ThumbnailClipMargin = new Thickness { Left = 100, Top = 105, Right = 130, Bottom = 110 }
            };
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService() {
                ProgressState = TaskbarItemProgressState.Error,
                ProgressValue = 0.2,
                Description = "desc2",
                OverlayIcon = icon_2,
                ThumbButtonInfos = new TaskbarThumbButtonInfoCollection { new TaskbarThumbButtonInfo() { Description = "thumbButton2" } },
                ThumbnailClipMargin = new Thickness { Left = 50, Top = 555, Right = 135, Bottom = 90 }
            };
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            Assert.AreEqual(TaskbarItemProgressState.Error, RealWindow.TaskbarItemInfo.ProgressState);
            Assert.AreEqual(0.2, RealWindow.TaskbarItemInfo.ProgressValue);
            Assert.AreEqual("desc2", RealWindow.TaskbarItemInfo.Description);
            AssertHelper.AssertEnumerablesAreEqual(new ThumbButtonInfo[] {
                           new ThumbButtonInfo() { Description = "thumbButton2" },
                       }, RealWindow.TaskbarItemInfo.ThumbButtonInfos, true, new string[] { "Command" });

            Assert.AreEqual(new Thickness { Left = 50, Top = 555, Right = 135, Bottom = 90 }, RealWindow.TaskbarItemInfo.ThumbnailClipMargin);

        }
        [Test]
        public void UpdateServicePropertiesOnWindowTaskbarButtonInfoChanged() {
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            taskbarServiceImpl.Description = "new desc";
            taskbarServiceImpl.ProgressValue = 0.5;
            RealWindow.TaskbarItemInfo = null;
            Assert.IsTrue(string.IsNullOrEmpty(taskbarServiceImpl.Description));
        }
        [Test]
        public void ExplicitWindow() {
            var testWindow = new Window();
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            taskbarServiceImpl.Window = testWindow;
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            taskbarService.ProgressValue = 0.5;
            Assert.IsNull(RealWindow.TaskbarItemInfo);
            Assert.AreEqual(0.5, testWindow.TaskbarItemInfo.ProgressValue);
        }
        [Test]
        public void RemoveAssociatedObjectFromTree_CheckWindowIsNull() {
            Grid grid = new Grid();
            RealWindow.Content = grid;
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(grid).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            Assert.AreEqual(RealWindow, taskbarServiceImpl.ActualWindow);
            RealWindow.Content = null;
            DispatcherHelper.UpdateLayoutAndDoEvents(RealWindow);
            Assert.IsNull(taskbarServiceImpl.ActualWindow);
        }
        [Test]
        public void TryUsingServiceWithoutWindow() {
            Grid grid = new Grid();
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(grid).Add(taskbarServiceImpl);
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            taskbarService.ProgressState = TaskbarItemProgressState.Paused;
            taskbarService.ProgressValue = 0.81;
        }
        [Test]
        public void SeveralServices() {
            TaskbarButtonService taskbarService_1 = new TaskbarButtonService();
            TaskbarButtonService taskbarService_2 = new TaskbarButtonService();
            taskbarService_1.Window = RealWindow;
            taskbarService_2.Window = RealWindow;
            EnqueueShowRealWindow();
            taskbarService_1.ProgressValue = 0.5;
            Assert.AreEqual(0.5, taskbarService_2.ProgressValue);
            taskbarService_2.ProgressValue = 0.8;
            Assert.AreEqual(0.8, taskbarService_1.ProgressValue);
            taskbarService_1.ProgressState = TaskbarItemProgressState.Normal;
            Assert.AreEqual(TaskbarItemProgressState.Normal, taskbarService_2.ProgressState);
            taskbarService_2.ProgressState = TaskbarItemProgressState.Paused;
            Assert.AreEqual(TaskbarItemProgressState.Paused, taskbarService_1.ProgressState);
        }
        [Test]
        public void SetOverlayIcon() {
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            ImageSource icon_1 = ApplicationJumpListServiceTestsImageSourceHelper.GetImageSource(AssemblyHelper.GetResourceUri(typeof(TaskbarButtonServiceTests).Assembly, "Mvvm.UI/Tests/Images/demoicon.ico"));
            ImageSource icon_2 = ApplicationJumpListServiceTestsImageSourceHelper.GetImageSource(AssemblyHelper.GetResourceUri(typeof(TaskbarButtonServiceTests).Assembly, "Mvvm.UI/Tests/Images/Code_Central.png"));
            taskbarService.OverlayIcon = icon_1;
            Assert.AreEqual(icon_1, RealWindow.TaskbarItemInfo.Overlay);
            taskbarService.OverlayIcon = icon_2;
            Assert.AreEqual(icon_2, RealWindow.TaskbarItemInfo.Overlay);
        }
        [Test]
        public void SetDescription() {
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            taskbarService.Description = "test1";
            Assert.AreEqual("test1", RealWindow.TaskbarItemInfo.Description);
            taskbarService.Description = "test2";
            Assert.AreEqual("test2", RealWindow.TaskbarItemInfo.Description);
        }
        [Test]
        public void SetThumbButtonInfos() {
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            TaskbarThumbButtonInfo thumbButtonInfo_1 = new TaskbarThumbButtonInfo() { Description = "thumbButton1" };
            TaskbarThumbButtonInfo thumbButtonInfo_2 = new TaskbarThumbButtonInfo() { Description = "thumbButton2" };
            taskbarService.ThumbButtonInfos.Add(thumbButtonInfo_1);
            taskbarService.ThumbButtonInfos.Add(thumbButtonInfo_2);
            AssertHelper.AssertEnumerablesAreEqual(new ThumbButtonInfo[] {
                new ThumbButtonInfo() { Description = "thumbButton1" },
                new ThumbButtonInfo() { Description = "thumbButton2" },
            }, RealWindow.TaskbarItemInfo.ThumbButtonInfos, true, new string[] { "Command" });
        }
        [Test]
        public void TaskbarThumbButtonPropertiesChanged() {
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ImageSource imageSource1 = ApplicationJumpListServiceTestsImageSourceHelper.GetImageSource(AssemblyHelper.GetResourceUri(typeof(TaskbarButtonServiceTests).Assembly, "Mvvm.UI/Tests/Images/demoicon.ico"));
            ImageSource imageSource2 = ApplicationJumpListServiceTestsImageSourceHelper.GetImageSource(AssemblyHelper.GetResourceUri(typeof(TaskbarButtonServiceTests).Assembly, "Mvvm.UI/Tests/Images/Code_Central.png"));
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            TaskbarThumbButtonInfo thumbButtonInfo_1 = new TaskbarThumbButtonInfo() {
                Description = "thumbButton1",
                IsEnabled = true,
                IsInteractive = true,
                IsBackgroundVisible = false,
                DismissWhenClicked = false,
                Visibility = Visibility.Visible,
                ImageSource = imageSource1
            };
            taskbarService.ThumbButtonInfos.Add(thumbButtonInfo_1);
            taskbarService.ThumbButtonInfos[0].Description = "thumbButton2";
            taskbarService.ThumbButtonInfos[0].IsEnabled = false;
            taskbarService.ThumbButtonInfos[0].IsInteractive = false;
            taskbarService.ThumbButtonInfos[0].IsBackgroundVisible = true;
            taskbarService.ThumbButtonInfos[0].DismissWhenClicked = true;
            taskbarService.ThumbButtonInfos[0].Visibility = Visibility.Collapsed;
            taskbarService.ThumbButtonInfos[0].ImageSource = imageSource2;
            AssertHelper.AssertEnumerablesAreEqual(new ThumbButtonInfo[] {
                new ThumbButtonInfo() {
                Description = "thumbButton2",
                IsEnabled = false,
                IsInteractive = false,
                IsBackgroundVisible = true,
                DismissWhenClicked = true,
                Visibility = Visibility.Collapsed,
                ImageSource = imageSource2 },
            }, RealWindow.TaskbarItemInfo.ThumbButtonInfos, true, new string[] { "Command" });
        }
        [Test]
        public void ThumbButtonPropertiesChanged() {
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ImageSource imageSource1 = ApplicationJumpListServiceTestsImageSourceHelper.GetImageSource(AssemblyHelper.GetResourceUri(typeof(TaskbarButtonServiceTests).Assembly, "Mvvm.UI/Tests/Images/demoicon.ico"));
            ImageSource imageSource2 = ApplicationJumpListServiceTestsImageSourceHelper.GetImageSource(AssemblyHelper.GetResourceUri(typeof(TaskbarButtonServiceTests).Assembly, "Mvvm.UI/Tests/Images/Code_Central.png"));
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            TaskbarThumbButtonInfo thumbButtonInfo_1 = new TaskbarThumbButtonInfo() {
                Description = "thumbButton1",
                IsEnabled = true,
                IsInteractive = true,
                IsBackgroundVisible = false,
                DismissWhenClicked = false,
                Visibility = Visibility.Visible,
                ImageSource = imageSource1
            };
            taskbarService.ThumbButtonInfos.Add(thumbButtonInfo_1);
            RealWindow.TaskbarItemInfo.ThumbButtonInfos[0].Description = "thumbButton2";
            RealWindow.TaskbarItemInfo.ThumbButtonInfos[0].IsEnabled = false;
            RealWindow.TaskbarItemInfo.ThumbButtonInfos[0].IsInteractive = false;
            RealWindow.TaskbarItemInfo.ThumbButtonInfos[0].IsBackgroundVisible = true;
            RealWindow.TaskbarItemInfo.ThumbButtonInfos[0].DismissWhenClicked = true;
            RealWindow.TaskbarItemInfo.ThumbButtonInfos[0].Visibility = Visibility.Collapsed;
            RealWindow.TaskbarItemInfo.ThumbButtonInfos[0].ImageSource = imageSource2;
            EnqueueShowRealWindow();
            AssertHelper.AssertEnumerablesAreEqual(new TaskbarThumbButtonInfo[] {
                new TaskbarThumbButtonInfo() {
                Description = "thumbButton2",
                IsEnabled = false,
                IsInteractive = false,
                IsBackgroundVisible = true,
                DismissWhenClicked = true,
                Visibility = Visibility.Collapsed,
                ImageSource = imageSource2 },
            }, taskbarService.ThumbButtonInfos, true, new string[] { "ItemInfo" });
        }
        [Test]
        public void SetThumbButtonInfosClickAction() {
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            TaskbarThumbButtonInfo taskbarThumbButtonInfo = new TaskbarThumbButtonInfo();
            bool actionExecuted = false;
            bool clickExecuted = false;
            taskbarThumbButtonInfo.Action = () => { actionExecuted = true; };
            taskbarThumbButtonInfo.Click += (s, e) => clickExecuted = true;

            taskbarService.ThumbButtonInfos.Add(taskbarThumbButtonInfo);
            ThumbButtonInfo thumbButtonInfo = RealWindow.TaskbarItemInfo.ThumbButtonInfos[0];
            ClickThumbButton(thumbButtonInfo);
            Assert.IsTrue(actionExecuted);
            Assert.IsTrue(clickExecuted);
        }

        static void ClickThumbButton(ThumbButtonInfo thumbButtonInfo) {
            EventHandler clickHandler = (EventHandler)typeof(ThumbButtonInfo).GetField("Click", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(thumbButtonInfo);
            if(clickHandler != null)
                clickHandler(thumbButtonInfo, EventArgs.Empty);
            if(thumbButtonInfo.Command != null && thumbButtonInfo.Command.CanExecute(thumbButtonInfo.CommandParameter))
                thumbButtonInfo.Command.Execute(thumbButtonInfo.CommandParameter);
        }
        [Test]
        public void ThumbButtonInfoFalseTriggering() {
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            int CommandClickCounter_0 = 0;
            int ActionClickCounter_0 = 0;
            int CommandClickCounter_1 = 0;
            int ActionClickCounter_1 = 0;

            taskbarService.ThumbButtonInfos.Add(
             new TaskbarThumbButtonInfo() {
                 Command = new DelegateCommand(() => ++CommandClickCounter_0),
                 Action = () => ++ActionClickCounter_0
             });
            taskbarService.ThumbButtonInfos.Add(
             new TaskbarThumbButtonInfo() {
                 Command = new DelegateCommand(() => ++CommandClickCounter_1),
                 Action = () => ++ActionClickCounter_1
             });

            ClickThumbButton(RealWindow.TaskbarItemInfo.ThumbButtonInfos[0]);
            ClickThumbButton(RealWindow.TaskbarItemInfo.ThumbButtonInfos[1]);
            Assert.AreEqual(CommandClickCounter_0, 1);
            Assert.AreEqual(ActionClickCounter_0, 1);
            Assert.AreEqual(CommandClickCounter_1, 1);
            Assert.AreEqual(ActionClickCounter_1, 1);
        }
        [Test]
        public void SetThumbButtonInfoProperties() {
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            for(int i = 0; i < 10; ++i)
                taskbarService.ThumbButtonInfos.Add(new TaskbarThumbButtonInfo());
            foreach(var item in taskbarService.ThumbButtonInfos) {
                item.Description = "NewDescription";
                item.IsEnabled = false;
                item.IsBackgroundVisible = false;
            }
            foreach(var item in RealWindow.TaskbarItemInfo.ThumbButtonInfos) {
                Assert.AreEqual("NewDescription", item.Description);
                Assert.AreEqual(false, item.IsEnabled);
                Assert.AreEqual(false, item.IsBackgroundVisible);
            }
        }
        [Test]
        public void SetThumbnailClipMargin() {
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            taskbarService.ThumbnailClipMargin = new Thickness { Bottom = 10, Left = 5, Right = 100, Top = 1 };
            Assert.AreEqual(new Thickness { Bottom = 10, Left = 5, Right = 100, Top = 1 }, RealWindow.TaskbarItemInfo.ThumbnailClipMargin);
        }
        [Test]
        public void SetThumbnailClipMarginCallback() {
            clipMarginShift = 0;
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService() { ThumbnailClipMarginCallback = GetThumbnailClipMargin };
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            Assert.IsTrue(RealWindow.Height > 100 && RealWindow.Width > 100);
            Assert.AreEqual(CorrectThickness(RealWindow), RealWindow.TaskbarItemInfo.ThumbnailClipMargin);
            clipMarginShift = 10;
            taskbarService.UpdateThumbnailClipMargin();
            Assert.AreEqual(CorrectThickness(RealWindow, clipMarginShift), RealWindow.TaskbarItemInfo.ThumbnailClipMargin);
        }
        [Test]
        public void SetThumbnailClipMarginCallback_LateBinding() {
            clipMarginShift = 0;
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            taskbarService.ThumbnailClipMarginCallback = GetThumbnailClipMargin;
            Assert.IsTrue(RealWindow.Height > 100 && RealWindow.Width > 100);
            Assert.AreEqual(CorrectThickness(RealWindow), RealWindow.TaskbarItemInfo.ThumbnailClipMargin);
        }
        [Test]
        public void SetThumbnailClipMarginCallback_SizeChanged() {
            clipMarginShift = 0;
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService() { ThumbnailClipMarginCallback = GetThumbnailClipMargin };
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            Assert.IsTrue(RealWindow.Height > 100 && RealWindow.Width > 100);
            Assert.AreEqual(CorrectThickness(RealWindow), RealWindow.TaskbarItemInfo.ThumbnailClipMargin);
            RealWindow.Width += 200;
            RealWindow.Width -= 50;
            Assert.AreEqual(CorrectThickness(RealWindow), RealWindow.TaskbarItemInfo.ThumbnailClipMargin);
        }
        [Test]
        public void SetThumbnailClipMarginCallback_TwoWindows() {
            EnqueueShowRealWindow();
            Window testWindow = new Window() { Height = 480, Width = 640, IsEnabled = true, Visibility = Visibility.Visible };
            testWindow.Show();
            try {
                clipMarginShift = 0;
                TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService() { ThumbnailClipMarginCallback = GetThumbnailClipMargin };
                Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
                Assert.IsTrue(RealWindow.Height > 100 && RealWindow.Width > 100);
                Assert.AreEqual(CorrectThickness(RealWindow), RealWindow.TaskbarItemInfo.ThumbnailClipMargin);
                taskbarServiceImpl.Window = testWindow;
                Assert.AreEqual(CorrectThickness(testWindow), testWindow.TaskbarItemInfo.ThumbnailClipMargin);
                getThumbnailClipMarginExecute = false;
                RealWindow.Width += 200;
                Assert.IsFalse(getThumbnailClipMarginExecute);
                testWindow.Width += 300;
                DispatcherHelper.UpdateLayoutAndDoEvents(testWindow);
                Assert.IsTrue(getThumbnailClipMarginExecute);
            } finally {
                testWindow.Close();
            }
        }
        [Test]
        public void ItemInfoBinding() {
            TaskbarButtonService taskbarServiceImpl = new TaskbarButtonService();
            Interaction.GetBehaviors(RealWindow).Add(taskbarServiceImpl);
            EnqueueShowRealWindow();
            ITaskbarButtonService taskbarService = taskbarServiceImpl;
            ImageSource icon_1 = ApplicationJumpListServiceTestsImageSourceHelper.GetImageSource(AssemblyHelper.GetResourceUri(typeof(TaskbarButtonServiceTests).Assembly, "Mvvm.UI/Tests/Images/demoicon.ico"));
            ThumbButtonInfo thumbButtonInfo = new ThumbButtonInfo() { Description = "thumbButton51" };
            RealWindow.TaskbarItemInfo.ProgressValue = 0.5;
            RealWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
            RealWindow.TaskbarItemInfo.Overlay = icon_1;
            RealWindow.TaskbarItemInfo.Description = "test1";
            RealWindow.TaskbarItemInfo.ThumbButtonInfos.Add(thumbButtonInfo);
            RealWindow.TaskbarItemInfo.ThumbnailClipMargin = new Thickness { Bottom = 1, Left = 50, Right = 99, Top = 11 };
            Assert.AreEqual(0.5, taskbarService.ProgressValue);
            Assert.AreEqual(TaskbarItemProgressState.Error, taskbarService.ProgressState);
            Assert.AreEqual(icon_1, taskbarService.OverlayIcon);
            Assert.AreEqual("test1", taskbarService.Description);
            AssertHelper.AssertEnumerablesAreEqual(new TaskbarThumbButtonInfo[] {
                new TaskbarThumbButtonInfo() { Description = "thumbButton51" }
            }, taskbarService.ThumbButtonInfos, true, new string[] { "ItemInfo" });
            Assert.AreEqual(new Thickness { Bottom = 1, Left = 50, Right = 99, Top = 11 }, taskbarService.ThumbnailClipMargin);
        }
        Thickness GetThumbnailClipMargin(Size size) {
            getThumbnailClipMarginExecute = true;
            return new Thickness {
                Left = 0.1 * size.Width + clipMarginShift,
                Top = 0.4 * size.Height - clipMarginShift,
                Right = 0.2 * size.Width + clipMarginShift,
                Bottom = 0.3 * size.Height - clipMarginShift
            };
        }
        Thickness CorrectThickness(Window window, double shift = 0.0) {
            return new Thickness {
                Left = 0.1 * window.Width + shift,
                Top = 0.4 * window.Height - shift,
                Right = 0.2 * window.Width + shift,
                Bottom = 0.3 * window.Height - shift
            };
        }
    }
}