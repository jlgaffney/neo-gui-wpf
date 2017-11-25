namespace Neo.Gui.Wpf.Views.Voting
{
    public partial class VotingView
    {
        public VotingView(UInt160 scriptHash)
        {
            InitializeComponent();

            var viewModel = this.DataContext as VotingViewModel;

            viewModel?.SetScriptHash(scriptHash);
        }
    }
}