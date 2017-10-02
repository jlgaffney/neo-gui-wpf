using System.Windows;
using System.Xml.Linq;
using Neo.UI.Messages;
using Neo.UI.MVVM;
using Neo.UI.ViewModels.Updater;

namespace Neo.UI.Views.Updater
{
    public partial class UpdateView
    {
        public UpdateView(XDocument xdoc)
        {
            InitializeComponent();

            var viewModel = this.DataContext as UpdateViewModel;

            if (viewModel != null)
            {
                viewModel.SetUpdateInfo(xdoc);
            }
        }
    }
}