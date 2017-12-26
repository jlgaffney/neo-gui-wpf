using System;
using System.Diagnostics;
using System.Windows;

using Autofac;

using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Managers;

using Neo.Gui.WPF.Controls;

namespace Neo.Gui.Wpf.Implementations.Managers
{
    public class DialogManager : IDialogManager
    {
        #region Private Fields 
        private static ILifetimeScope containerLifetimeScope;
        #endregion

        #region IDialogManager implementation
        
        public IDialog<TDialogResult> CreateDialog<TDialogResult, TLoadParameters>(Action<TDialogResult> resultSetter, TLoadParameters parameters)
        {
            var view = ResolveDialogInstance<TDialogResult>();

            var loadable = view.DataContext as ILoadableDialogViewModel<TDialogResult, TLoadParameters>;
            loadable?.OnDialogLoad(parameters);

            InitializeDialogViewModel(view, resultSetter);

            return view;
        }

        public IDialog<TDialogResult> CreateDialog<TDialogResult>(Action<TDialogResult> resultSetter)
        {
            var view = ResolveDialogInstance<TDialogResult>();

            InitializeDialogViewModel(view, resultSetter);

            return view;
        }

        public TDialogResult ShowDialog<TDialogResult>()
        {
            var dialogResult = default(TDialogResult);

            var view = CreateDialog<TDialogResult>(result => { dialogResult = result; });
            
            view.ShowDialog();

            return dialogResult;
        }

        public TDialogResult ShowDialog<TDialogResult, TLoadParameters>(TLoadParameters parameters)
        {
            var dialogResult = default(TDialogResult);

            var view = CreateDialog<TDialogResult, TLoadParameters>(result => { dialogResult = result; }, parameters);
            
            view.ShowDialog();

            return dialogResult;
        }

        public string ShowInputDialog(string title, string message, string input = "")
        {
            var isOk = InputBox.Show(out var result, message, title, input);

            return isOk ? result : null;
        }

        public void ShowInformationDialog(string title, string message, string text)
        {
            InformationBox.Show(text, message, title);
        }

        public MessageDialogResult ShowMessageDialog(string title, string message, MessageDialogType type = MessageDialogType.Ok, MessageDialogResult defaultResult = MessageDialogResult.Ok)
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

        private static IDialog<TDialogResult> ResolveDialogInstance<TDialogResult>()
        {
            var view = containerLifetimeScope?.Resolve<IDialog<TDialogResult>>();

            Debug.Assert(view != null);
            Debug.Assert(view is Window);

            return view;
        }

        private static void InitializeDialogViewModel<TDialogResult>(IDialog<TDialogResult> view, Action<TDialogResult> resultSetter)
        {
            var viewWindow = view as Window;

            var viewModel = view.DataContext as IDialogViewModel<TDialogResult>;

            if (viewModel == null) return;

            viewModel.Close += (sender, e) =>
            {
                viewWindow?.Close();
            };

            viewModel.SetDialogResultAndClose += (sender, e) =>
            {
                resultSetter?.Invoke(e);

                viewWindow?.Close();
            };
        }
        #endregion
    }
}
