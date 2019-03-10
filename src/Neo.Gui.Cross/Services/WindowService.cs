using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommonServiceLocator;
using Neo.Gui.Cross.ViewModels;

namespace Neo.Gui.Cross.Services
{
    public class WindowService : IWindowService
    {
        private const string ViewModelSuffix = "ViewModel";
        private const string ViewSuffix = "View";
        private const string WindowSuffix = "Window";


        public void Show<TViewModel>() where TViewModel : ViewModelBase
        {
            var window = GetWindow<TViewModel>();

            window.Show();
        }

        public async Task ShowDialog<TViewModel>() where TViewModel : ViewModelBase
        {
            var window = GetWindow<TViewModel>();

            await window.ShowDialog();
        }

        /* // TODO Implement better handling of dialogs
         public Task<TResult> ShowDialog<TViewModel, TResult>() where TViewModel : DialogViewModelBase<TResult>
         {
             var window = GetWindow<TViewModel>();


             var tcs = new TaskCompletionSource<TResult>();

             window.Closed += (sender, args) =>
             {
                 // TODO Handle closing by user (i.e. set result as null)
             };

             viewModel.Closed += (sender, args) =>
             {
                 window.Close();
                 tcs.SetResult(args);
             };

             window.ShowDialog();

             return tcs.Task;
         }*/

        private static Window GetWindow<TViewModel>() where TViewModel : ViewModelBase
        {
            var viewType = GetViewType<TViewModel>();

            var view = (Control)Activator.CreateInstance(viewType);
            
            if (!(view.DataContext is TViewModel viewModel))
            {
                viewModel = (TViewModel) ServiceLocator.Current.GetInstance(typeof(TViewModel));

                view.DataContext = viewModel;
            }

            if (!(view is Window window))
            {
                // TODO Get window title from somewhere
                window = new Window
                {
                    Content = view
                };
            }

            // TODO Handle user initiated closing (e.g. clicking X button)
            viewModel.Close += (sender, e) =>
            {
                window.Close();
            };

            return window;
        }

        private static Type GetViewType<TViewModel>() where TViewModel : ViewModelBase
        {
            var viewName = typeof(TViewModel).FullName.Replace(ViewModelSuffix, ViewSuffix);

            var viewType = Type.GetType(viewName);

            if (viewType != null)
            {
                return viewType;
            }

            if (viewName.EndsWith(ViewSuffix))
            {
                // Try type name with suffix "Window"
                viewName = viewName.Substring(0, viewName.Length - ViewSuffix.Length) + WindowSuffix;
            }

            viewType = Type.GetType(viewName);

            return viewType;
        }
    }
}
