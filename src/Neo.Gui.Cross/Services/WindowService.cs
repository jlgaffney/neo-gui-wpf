using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Neo.Gui.Cross.ViewModels;

namespace Neo.Gui.Cross.Services
{
    // TODO Reduce duplication in methods
    public class WindowService : IWindowService
    {
        private const string ViewModelSuffix = "ViewModel";
        private const string ViewSuffix = "View";
        private const string WindowSuffix = "Window";

        public void Show<TViewModel, TLoadParameters>(TLoadParameters parameters) where TViewModel : ViewModelBase, ILoadable<TLoadParameters>
        {
            var window = GetWindow<TViewModel, TLoadParameters>(parameters);

            window.Show();
        }

        public void Show<TViewModel>() where TViewModel : ViewModelBase
        {
            var window = GetWindow<TViewModel>();

            window.Show();
        }

        public async Task ShowDialog<TViewModel, TLoadParameters>(TLoadParameters parameters) where TViewModel : ViewModelBase, ILoadable<TLoadParameters>
        {
            var window = GetWindow<TViewModel, TLoadParameters>(parameters);

            await window.ShowDialog();
        }

        public async Task ShowDialog<TViewModel>() where TViewModel : ViewModelBase
        {
            var window = GetWindow<TViewModel>();

            await window.ShowDialog();
        }

        private static Window GetWindow<TViewModel, TLoadParameters>(TLoadParameters parameters)
            where TViewModel : ViewModelBase, ILoadable<TLoadParameters>
        {
            var viewType = GetViewType<TViewModel>();

            var view = (Control)Activator.CreateInstance(viewType);

            if (!(view.DataContext is TViewModel viewModel))
            {
                viewModel = ViewModelLocator.GetDataContext<TViewModel, TLoadParameters>(parameters);

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
        
        private static Window GetWindow<TViewModel>()
            where TViewModel : ViewModelBase
        {
            var viewType = GetViewType<TViewModel>();

            var view = (Control)Activator.CreateInstance(viewType);
            
            if (!(view.DataContext is TViewModel viewModel))
            {
                viewModel = ViewModelLocator.GetDataContext<TViewModel>();

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
