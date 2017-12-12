namespace Neo.Gui.Base.Services
{
    public interface INotificationService
    {
        void ShowSuccessNotification(string notificationMessage);

        void ShowErrorNotification(string notificationMessage);

        void ShowWarningNotification(string notificationMessage);

        void ShowInformationNotification(string notificationMessage);
    }
}