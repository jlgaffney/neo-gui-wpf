using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Dialogs.Results.Contracts;

namespace Neo.Gui.Wpf.Views.Contracts
{
    /// <summary>
    /// Interaction logic for DeployContractView.xaml
    /// </summary>
    public partial class DeployContractView : IDialog<DeployContractDialogResult>
    {
        public DeployContractView()
        {
            InitializeComponent();
        }
    }
}