using System.Threading;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Accounts;
using Neo.UI.Core.Controllers.Interfaces;
using Xunit;

namespace Neo.Gui.ViewModels.Tests.Accounts
{
    public class ImportCustomContractViewModelTests : TestBase
    {
        [Fact]
        public void Ctr_CreateValidImportCustomContractViewModel()
        {
            // Arrange
            // Act
            var viewModel = this.AutoMockContainer.Create<ImportCustomContractViewModel>();

            //Assert
            Assert.IsType<ImportCustomContractViewModel>(viewModel);
        }

        [Fact]
        public void ConfirmCommand_AddContractAndCloseDialog()
        {
            // Arrange
            var closeAutoResetEvent = new AutoResetEvent(false);
            var expectedCloseEventRaised = false;
            var script = "string";
            var parameterList = "Item1, Item2";

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();

            var viewModel = this.AutoMockContainer.Create<ImportCustomContractViewModel>();
            viewModel.Script = script;
            viewModel.ParameterList = parameterList;

            viewModel.Close += (sender, args) =>
            {
                expectedCloseEventRaised = true;
                closeAutoResetEvent.Set();
            };

            // Act
            viewModel.ConfirmCommand.Execute(null);
            closeAutoResetEvent.WaitOne();

            //Assert
            walletControllerMock.Verify(x => x.AddContractWithParameters(script, parameterList));
            Assert.True(expectedCloseEventRaised);
        }

        [Fact]
        public void CancelCommand_CloseDialog()
        {
            // Arrange
            var closeAutoResetEvent = new AutoResetEvent(false);
            var expectedCloseEventRaised = false;

            var viewModel = this.AutoMockContainer.Create<ImportCustomContractViewModel>();

            viewModel.Close += (sender, args) =>
            {
                expectedCloseEventRaised = true;
                closeAutoResetEvent.Set();
            };

            // Act
            viewModel.CancelCommand.Execute(null);
            closeAutoResetEvent.WaitOne();

            //Assert
            Assert.True(expectedCloseEventRaised);
        }
    }
}
