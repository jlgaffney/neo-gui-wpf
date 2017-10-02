using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Neo.UI.Views.Development
{
    /// <summary>
    /// Wrapper control which hosts a WinForms PropertyGrid control
    /// and provides dependency properties to allow property binding.
    /// </summary>
    public class TransactionPropertyGrid : WindowsFormsHost
    {
        private readonly PropertyGrid propertyGrid;

        public TransactionPropertyGrid()
        {
            this.propertyGrid = new PropertyGrid();

            this.Child = this.propertyGrid;
        }

        // Dependency Property
        public static readonly DependencyProperty TransactionWrapperProperty =
            DependencyProperty.Register("TransactionWrapper",
                typeof(object), typeof(TransactionPropertyGrid),
                    new FrameworkPropertyMetadata(null,
                        OnTransactionWrapperPropertyChanged,
                        OnCoerceTransactionWrapperProperty));

        // .NET Property wrapper
        // TODO Fix bug in functionality
        public object TransactionWrapper
        {
            get => GetValue(TransactionWrapperProperty);
            set => SetValue(TransactionWrapperProperty, value);
        }

        private void SetSelectedObject(object value)
        {
            if (this.propertyGrid.SelectedObject == value) return;

            this.propertyGrid.SelectedObject = value;
        }

        private static void OnTransactionWrapperPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var propertyGrid = source as TransactionPropertyGrid;

            // Set new selected object in WinForms PropertyGrid control
            propertyGrid?.SetSelectedObject(e.NewValue);
        }

        private static object OnCoerceTransactionWrapperProperty(DependencyObject source, object data)
        {
            var propertyGrid = source as TransactionPropertyGrid;

            // Set new selected object in WinForms PropertyGrid control
            propertyGrid?.SetSelectedObject(data);

            return data;
        }
    }
}