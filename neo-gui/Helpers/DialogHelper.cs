using Autofac;
using Neo.Gui.Helpers.Interfaces;

namespace Neo.Helpers
{
    public class DialogHelper : IDialogHelper
    {
        #region Private Fields 
        private readonly IApplicationContext applicationContext;
        #endregion

        #region Constructor 
        public DialogHelper(IApplicationContext applicationContext)
        {
            this.applicationContext = applicationContext;
        }
        #endregion

        #region IDialogHelper implementation 
        public T ShowDialog<T>(params string[] parameters)
        {
            T dialogResult = default(T);

            var view = this.applicationContext.ContainerLifetimeScope.Resolve<IDialog<T>>();
            var viewModel = view.DataContext as IDialogViewModel<T>;
            viewModel.SetDialogResult += (sender, e) =>
            {
                dialogResult = e;
            };

            view.ShowDialog();

            return dialogResult;
        }
        #endregion
    }
}
