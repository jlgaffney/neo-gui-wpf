namespace Neo.Helpers
{
    /// <summary>
    /// Interface of the DialogHelper that abstracts the usage of Dialog windows in the application.
    /// </summary>
    public interface IDialogHelper
    {
        void ShowDialog(string dialogName, params string[] parameters);

        DialogResult<T> ShowDialog<T>(string dialogName, params string[] parameters);
    }
}
