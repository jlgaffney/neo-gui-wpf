using System.Linq;
using Moq;
using Neo.Gui.Dialogs;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Accounts;
using Neo.Gui.Dialogs.LoadParameters.Voting;
using Neo.UI.Core.Globalization.Resources;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Home;
using Neo.UI.Core.Data.Enums;
using Neo.UI.Core.Helpers.Extensions;
using Neo.UI.Core.Messaging.Interfaces;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Wallet;
using Neo.UI.Core.Wallet.Messages;
using Xunit;

namespace Neo.Gui.ViewModels.Tests.Home
{
    public class AccountsViewModelTests : TestBase
    {
        private const string TestAccountLabel = "Test Account";
        private const string TestAddress = "AWkWVcBwjE9aEqUd8Uzzp5jGteykvXJmsy";
        private const string TestScriptHash = "d3cce84d0800172d09c88ccad61130611bd047a4";

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

            // Act
            var initialAccountsCount = viewModel.Accounts.Count;
            accountAddedMessageHandler.HandleMessage(GetTestAccountAddedMessage());

            // Assert
            Assert.Equal(0, initialAccountsCount);
            Assert.Single(viewModel.Accounts);
        }

        [Fact]
        public void CurrentWalletHasChangedMessageReceived_AfterAccountAddedAccountIsClear()
        {
            // Arrange
            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;
            var currentWalletHasChangedMessageHandler = viewModel as IMessageHandler<CurrentWalletHasChangedMessage>;
            
            // Act
            var initialAccountsCount = viewModel.Accounts.Count;
            accountAddedMessageHandler.HandleMessage(GetTestAccountAddedMessage());
            var accountWithOneRecord = viewModel.Accounts.Count;
            currentWalletHasChangedMessageHandler.HandleMessage(new CurrentWalletHasChangedMessage());
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
            dialogManagerMock.Verify(x => x.ShowDialog<ImportPrivateKeyLoadParameters>(null));
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
            dialogManagerMock.Verify(x => x.ShowDialog<ImportCertificateLoadParameters>(null));
        }

        [Fact]
        public void ImportWatchOnlyAddressCommand_ShowInputDialogReturnNotNullString_ShowImportPrivateKeyDialog()
        {
            // Arrange
            var expectedReturnedAddresses = @"address1
address2";
            var expectedReturnedAddressArray = expectedReturnedAddresses.ToLines();

            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();
            dialogManagerMock
                .Setup(x => x.ShowInputDialog(Strings.ImportWatchOnlyAddress, Strings.Address, It.IsAny<string>()))
                .Returns(expectedReturnedAddresses);

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
        
            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();

            // Act
            viewModel.ImportWatchOnlyAddressCommand.Execute(null);

            // Assert
            walletControllerMock.Verify(x => x.ImportWatchOnlyAddress(expectedReturnedAddressArray));
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
            walletControllerMock.Verify(x => x.ImportWatchOnlyAddress(It.IsAny<string[]>()), Times.Never);
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
            dialogManagerMock.Verify(x => x.ShowDialog<CreateMultiSigContractLoadParameters>(null));
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
            dialogManagerMock.Verify(x => x.ShowDialog<CreateLockAccountLoadParameters>(null));
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
            dialogManagerMock.Verify(x => x.ShowDialog<ImportCustomContractLoadParameters>(null));
        }

        [Fact]
        public void ViewPrivateKeyCommand_ViewPrivateKeyIsEnabled_ShowViewPrivateKeyDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;

            // Act
            accountAddedMessageHandler.HandleMessage(GetTestAccountAddedMessage());
            viewModel.SelectedAccount = viewModel.Accounts.First();
            viewModel.ViewPrivateKeyCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog(It.Is<ViewPrivateKeyLoadParameters>(p => p.ScriptHash == viewModel.SelectedAccount.ScriptHash)));
        }

        [Fact]
        public void ViewPrivateKeyCommand_ViewPrivateKeyIsNotEnabled_ShowNothing()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();
            
            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;

            // Act
            accountAddedMessageHandler.HandleMessage(GetTestAccountAddedMessage(AccountType.NonStandard));
            viewModel.SelectedAccount = viewModel.Accounts.First();
            viewModel.ViewPrivateKeyCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog(It.Is<ViewPrivateKeyLoadParameters>(p => p.ScriptHash == viewModel.SelectedAccount.ScriptHash)), Times.Never);
        }

        [Fact]
        public void ViewContractCommand_ViewContractIsEnabled_ShowViewContractDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;

            // Act
            accountAddedMessageHandler.HandleMessage(GetTestAccountAddedMessage());
            viewModel.SelectedAccount = viewModel.Accounts.First();
            viewModel.ViewContractCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog(It.Is<ViewContractLoadParameters>(p => p.ScriptHash == viewModel.SelectedAccount.ScriptHash.ToString())), Times.Once);
        }

        [Fact]
        public void ViewContractCommand_ViewContractIsNotEnabled_ShowNothing()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;

            // Act
            accountAddedMessageHandler.HandleMessage(GetTestAccountAddedMessage(AccountType.WatchOnly));
            viewModel.SelectedAccount = viewModel.Accounts.First();
            viewModel.ViewContractCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog(It.Is<ViewContractLoadParameters>(p => p.ScriptHash == viewModel.SelectedAccount.ScriptHash)), Times.Never);
        }

        [Fact]
        public void ShowVotingDialogCommand_ShowVotingDialogIsEnabled_ShowVotingDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;

            // Act
            accountAddedMessageHandler.HandleMessage(GetTestAccountAddedMessage());
            viewModel.SelectedAccount = viewModel.Accounts.First();
            viewModel.SelectedAccount.Neo = 1;
            viewModel.ShowVotingDialogCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog(It.Is<VotingLoadParameters>(p => p.VoterScriptHash == viewModel.SelectedAccount.ScriptHash)));
        }

        [Fact]
        public void ShowVotingDialogCommand_ShowVotingDialogIsNotEnabledNoBalance_ShowNothing()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;

            // Act
            accountAddedMessageHandler.HandleMessage(GetTestAccountAddedMessage());
            viewModel.SelectedAccount = viewModel.Accounts.First();
            viewModel.ShowVotingDialogCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog(new VotingLoadParameters(viewModel.SelectedAccount.ScriptHash)), Times.Never);
        }

        [Fact]
        public void ShowVotingDialogCommand_ShowVotingDialogIsNotEnabledwatchOnlyAccountNoBalance_ShowNothing()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;

            // Act
            accountAddedMessageHandler.HandleMessage(GetTestAccountAddedMessage(AccountType.WatchOnly));
            viewModel.SelectedAccount = viewModel.Accounts.First();
            viewModel.ShowVotingDialogCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog(new VotingLoadParameters(viewModel.SelectedAccount.ScriptHash)), Times.Never);
        }

        [Fact]
        public void ShowVotingDialogCommand_ShowVotingDialogIsNotEnabledwatchOnlyAccountWithBalance_ShowNothing()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;

            // Act
            accountAddedMessageHandler.HandleMessage(GetTestAccountAddedMessage(AccountType.WatchOnly));
            viewModel.SelectedAccount = viewModel.Accounts.First();
            viewModel.SelectedAccount.Neo = 1;
            viewModel.ShowVotingDialogCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog(new VotingLoadParameters(viewModel.SelectedAccount.ScriptHash)), Times.Never);
        }

        [Fact]
        public void CopyAddressToClipboardCommand_TextSetInClipboardManager()
        {
            // Arrange
            var clipboardManagetMock = this.AutoMockContainer.GetMock<IClipboardManager>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;

            // Act
            accountAddedMessageHandler.HandleMessage(GetTestAccountAddedMessage());
            viewModel.SelectedAccount = viewModel.Accounts.First();
            viewModel.CopyAddressToClipboardCommand.Execute(null);

            // Assert
            clipboardManagetMock.Verify(x => x.SetText(TestAddress));
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

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
            walletControllerMock
                .Setup(x => x.DeleteAccount(TestScriptHash))
                .Returns(true);

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;

            // Act
            accountAddedMessageHandler.HandleMessage(GetTestAccountAddedMessage());
            viewModel.SelectedAccount = viewModel.Accounts.First();
            viewModel.DeleteAccountCommand.Execute(null);

            // Assert
            walletControllerMock.Verify(x => x.DeleteAccount(viewModel.SelectedAccount.ScriptHash));
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

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;

            // Act
            accountAddedMessageHandler.HandleMessage(GetTestAccountAddedMessage());
            viewModel.SelectedAccount = viewModel.Accounts.First();
            viewModel.DeleteAccountCommand.Execute(null);

            // Assert
            walletControllerMock.Verify(x => x.DeleteAccount(viewModel.SelectedAccount.ScriptHash), Times.Never);
            Assert.Single(viewModel.Accounts);
        }

        [Fact]
        public void ViewSelectedAccountDetailsCommand_MessageResultDialogNo_AccountIsDeleted()
        {
            // Arrange
            var urlFormat = @"x:\test\{0}";
            var expectedUrl = string.Format(urlFormat, TestAddress);

            var settingsManagerMock = this.AutoMockContainer.GetMock<ISettingsManager>();
            settingsManagerMock
                .SetupGet(x => x.AddressURLFormat)
                .Returns(urlFormat);

            var processManagerMock = this.AutoMockContainer.GetMock<IProcessManager>();

            var viewModel = this.AutoMockContainer.Create<AccountsViewModel>();
            var accountAddedMessageHandler = viewModel as IMessageHandler<AccountAddedMessage>;

            // Act
            accountAddedMessageHandler.HandleMessage(GetTestAccountAddedMessage());
            viewModel.SelectedAccount = viewModel.Accounts.First();
            viewModel.ViewSelectedAccountDetailsCommand.Execute(null);

            // Assert
            processManagerMock.Verify(x => x.OpenInExternalBrowser(expectedUrl));
        }

        private static AccountAddedMessage GetTestAccountAddedMessage(AccountType accountType = AccountType.Standard)
        {
            return new AccountAddedMessage(TestAccountLabel, TestAddress, TestScriptHash, accountType);
        }
    }
}
