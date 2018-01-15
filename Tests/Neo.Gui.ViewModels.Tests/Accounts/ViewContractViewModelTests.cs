using System.Threading;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Accounts;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Accounts;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Data;
using Xunit;

namespace Neo.Gui.ViewModels.Tests.Accounts
{
    public class ViewContractViewModelTests : TestBase
    {
        [Fact]
        public void Ctr_CreateValidViewContractViewModel()
        {
            // Arrange
            // Act
            var viewModel = this.AutoMockContainer.Create<ViewContractViewModel>();

            //Assert
            Assert.IsType<ViewContractViewModel>(viewModel);
        }

        [Fact]
        public void CancelCommand_CloseDialog()
        {
            // Arrange
            var closeAutoResetEvent = new AutoResetEvent(false);
            var expectedCloseEventRaised = false;

            // Act
            var viewModel = this.AutoMockContainer.Create<ViewContractViewModel>();

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
        public void OnDialoadLoad_ParametersIsNotNull_LoadAccountContract()
        {
            // Arrange
            var accountScriptHash = "accountScriptHash";
            var expectedAccountContract = new AccountContract
            {
                Address = "Address",
                ParameterList = "ParameterList",
                RedeemScriptHex = "RedeemScriptHex",
                ScriptHash = "ScriptHash"
            };

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
            walletControllerMock
                .Setup(x => x.GetAccountContract(accountScriptHash))
                .Returns(expectedAccountContract);

            var viewModel = this.AutoMockContainer.Create<ViewContractViewModel>();
            var loadableDialog = viewModel as IDialogViewModel<ViewContractLoadParameters>;

            // Act
            loadableDialog.OnDialogLoad(new ViewContractLoadParameters(accountScriptHash));

            // Assert
            Assert.Equal(expectedAccountContract.Address, viewModel.Address);
            Assert.Equal(expectedAccountContract.ParameterList, viewModel.ParameterList);
            Assert.Equal(expectedAccountContract.RedeemScriptHex, viewModel.RedeemScriptHex);
            Assert.Equal(expectedAccountContract.ScriptHash, viewModel.ScriptHash);
        }
    }
}
