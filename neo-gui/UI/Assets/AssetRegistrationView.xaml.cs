using Neo.Core;

namespace Neo.UI.Assets
{
    public partial class AssetRegistrationView
    {
        public AssetRegistrationView()
        {
            InitializeComponent();
        }

        public InvocationTransaction GetTransaction()
        {
            return (this.DataContext as AssetRegistrationViewModel)?.GetTransaction();
        }
    }
}