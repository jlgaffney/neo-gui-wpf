using Neo.Core;

namespace Neo.UI.Voting
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