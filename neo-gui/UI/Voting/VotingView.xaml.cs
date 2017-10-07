using Neo.Core;

namespace Neo.UI.Voting
{
    public partial class VotingView
    {
        public VotingView(UInt160 scriptHash)
        {
            InitializeComponent();

            var viewModel = this.DataContext as VotingViewModel;

            viewModel?.SetScriptHash(scriptHash);
        }

        public InvocationTransaction GetTransaction()
        {
            return (this.DataContext as VotingViewModel)?.GetTransaction();
        }
    }
}