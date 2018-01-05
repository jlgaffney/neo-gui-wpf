using System;
using System.Threading;
using Moq;
using Neo.Cryptography.ECC;
using Neo.Gui.Base.Controllers.Interfaces;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Accounts;
using Xunit;

namespace Neo.Gui.ViewModels.Tests.Accounts
{
    public class CreateLockAccountViewModelTests : TestBase
    {
        [Fact]
        public void Ctr_CreateValidHomeViewModel()
        {
            // Arrange
            // Act
            var viewModel = this.AutoMockContainer.Create<CreateLockAccountViewModel>();

            //Assert
            Assert.IsType<CreateLockAccountViewModel>(viewModel);
        }

        [Fact]
        public void CreateCommand_CallWalletControllerCreateContractMethodAndCloseDialog()
        {
            // Arrange
            var closeAutoResetEvent = new AutoResetEvent(false);
            var expectedCloseEventRaised = false;

            var unlockHour = 1;
            var unlockMinute = 1;
            var unlockTimeStamp = new DateTime(2017, 1, 1, 10, 0, 0);

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();

            // Act
            var viewModel = this.AutoMockContainer.Create<CreateLockAccountViewModel>();
            viewModel.UnlockDate = unlockTimeStamp;
            viewModel.UnlockHour = unlockHour;
            viewModel.UnlockMinute = unlockMinute;
            viewModel.SelectedKeyPair = new ECPoint();

            viewModel.Close += (sender, args) =>
            {
                expectedCloseEventRaised = true;
                closeAutoResetEvent.Set();
            };

            viewModel.CreateCommand.Execute(null);
            closeAutoResetEvent.WaitOne();

            // Assert
            walletControllerMock.Verify(x => x.AddAccountContract(viewModel.SelectedKeyPair, It.IsAny<uint>()));
            Assert.True(expectedCloseEventRaised);
        }

        [Fact]
        public void CancelCommand_CloseDialog()
        {
            // Arrange
            var closeAutoResetEvent = new AutoResetEvent(false);
            var expectedCloseEventRaised = false;

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();

            // Act
            var viewModel = this.AutoMockContainer.Create<CreateLockAccountViewModel>();

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
    }
}
