using System;
using System.Windows;

using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.Gui.Wpf.Implementations.Services
{
    public class NotificationService : INotificationService
    {
        #region Private Fields 
        private readonly Notifier notifier;
        #endregion

        #region Constructor 
        public NotificationService()
        {
            this.notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.TopRight,
                    offsetX: 10,
                    offsetY: 10);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(3),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
        }
        #endregion

        #region INotificationService implementation 
        public void ShowErrorNotification(string message)
        {
            this.notifier.ShowError(message);
        }

        public void ShowInformationNotification(string message)
        {
            this.notifier.ShowInformation(message);
        }

        public void ShowSuccessNotification(string message)
        {
            this.notifier.ShowSuccess(message);
        }

        public void ShowWarningNotification(string message)
        {
            this.notifier.ShowWarning(message);
        }
        #endregion
    }
}
