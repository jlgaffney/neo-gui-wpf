using Neo.Core;
using Neo.UI.ViewModels.Assets;

namespace Neo.UI.Views.Assets
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