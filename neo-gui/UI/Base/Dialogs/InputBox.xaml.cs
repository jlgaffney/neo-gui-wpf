using System.Windows;

namespace Neo.UI.Base.Dialogs
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

        public static string Show(string text, string caption, string content = "")
        {
            var dialog = new InputBox(text, caption, content);

            dialog.ShowDialog();

            return dialog.inputResult;
        }

        private void OkClicked(object sender, RoutedEventArgs e)
        {
            this.inputResult = this.InputTextBox.Text;

            this.Close();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}