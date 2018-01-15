using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Voting;

namespace Neo.Gui.Wpf.Views.Voting
{
    public partial class VotingView : IDialog<VotingLoadParameters>
    {
        public VotingView()
        {
            InitializeComponent();
        }
    }
}