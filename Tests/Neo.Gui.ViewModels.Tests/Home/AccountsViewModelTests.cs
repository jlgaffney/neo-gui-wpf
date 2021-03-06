﻿using Moq;
using Neo.Gui.Base.Controllers.Interfaces;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Dialogs;
using Neo.Gui.Base.Dialogs.LoadParameters.Accounts;
using Neo.Gui.Base.Dialogs.LoadParameters.Voting;
using Neo.Gui.Base.Dialogs.Results.Voting;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Globalization.Resources;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Home;
using Neo.Gui.ViewModels.Tests.Builders;
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

            var selectedAccount = new AccountItemBuilder()
              .StandardAccount()
              .WithHash("d3cce84d0800172d09c88ccad61130611bd047a4")
              .Build();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            viewModel.SelectedAccount = selectedAccount;

            // Act
            viewModel.ViewPrivateKeyCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<ViewPrivateKeyDialogResult, ViewPrivateKeyLoadParameters>(It.Is<ViewPrivateKeyLoadParameters>(p => p.ScriptHash == selectedAccount.ScriptHash)));
        }

        [Fact]
        public void ViewPrivateKeyCommand_ViewPrivateKeyIsNotEnabled_ShowNothing()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var selectedAccount = new AccountItemBuilder()
              .NonStandardAccount()
              .WithHash("d3cce84d0800172d09c88ccad61130611bd047a4")
              .Build();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            viewModel.SelectedAccount = selectedAccount;

            // Act
            viewModel.ViewPrivateKeyCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<ViewPrivateKeyDialogResult, ViewPrivateKeyLoadParameters>(It.Is<ViewPrivateKeyLoadParameters>(p => p.ScriptHash == selectedAccount.ScriptHash)), Times.Never);
        }

        [Fact]
        public void ViewContractCommand_ViewContractIsEnabled_ViewPrivateKeyIsNotEnabled_ShowViewContractDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var selectedAccount = new AccountItemBuilder()
              .StandardAccount()
              .WithHash("d3cce84d0800172d09c88ccad61130611bd047a4")
              .Build();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            viewModel.SelectedAccount = selectedAccount;

            // Act
            viewModel.ViewContractCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<ViewContractDialogResult, ViewContractLoadParameters>(It.Is<ViewContractLoadParameters>(p => p.ScriptHash == selectedAccount.ScriptHash)));
        }

        [Fact]
        public void ViewContractCommand_ViewContractIsNotEnabled_ViewPrivateKeyIsNotEnabled_ShowNothing()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var selectedAccount = new AccountItemBuilder()
              .WatchOnlyAccount()
              .WithHash("d3cce84d0800172d09c88ccad61130611bd047a4")
              .Build();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            viewModel.SelectedAccount = selectedAccount;

            // Act
            viewModel.ViewContractCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<ViewContractDialogResult, ViewContractLoadParameters>(It.Is<ViewContractLoadParameters>(p => p.ScriptHash == selectedAccount.ScriptHash)), Times.Never);
        }

        [Fact]
        public void ShowVotingDialogCommand_ShowVotingDialogIsEnabled_ShowVotingDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var selectedAccount = new AccountItemBuilder()
                .StandardAccount()
                .WithHash("d3cce84d0800172d09c88ccad61130611bd047a4")
                .AccountWithNeoBalance()
                .Build();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            viewModel.SelectedAccount = selectedAccount;

            // Act
            viewModel.ShowVotingDialogCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<VotingDialogResult, VotingLoadParameters>(It.Is<VotingLoadParameters>(p => p.ScriptHash == selectedAccount.ScriptHash)));
        }

        [Fact]
        public void ShowVotingDialogCommand_ShowVotingDialogIsNotEnabledNoBalance_ShowNothing()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var selectedAccount = new AccountItemBuilder()
                .StandardAccount()
                .WithHash("d3cce84d0800172d09c88ccad61130611bd047a4")
                .Build();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            viewModel.SelectedAccount = selectedAccount;

            // Act
            viewModel.ShowVotingDialogCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<VotingDialogResult, VotingLoadParameters>(It.Is<VotingLoadParameters>(p => p.ScriptHash == selectedAccount.ScriptHash)), Times.Never);
        }

        [Fact]
        public void ShowVotingDialogCommand_ShowVotingDialogIsNotEnabledwatchOnlyAccountNoBalance_ShowNothing()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var selectedAccount = new AccountItemBuilder()
                .WatchOnlyAccount()
                .WithHash("d3cce84d0800172d09c88ccad61130611bd047a4")
                .Build();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            viewModel.SelectedAccount = selectedAccount;

            // Act
            viewModel.ShowVotingDialogCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<VotingDialogResult, VotingLoadParameters>(It.Is<VotingLoadParameters>(p => p.ScriptHash == selectedAccount.ScriptHash)), Times.Never);
        }

        [Fact]
        public void ShowVotingDialogCommand_ShowVotingDialogIsNotEnabledwatchOnlyAccountWithBalance_ShowNothing()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var selectedAccount = new AccountItemBuilder()
                .WatchOnlyAccount()
                .WithHash("d3cce84d0800172d09c88ccad61130611bd047a4")
                .AccountWithNeoBalance()
                .Build();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            viewModel.SelectedAccount = selectedAccount;

            // Act
            viewModel.ShowVotingDialogCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<VotingDialogResult, VotingLoadParameters>(It.Is<VotingLoadParameters>(p => p.ScriptHash == selectedAccount.ScriptHash)), Times.Never);
        }

        [Fact]
        public void CopyAddressToClipboardCommand_TextSetInClipboardManager()
        {
            // Arrange
            var walletAddress = "walletAddress";

            var clipboardManagetMock = this.AutoMockContainer.GetMock<IClipboardManager>();

            var selectedAccount = new AccountItemBuilder()
                .StandardAccount()
                .WithHash("d3cce84d0800172d09c88ccad61130611bd047a4")
                .Build();

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
            walletControllerMock
                .Setup(x => x.ScriptHashToAddress(selectedAccount.ScriptHash))
                .Returns(walletAddress);

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            viewModel.SelectedAccount = selectedAccount;

            // Act
            viewModel.CopyAddressToClipboardCommand.Execute(null);

            // Assert
            clipboardManagetMock.Verify(x => x.SetText(walletAddress));
        }

        [Fact]
        public void DeleteAccountCommand_MessageResultDialogYes_AccountIsDeleted()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();
            dialogManagerMock
                .Setup(x => 
                    x.ShowMessageDialog(
                        Strings.DeleteAddressConfirmationCaption, 
                        Strings.DeleteAddressConfirmationMessage, 
                        MessageDialogType.YesNo, 
                        MessageDialogResult.No))
                .Returns(MessageDialogResult.Yes);

            var selectedAccount = new AccountItemBuilder()
                .StandardAccount()
                .WithHash("d3cce84d0800172d09c88ccad61130611bd047a4")
                .Build();

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
            walletControllerMock
                .Setup(x => x.DeleteAccount(selectedAccount))
                .Returns(true);

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;
            viewModel.SelectedAccount = selectedAccount;

            // Act
            accountAddedMessageHandler.HandleMessage(new AccountAddedMessage(selectedAccount));
            viewModel.DeleteAccountCommand.Execute(null);

            // Assert
            walletControllerMock.Verify(x => x.DeleteAccount(selectedAccount));
            Assert.Empty(viewModel.Accounts);
        }

        [Fact]
        public void DeleteAccountCommand_MessageResultDialogNo_AccountIsDeleted()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();
            dialogManagerMock
                .Setup(x =>
                    x.ShowMessageDialog(
                        Strings.DeleteAddressConfirmationCaption,
                        Strings.DeleteAddressConfirmationMessage,
                        MessageDialogType.YesNo,
                        MessageDialogResult.No))
                .Returns(MessageDialogResult.No);

            var selectedAccount = new AccountItemBuilder()
                .StandardAccount()
                .WithHash("d3cce84d0800172d09c88ccad61130611bd047a4")
                .Build();

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;
            viewModel.SelectedAccount = selectedAccount;

            // Act
            accountAddedMessageHandler.HandleMessage(new AccountAddedMessage(selectedAccount));
            viewModel.DeleteAccountCommand.Execute(null);

            // Assert
            walletControllerMock.Verify(x => x.DeleteAccount(selectedAccount), Times.Never);
            walletControllerMock.Verify(x => x.DeleteAccount(selectedAccount), Times.Never);
            Assert.Single(viewModel.Accounts);
        }

        [Fact]
        public void ViewSelectedAccountDetailsCommand_MessageResultDialogNo_AccountIsDeleted()
        {
            // Arrange
            var walletAddress = "walletAddress";
            var urlFormat = @"x:\test\{0}";
            var expectedUrl = string.Format(urlFormat, walletAddress);

            var settingsManagerMock = this.AutoMockContainer.GetMock<ISettingsManager>();
            settingsManagerMock
                .SetupGet(x => x.AddressURLFormat)
                .Returns(urlFormat);

            var processManagerMock = this.AutoMockContainer.GetMock<IProcessManager>();

            var selectedAccount = new AccountItemBuilder()
                .StandardAccount()
                .WithHash("d3cce84d0800172d09c88ccad61130611bd047a4")
                .Build();

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
            walletControllerMock
                .Setup(x => x.ScriptHashToAddress(selectedAccount.ScriptHash))
                .Returns(walletAddress);

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            viewModel.SelectedAccount = selectedAccount;

            // Act
            viewModel.ViewSelectedAccountDetailsCommand.Execute(null);

            // Assert
            processManagerMock.Verify(x => x.OpenInExternalBrowser(expectedUrl));
        }
    }
}
