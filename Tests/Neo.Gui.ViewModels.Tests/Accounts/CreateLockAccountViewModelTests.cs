using System;
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
    public class CreateLockAccountViewModelTests : TestBase
    {
        [Fact]
        public void Ctr_CreateValidCreateLockAccountViewModel()
        {
            // Arrange
            // Act
            var viewModel = this.AutoMockContainer.Create<CreateLockAccountViewModel>();

            //Assert
            Assert.IsType<CreateLockAccountViewModel>(viewModel);
        }

        [Fact]
        public void OnLoad_PublicKeysAreLoaded()
        {
            // Arrange
            var firstPublicKey = "000";
            var publicKeys = new List<string>
            {
                firstPublicKey
            };

            var walletControlerMock = this.AutoMockContainer.GetMock<IWalletController>();
            walletControlerMock
                .Setup(x => x.GetPublicKeysFromStandardAccounts())
                .Returns(publicKeys);

            var viewModel = this.AutoMockContainer.Create<CreateLockAccountViewModel>();

            // Act
            viewModel.OnLoad();

            // Assert
            Assert.Single(viewModel.PublicKeys);
            Assert.Equal(firstPublicKey, viewModel.PublicKeys.Single());
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
            viewModel.SelectedPublicKey = "0000";

            viewModel.Close += (sender, args) =>
            {
                expectedCloseEventRaised = true;
                closeAutoResetEvent.Set();
            };

            viewModel.CreateCommand.Execute(null);
            closeAutoResetEvent.WaitOne();

            // Assert
            walletControllerMock.Verify(x => x.AddLockContractAccount(viewModel.SelectedPublicKey, It.IsAny<uint>()));
            Assert.True(expectedCloseEventRaised);
        }

        [Fact]
        public void CancelCommand_CloseDialog()
        {
            // Arrange
            var closeAutoResetEvent = new AutoResetEvent(false);
            var expectedCloseEventRaised = false;

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
