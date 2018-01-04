using System.Windows;

namespace Neo.Gui.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for InformationBox.xaml
    /// </summary>
    public partial class InformationBox
    {
        private InformationBox()
        {
            InitializeComponent();
        }

        public static bool? Show(string text, string message = null, string title = null)
        {
            var box = new InformationBox();

            if (title != null)
            {
                box.Title = title;
            }
            if (message != null)
            {
                box.MessageLabel.Content = message;
            }

            box.InputTextBox.Text = text;

            return box.ShowDialog();
        }

        private void CopyClicked(object sender, RoutedEventArgs e)
        {
            this.InputTextBox.SelectAll();
            this.InputTextBox.Copy();
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}