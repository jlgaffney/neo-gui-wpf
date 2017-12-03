using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Autofac;
using Neo.Gui.Base.MVVM;

namespace Neo.Gui.Wpf.MarkupExtensions
{
    public class DataContextBindingExtension : MarkupExtension
    {
        #region Private fields
        private static ILifetimeScope containerLifetimeScope;
        #endregion

        #region Public Properties 
        [ConstructorArgument("viewModel")]
        public Type ViewModel { get; set; }
        #endregion

        #region Constructor 
        public DataContextBindingExtension()
        {
            // NOP
        }

        public DataContextBindingExtension(Type viewModel)
        {
            this.ViewModel = viewModel;
        }
        #endregion

        #region MarkupExtension implementation 
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.ViewModel == null) return null;

            var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            var target = provideValueTarget?.TargetObject as FrameworkElement;

            if (target == null || DesignerProperties.GetIsInDesignMode(target)) return null;

            var viewModelInstance = containerLifetimeScope.Resolve(this.ViewModel);

            if (viewModelInstance == null) return null;

            Debug.Assert(viewModelInstance.GetType() == this.ViewModel);

            if (viewModelInstance is ILoadable loadableViewModel)
            {
                loadableViewModel.OnLoad();
            }

            return viewModelInstance;
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