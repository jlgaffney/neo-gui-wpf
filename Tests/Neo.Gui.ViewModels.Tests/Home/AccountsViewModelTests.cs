using Moq;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Dialogs.LoadParameters.Accounts;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Globalization.Resources;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Home;
using Xunit;

namespace Neo.Gui.ViewModels.Tests.Home
{
    public class AccountsViewModelTests : TestBase
    {
        [Fact]
        public void Ctr_CreateAccountsViewModel()
        {
            // Arrange
            // Act
            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();

            //Assert
            Assert.IsType<AccountsViewModel>(viewModel);
        }

        [Fact]
        public void Load_SubscribeMessages_SubscribeMethodCalledWithRightParameter()
        {
            // Arrange
            var messageSubscriberMock = this.AutoMockContainer.GetMock<IMessageSubscriber>();
            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();

            // Act
            viewModel.OnLoad();

            // Assert
            messageSubscriberMock.Verify(x => x.Subscribe(viewModel));
        }

        [Fact]
        public void Unload_UnsubscibeMessages_UnsubscribeMethodCalledWithRightParameter()
        {
            // Arrange
            var messageSubscriberMock = this.AutoMockContainer.GetMock<IMessageSubscriber>();
            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();

            // Act
            viewModel.OnUnload();

            // Assert
            messageSubscriberMock.Verify(x => x.Unsubscribe(viewModel));
        }

        [Fact]
        public void AccountAddedMessageReceived_AccountAdded()
        {
            // Arrange
            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;

            var expectedVersion = new System.Version(1, 0);

            // Act
            var initialAccountsCount = viewModel.Accounts.Count;
            accountAddedMessageHandler.HandleMessage(new AccountAddedMessage(new AccountItem("AccountLabel", null, AccountType.Standard)));

            // Assert
            Assert.Equal(0, initialAccountsCount);
            Assert.Single(viewModel.Accounts);
        }

        [Fact]
        public void ClearAccountsMessageReceived_AfterAccountAddedAccountIsClear()
        {
            // Arrange
            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;
            var clearAccountsMessageHandler = viewModel as IMessageHandler<ClearAccountsMessage>;

            var expectedVersion = new System.Version(1, 0);

            // Act
            var initialAccountsCount = viewModel.Accounts.Count;
            accountAddedMessageHandler.HandleMessage(new AccountAddedMessage(new AccountItem("AccountLabel", null, AccountType.Standard)));
            var accountWithOneRecord = viewModel.Accounts.Count;
            clearAccountsMessageHandler.HandleMessage(new ClearAccountsMessage());
;
            // Assert
            Assert.Equal(0, initialAccountsCount);
            Assert.Equal(1, accountWithOneRecord);
            Assert.Empty(viewModel.Accounts);
        }

        [Fact]
        public void CreateNewAccountCommand_CreateNewAccountCalledInWalletController()
        {
            // Arrange
            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();

            // Act
            viewModel.CreateNewAddressCommand.Execute(null);

            // Assert
            walletControllerMock.Verify(x => x.CreateNewAccount());
        }

        [Fact]
        public void ImportWifPrivateKeyCommand_ShowImportPrivateKeyDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();

            // Act
            viewModel.ImportWifPrivateKeyCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<ImportPrivateKeyDialogResult>());
        }

        [Fact]
        public void ImportFromCertificateCommand_ShowImportCertificateDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();

            // Act
            viewModel.ImportFromCertificateCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<ImportCertificateDialogResult>());
        }

        [Fact]
        public void ImportWatchOnlyAddressCommand_ShowInputDialogReturnNotNullString_ShowImportPrivateKeyDialog()
        {
            // Arrange
            var expectedReturnedAddress = "address";

            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();
            dialogManagerMock
                .Setup(x => x.ShowInputDialog(Strings.ImportWatchOnlyAddress, Strings.Address, It.IsAny<string>()))
                .Returns(expectedReturnedAddress);

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
        
            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();

            // Act
            viewModel.ImportWatchOnlyAddressCommand.Execute(null);

            // Assert
            walletControllerMock.Verify(x => x.ImportWatchOnlyAddress(expectedReturnedAddress));
        }

        [Fact]
        public void ImportWatchOnlyAddressCommand_ShowInputDialogReturnNullString_DoNothing()
        {
            // Arrange
            string expectedReturnedAddress = null;

            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();
            dialogManagerMock
                .Setup(x => x.ShowInputDialog(Strings.ImportWatchOnlyAddress, Strings.Address, It.IsAny<string>()))
                .Returns(expectedReturnedAddress);

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();

            // Act
            viewModel.ImportWatchOnlyAddressCommand.Execute(null);

            // Assert
            walletControllerMock.Verify(x => x.ImportWatchOnlyAddress(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void CreateMultiSignatureContractAddressCommand_ShowCreateMultiSigContractDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();

            // Act
            viewModel.CreateMultiSignatureContractAddressCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<CreateMultiSigContractDialogResult>());
        }

        [Fact]
        public void CreateLockContractAddressCommand_ShowCreateMultiSigContractDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();

            // Act
            viewModel.CreateLockContractAddressCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<CreateLockAccountDialogResult>());
        }

        [Fact]
        public void CreateCustomContractAddressCommand_ShowImportCustomContractDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();

            // Act
            viewModel.CreateCustomContractAddressCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<ImportCustomContractDialogResult>());
        }

        [Fact]
        public void ViewPrivateKeyCommand_ViewPrivateKeyIsEnabled_ShowViewPrivateKeyDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var hash = UInt160.Parse("d3cce84d0800172d09c88ccad61130611bd047a4");
            var selectedAccount = new AccountItem("accountItem", hash, AccountType.Standard);

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            viewModel.SelectedAccount = selectedAccount;

            // Act
            viewModel.ViewPrivateKeyCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<ViewPrivateKeyDialogResult, ViewPrivateKeyLoadParameters>(It.Is<ViewPrivateKeyLoadParameters>(p => p.ScriptHash == hash)));
        }

        [Fact]
        public void ViewPrivateKeyCommand_ViewPrivateKeyIsNotEnabled_ShowNothing()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var hash = UInt160.Parse("d3cce84d0800172d09c88ccad61130611bd047a4");
            var selectedAccount = new AccountItem("accountItem", hash, AccountType.NonStandard);

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            viewModel.SelectedAccount = selectedAccount;

            // Act
            viewModel.ViewPrivateKeyCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<ViewPrivateKeyDialogResult, ViewPrivateKeyLoadParameters>(It.Is<ViewPrivateKeyLoadParameters>(p => p.ScriptHash == hash)), Times.Never);
        }
    }
}
