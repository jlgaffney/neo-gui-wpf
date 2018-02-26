using System.Threading;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Accounts;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Accounts;
using Neo.UI.Core.Data;
using Neo.UI.Core.Wallet;
using Xunit;

namespace Neo.Gui.ViewModels.Tests.Accounts
{
    public class ViewPrivateKeyViewModelTests : TestBase
    {
        [Fact]
        public void Ctr_CreateValidViewPrivateKeyViewModel()
        {
            // Arrange
            // Act
            var viewModel = this.AutoMockContainer.Create<ViewPrivateKeyViewModel>();

            //Assert
            Assert.IsType<ViewPrivateKeyViewModel>(viewModel);
        }

        [Fact]
        public void CancelCommand_CloseDialog()
        {
            // Arrange
            var closeAutoResetEvent = new AutoResetEvent(false);
            var expectedCloseEventRaised = false;

            // Act
            var viewModel = this.AutoMockContainer.Create<ViewPrivateKeyViewModel>();

            viewModel.Close += (sender, args) =>
            {
                expectedCloseEventRaised = true;
                closeAutoResetEvent.Set();
            };

            viewModel.CloseCommand.Execute(null);
            closeAutoResetEvent.WaitOne();

            // Assert
            Assert.True(expectedCloseEventRaised);
        }

        [Fact]
        public void OnDialogLoad_GetAccountKeysForTheAddress_PropertiesAreAssignedWithRightValue()
        {
            // Arrange
            var scriptHash = "ScriptHash";
            var viewPrivateKeyLoadParameters = new ViewPrivateKeyLoadParameters(scriptHash);

            var expectedAccountKeys = new AccountKeyInfo
            {
                Address = "Address",
                PrivateKeyHex = "PrivateHexKey",
                PublicKeyHex = "PublicHexKey",
                PrivateKeyWif = "PrivateWifKey"
            };

            var wallerControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
            wallerControllerMock
                .Setup(x => x.GetAccountKeys(scriptHash))
                .Returns(expectedAccountKeys);

            var viewModel = this.AutoMockContainer.Create<ViewPrivateKeyViewModel>();
            var loadableDialog = viewModel as IDialogViewModel<ViewPrivateKeyLoadParameters>;

            // Act
            loadableDialog.OnDialogLoad(viewPrivateKeyLoadParameters);

            // Assert
            Assert.Equal(viewModel.Address, expectedAccountKeys.Address);
            Assert.Equal(viewModel.PrivateKeyHex, expectedAccountKeys.PrivateKeyHex);
            Assert.Equal(viewModel.PublicKeyHex, expectedAccountKeys.PublicKeyHex);
            Assert.Equal(viewModel.PrivateKeyWif, expectedAccountKeys.PrivateKeyWif);
        }
    }
}
