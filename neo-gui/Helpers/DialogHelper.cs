using System;

namespace Neo.Helpers
{
    public class DialogHelper : IDialogHelper
    {
        #region IDialogHelper implementation 
        public void ShowDialog(string dialogName, params string[] parameters)
        {

        }

        public DialogResult<T> ShowDialog<T>(string dialogName, params string[] parameters)
        {
            return default(DialogResult<T>);
        }
        #endregion
    }
}
