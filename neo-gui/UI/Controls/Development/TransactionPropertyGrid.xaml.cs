using System.Windows;

namespace Neo.UI.Controls.Development
{
    /// <summary>
    /// Interaction logic for TransactionPropertyGrid.xaml
    /// </summary>
    public partial class TransactionPropertyGrid
    {
        public TransactionPropertyGrid()
        {
            InitializeComponent();
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
            if (this.PropertyGrid.SelectedObject == value) return;

            this.PropertyGrid.SelectedObject = value;
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