using System;
using System.Windows;
using Neo.Gui.Base.Helpers.Interfaces;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace Neo.Gui.Wpf.Helpers
{
    public class NotificationHelper : INotificationHelper
    {
        #region Private Fields 
        private readonly Notifier notifier;
        #endregion

        #region Constructor 
        public NotificationHelper()
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

        #region INotificationHelper implementation 
        public void ShowErrorNotification(string notificationMessage)
        {
            this.notifier.ShowError(notificationMessage);
        }

        public void ShowInformationNotification(string notificationMessage)
        {
            this.notifier.ShowInformation(notificationMessage);
        }

        public void ShowSuccessNotification(string notificationMessage)
        {
            this.notifier.ShowSuccess(notificationMessage);
        }

        public void ShowWarningNotification(string notificationMessage)
        {
            this.notifier.ShowWarning(notificationMessage);
        }
        #endregion
    }
}
