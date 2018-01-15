using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Accounts;
using Neo.UI.Core.Controllers.Interfaces;
using Xunit;

namespace Neo.Gui.ViewModels.Tests.Accounts
{
    public class ImportPrivateKeyViewModelTests : TestBase
    {
        [Fact]
        public void Ctr_CreateValidImportPrivateKeyViewModel()
        {
            // Arrange
            // Act
            var viewModel = this.AutoMockContainer.Create<ImportPrivateKeyViewModel>();

            //Assert
            Assert.IsType<ImportPrivateKeyViewModel>(viewModel);
        }

        [Fact]
        public void CancelCommand_CloseDialog()
        {
            // Arrange
            var closeAutoResetEvent = new AutoResetEvent(false);
            var expectedCloseEventRaised = false;

            // Act
            var viewModel = this.AutoMockContainer.Create<ImportPrivateKeyViewModel>();

            viewModel.Close += (sender, args) =>
            {
                expectedCloseEventRaised = true;
                closeAutoResetEvent.Set();
            };

            viewModel.CancelCommand.Execute(null);
            closeAutoResetEvent.WaitOne();

            // Assert
            Assert.True(expectedCloseEventRaised);
        }

        [Fact]
        public void OkCommand_OneKey_ImportKeyAndCloseDialog()
        {
            // Arrange
            var closeAutoResetEvent = new AutoResetEvent(false);
            var expectedCloseEventRaised = false;

            // Act
            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();

            var viewModel = this.AutoMockContainer.Create<ImportPrivateKeyViewModel>();
            var privateKey1 = "PrivateKey1";
            viewModel.PrivateKeysWif = $"{privateKey1}";

            viewModel.Close += (sender, args) =>
            {
                expectedCloseEventRaised = true;
                closeAutoResetEvent.Set();
            };

            viewModel.OkCommand.Execute(null);
            closeAutoResetEvent.WaitOne();

            // Assert
            walletControllerMock.Verify(x => x.ImportPrivateKeys(It.Is<IEnumerable<string>>(keys => keys.Single() == privateKey1)));
            Assert.True(expectedCloseEventRaised);
        }

        [Fact]
        public void OkCommand_OneKeyAndEmptyLine_ImportKeyAndCloseDialog()
        {
            // Arrange
            var closeAutoResetEvent = new AutoResetEvent(false);
            var expectedCloseEventRaised = false;

            // Act
            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();

            var viewModel = this.AutoMockContainer.Create<ImportPrivateKeyViewModel>();
            var privateKey1 = "PrivateKey1";
            viewModel.PrivateKeysWif = $"{privateKey1}\r\n";

            viewModel.Close += (sender, args) =>
            {
                expectedCloseEventRaised = true;
                closeAutoResetEvent.Set();
            };

            viewModel.OkCommand.Execute(null);
            closeAutoResetEvent.WaitOne();

            // Assert
            walletControllerMock.Verify(x => x.ImportPrivateKeys(It.Is<IEnumerable<string>>(keys => keys.Single() == privateKey1)));
            Assert.True(expectedCloseEventRaised);
        }

        [Fact]
        public void OkCommand_TwoKeys_ImportKeysAndCloseDialog()
        {
            // Arrange
            var closeAutoResetEvent = new AutoResetEvent(false);
            var expectedCloseEventRaised = false;

            // Act
            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();

            var viewModel = this.AutoMockContainer.Create<ImportPrivateKeyViewModel>();
            var privateKey1 = "PrivateKey1";
            var privateKey2 = "PrivateKey2";
            viewModel.PrivateKeysWif = $"{privateKey1}\r\n{privateKey2}";

            viewModel.Close += (sender, args) =>
            {
                expectedCloseEventRaised = true;
                closeAutoResetEvent.Set();
            };

            viewModel.OkCommand.Execute(null);
            closeAutoResetEvent.WaitOne();

            // Assert
            walletControllerMock.Verify(x => x.ImportPrivateKeys(It.Is<IEnumerable<string>>(keys => keys.Count() == 2 && keys.First() == privateKey1 && keys.Last() == privateKey2)));
            Assert.True(expectedCloseEventRaised);
        }
    }
}
