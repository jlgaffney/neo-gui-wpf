using System.Diagnostics;
using System.Windows;
using Autofac;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.MVVM;

namespace Neo.Gui.Wpf.Implementations.Managers
{
    public class DialogManager : IDialogManager
    {
        #region Private Fields 
        private static ILifetimeScope containerLifetimeScope;
        #endregion

        #region IDialogManager implementation

        public TDialogResult ShowDialog<TDialogResult>()
        {
            var dialogResult = default(TDialogResult);

            var view = containerLifetimeScope?.Resolve<IDialog<TDialogResult>>();

            Debug.Assert(view != null);
            Debug.Assert(view is Window);

            var viewWindow = view as Window;

            var viewModel = view.DataContext as IDialogViewModel<TDialogResult>;
            if (viewModel != null)
            {
                viewModel.Close += (sender, e) =>
                {
                    viewWindow.Close();
                };

                viewModel.SetDialogResultAndClose += (sender, e) =>
                {
                    dialogResult = e;

                    viewWindow.Close();
                };
            }

            view.ShowDialog();

            return dialogResult;
        }

        public TDialogResult ShowDialog<TDialogResult, TLoadParameters>(ILoadParameters<TLoadParameters> parameters)
        {
            var dialogResult = default(TDialogResult);

            var view = containerLifetimeScope?.Resolve<IDialog<TDialogResult>>();

            // TODO [AboimPinto]: Don't agree with this return. Is there is no IDialog<T> exported, in developement this should be catched. This should never happen in prodution or there was enough tests.
            // Should throw an exception
            if (view == null) return dialogResult;

            if (view.DataContext is ILoadable loadableViewModel)
            {
                loadableViewModel.OnLoad(parameters);
            }

            var viewModel = view.DataContext as IDialogViewModel<TDialogResult>;
            if (viewModel != null)
            {

                viewModel.Close += (sender, e) =>
                {
                    var viewWindow = view as Window;
                    viewWindow.Close();
                };

                viewModel.SetDialogResultAndClose += (sender, e) =>
                {
                    dialogResult = e;

                    var viewWindow = view as Window;
                    viewWindow.Close();
                };
            }

            view.ShowDialog();

            return dialogResult;
        }

        public MessageDialogResult ShowMessage(string title, string message, MessageDialogType type = MessageDialogType.Ok, MessageDialogResult defaultResult = MessageDialogResult.Ok)
        {
            switch (type)
            {
                case MessageDialogType.Ok:
                    if (string.IsNullOrEmpty(title))
                    {
                        MessageBox.Show(message);
                    }
                    else
                    {
                        MessageBox.Show(message, title);
                    }
                    return MessageDialogResult.Ok;

                case MessageDialogType.OkCancel:
                    var result = MessageBox.Show(message, title, MessageBoxButton.OKCancel);

                    switch (result)
                    {
                        case MessageBoxResult.OK:
                            return MessageDialogResult.Ok;

                        case MessageBoxResult.Cancel:
                        default:
                            return MessageDialogResult.Cancel;
                    }

                case MessageDialogType.YesNo:
                    result = MessageBox.Show(message, title, MessageBoxButton.YesNo);

                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            return MessageDialogResult.Yes;

                        case MessageBoxResult.No:
                        default:
                            return MessageDialogResult.No;
                    }
            }

            return defaultResult;
        }

        #endregion

        #region Static methods
        public static void SetLifetimeScope(ILifetimeScope lifetimeScope)
        {
            containerLifetimeScope = lifetimeScope;
        }
        #endregion
    }
}
