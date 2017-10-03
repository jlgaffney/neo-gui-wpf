using Neo.Core;
using Neo.UI.ViewModels.Voting;

namespace Neo.UI.Views.Voting
{
    public partial class ElectionView
    {
        public ElectionView()
        {
            InitializeComponent();
        }

        public InvocationTransaction GetTransactionResult()
        {
            return (this.DataContext as ElectionViewModel)?.Transaction;
        }
    }
}