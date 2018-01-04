using System.Windows;

namespace Neo.Gui.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox
    {
        private string inputResult;

        private InputBox(string text, string caption, string content)
        {
            InitializeComponent();

            this.Title = caption;
            this.GroupBox.Header = text;
            this.InputTextBox.Text = content;
        }

        private bool IsOk { get; set; }

        public static bool Show(out string result, string text, string caption, string content = "")
        {
            result = null;

            var dialog = new InputBox(text, caption, content);

            dialog.ShowDialog();

            if (!dialog.IsOk) return false;

            result = dialog.inputResult;
            return true;
        }

        private void OkClicked(object sender, RoutedEventArgs e)
        {
            this.inputResult = this.InputTextBox.Text;

            this.IsOk = true;

            this.Close();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}