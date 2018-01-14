using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Voting;

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