using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Neo.UI.Controls
{
    /// <summary>
    /// Interaction logic for NumericUpDownControl.xaml
    /// </summary>
    public partial class NumericUpDownControl
    {
        public NumericUpDownControl()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        
        // Dependency Property
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value",
                typeof(int), typeof(NumericUpDownControl),
                new FrameworkPropertyMetadata(default(int)));

        // .NET Property wrapper
        public int Value
        {
            get
            {
                var value = GetValue(ValueProperty);

                if (value == null) return default(int);

                return (int) value;
            }
            set => SetValue(ValueProperty, value);
        }

        // Dependency Property
        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.Register("MinimumValue",
                typeof(int?), typeof(NumericUpDownControl),
                new FrameworkPropertyMetadata(null));

        // .NET Property wrapper
        public int? MinimumValue
        {
            get => (int?)GetValue(MinimumValueProperty);
            set => SetValue(MinimumValueProperty, value);
        }

        // Dependency Property
        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.Register("MaximumValue",
                typeof(int?), typeof(NumericUpDownControl),
                new FrameworkPropertyMetadata(null));

        // .NET Property wrapper
        public int? MaximumValue
        {
            get => (int?)GetValue(MaximumValueProperty);
            set => SetValue(MaximumValueProperty, value);
        }

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            if (this.MaximumValue.HasValue && this.Value >= this.MaximumValue.Value)
            {
                this.Value = this.MaximumValue.Value;
            }
            else
            {
                this.Value++;
            }
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            if (this.MinimumValue.HasValue && this.Value <= this.MinimumValue.Value)
            {
                this.Value = this.MinimumValue.Value;
            }
            else
            {
                this.Value--;
            }
        }

        private void txtNum_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (txtNum == null) return;

            if (!int.TryParse(e.Text, out var value)) return;

            if ((this.MinimumValue.HasValue && value <= this.MinimumValue.Value) ||
                (this.MaximumValue.HasValue && value >= this.MaximumValue.Value))
            {
                e.Handled = true;
                return;
            }

            this.Value = value;
        }

        private void txtNumber_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNum == null) return;

            if (!int.TryParse(txtNum.Text, out var value)) return;

            if (this.MinimumValue.HasValue && value <= this.MinimumValue.Value)
            {
                // TODO Cancel text change
                this.Value = this.MinimumValue.Value;
            }
            else if (this.MaximumValue.HasValue && value >= this.MaximumValue.Value)
            {
                // TODO Cancel text change
                this.Value = this.MaximumValue.Value;
            }
            else
            {
                this.Value = value;
            }
        }
    }
}