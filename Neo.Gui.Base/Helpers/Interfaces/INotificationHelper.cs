namespace Neo.Gui.Base.Interfaces.Helpers
{
    public interface INotificationHelper
    {
        void ShowSuccessNotification(string notificationMessage);

        void ShowErrorNotification(string notificationMessage);

        void ShowWarningNotification(string notificationMessage);

        void ShowInformationNotification(string notificationMessage);
    }
}