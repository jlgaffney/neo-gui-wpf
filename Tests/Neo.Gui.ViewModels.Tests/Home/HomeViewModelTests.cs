using System;

using Xunit;

using Moq;

using Neo.Gui.Globalization.Resources;
using Neo.Gui.Dialogs;
using Neo.Gui.Dialogs.LoadParameters;
using Neo.Gui.Dialogs.LoadParameters.Assets;
using Neo.Gui.Dialogs.LoadParameters.Contracts;
using Neo.Gui.Dialogs.LoadParameters.Development;
using Neo.Gui.Dialogs.LoadParameters.Settings;
using Neo.Gui.Dialogs.LoadParameters.Transactions;
using Neo.Gui.Dialogs.LoadParameters.Updater;
using Neo.Gui.Dialogs.LoadParameters.Voting;
using Neo.Gui.Dialogs.LoadParameters.Wallets;
using Neo.Gui.Dialogs.Results.Wallets;
using Neo.Gui.ViewModels.Home;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.Gui.TestHelpers;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Managers.Interfaces;
using Neo.UI.Core.Messages;
using Neo.UI.Core.Messaging.Interfaces;
using Neo.UI.Core.Status;

namespace Neo.Gui.ViewModels.Tests.Home
{
    public class HomeViewModelTests : TestBase
    {
        [Fact]
        public void Ctr_CreateValidHomeViewModel()
        {
            // Arrange
            // Act
            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            //Assert
            Assert.IsType<HomeViewModel>(viewModel);
        }

        [Fact]
        public void Load_SubscribeMessages_SubscribeMethodCalledWithRightParameter()
        {
            // Arrange
            var messageSubscriberMock = this.AutoMockContainer.GetMock<IMessageSubscriber>();
            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

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
            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.OnUnload();

            // Assert
            messageSubscriberMock.Verify(x => x.Unsubscribe(viewModel));
        }

        [Fact]
        public void WalletStatusMessageReceived_BlockChainStatusUpdated()
        {
            // Arrange
            uint walletHeight = 0;
            uint blockChainHeight = 0;
            uint blockChainHeaderHeight = 0;
            bool nextBlockProgressIsIndeterminate = false;
            double nextBlockProgressFraction = 0;
            TimeSpan timeSinceLastBlock = TimeSpan.Zero;
            int nodeCount = 1;

            var expectedHeightStatus = $"{walletHeight}/{blockChainHeight}/{blockChainHeaderHeight}";

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();
            var walletStatusMessageHandler = viewModel as IMessageHandler<WalletStatusMessage>;

            // Act
            walletStatusMessageHandler.HandleMessage(new WalletStatusMessage(
                new WalletStatus(
                    walletHeight, 
                    new BlockchainStatus(
                        blockChainHeight, 
                        blockChainHeaderHeight, 
                        nextBlockProgressIsIndeterminate, 
                        nextBlockProgressFraction,
                        timeSinceLastBlock),
                    new NetworkStatus(nodeCount))));

            // Assert
            Assert.Equal(expectedHeightStatus, viewModel.HeightStatus);
            Assert.Equal(nextBlockProgressIsIndeterminate, viewModel.NextBlockProgressIsIndeterminate);
            Assert.Equal(nextBlockProgressFraction, viewModel.NextBlockProgressFraction);
            Assert.Equal(nodeCount, viewModel.NodeCount);
            Assert.Equal($"{Strings.WaitingForNextBlock}:", viewModel.BlockStatus);
        }

        [Fact]
        public void CreateWalletCommand_ShowCreateWalletDialogAndCallWalletController()
        {
            // Arrange
            var walletPath = "walletPath";
            var walletPassword = "walletPassword";
            var createWalletWithAccount = true;
            var createWalletDialogResult = new CreateWalletDialogResult(walletPath, walletPassword);

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();

            var settingsManagerMock = this.AutoMockContainer.GetMock<ISettingsManager>();

            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();
            dialogManagerMock
                .Setup(x => x.ShowDialog<CreateWalletLoadParameters, CreateWalletDialogResult>(null))
                .Returns(createWalletDialogResult);

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.CreateWalletCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<CreateWalletLoadParameters, CreateWalletDialogResult>(null));
            walletControllerMock.Verify(x => x.CreateWallet(walletPath, walletPassword, createWalletWithAccount));
            settingsManagerMock.VerifySet(x => x.LastWalletPath = walletPath);
            settingsManagerMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void OpenWalletCommand_WalletDoesNotNeedMigrating_ShowOpenDialogAndCallWalletController()
        {
            // Arrange
            var walletPath = "walletPath";
            var walletPassword = "walletPassword";
            var openWalletDialogResult = new OpenWalletDialogResult(walletPath, walletPassword);

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
            walletControllerMock
                .Setup(x => x.WalletCanBeMigrated(walletPath))
                .Returns(false);

            var settingsManagerMock = this.AutoMockContainer.GetMock<ISettingsManager>();

            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();
            dialogManagerMock
                .Setup(x => x.ShowDialog<OpenWalletLoadParameters, OpenWalletDialogResult>(null))
                .Returns(openWalletDialogResult);

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.OpenWalletCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<OpenWalletLoadParameters, OpenWalletDialogResult>(null));
            walletControllerMock.Verify(x => x.OpenWallet(walletPath, walletPassword));
            settingsManagerMock.VerifySet(x => x.LastWalletPath = walletPath);
            settingsManagerMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void OpenWalletCommand_WalletDoesNeedMigrating_ShowOpenDialogAndCallWalletController()
        {
            // Arrange
            var walletPath = "walletPath";
            var walletPassword = "walletPassword";
            var newWalletPath = "newWalletPath";
            var openWalletDialogResult = new OpenWalletDialogResult(walletPath, walletPassword);

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
            walletControllerMock
                .Setup(x => x.WalletCanBeMigrated(walletPath))
                .Returns(true);
            walletControllerMock
                .Setup(x => x.MigrateWallet(walletPath, walletPassword))
                .Returns(newWalletPath);

            var settingsManagerMock = this.AutoMockContainer.GetMock<ISettingsManager>();

            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();
            dialogManagerMock
                .Setup(x => x.ShowDialog<OpenWalletLoadParameters, OpenWalletDialogResult>(null))
                .Returns(openWalletDialogResult);
            dialogManagerMock
                .Setup(x => x.ShowMessageDialog(Strings.MigrateWalletCaption, Strings.MigrateWalletMessage, MessageDialogType.YesNo, MessageDialogResult.Ok))
                .Returns(MessageDialogResult.Yes);

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.OpenWalletCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<OpenWalletLoadParameters, OpenWalletDialogResult>(null));
            walletControllerMock.Verify(x => x.MigrateWallet(walletPath, walletPassword));
            walletControllerMock.Verify(x => x.OpenWallet(newWalletPath, walletPassword));
            settingsManagerMock.VerifySet(x => x.LastWalletPath = newWalletPath);
            settingsManagerMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void TransferCommand_TransferDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.TransferCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<TransferLoadParameters>(null));
        }

        [Fact]
        public void ShowTransactionDialogCommand_TradeDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.ShowTransactionDialogCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<TradeLoadParameters>(null));
        }

        [Fact]
        public void ShowSigningDialogCommand_ShowSigningDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.ShowSigningDialogCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<SigningLoadParameters>(null));
        }

        [Fact]
        public void ClaimCommand_ShowClaimDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.ClaimCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<ClaimLoadParameters>(null));
        }

        [Fact]
        public void RequestCertificateCommand_ShowCertificateApplicationDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.RequestCertificateCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<CertificateApplicationLoadParameters>(null));
        }

        [Fact]
        public void AssetRegistrationCommand_ShowAssetRegistrationDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.AssetRegistrationCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<AssetRegistrationLoadParameters>(null));
        }

        [Fact]
        public void DistributeAssetCommand_ShowAssetDistributionDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.DistributeAssetCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<AssetDistributionLoadParameters>(null));
        }

        [Fact]
        public void DeployContractCommand_ShowDeployContractDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.DeployContractCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<DeployContractLoadParameters>(null));
        }

        [Fact]
        public void InvokeContractCommand_PublishInvokeContractMessage()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.InvokeContractCommand.Execute(null);
            
            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<InvokeContractLoadParameters>(null));
        }

        [Fact]
        public void ShowElectionDialogCommand_ShowElectionDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.ShowElectionDialogCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<ElectionLoadParameters>(null));
        }

        [Fact]
        public void ShowSettingsCommand_ShowSettingsDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.ShowSettingsCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<SettingsLoadParameters>(null));
        }

        [Fact]
        public void ShowOfficialWebsiteCommand_OpenNeoWebSite()
        {
            // Arrange
            const string officialWebsiteUrl = "https://neo.org/";
            var processHelperMock = this.AutoMockContainer.GetMock<IProcessManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.ShowOfficialWebsiteCommand.Execute(null);

            // Assert
            processHelperMock.Verify(x => x.OpenInExternalBrowser(officialWebsiteUrl));
        }

        [Fact]
        public void ShowDeveloperToolsCommand_ShowAboutDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.ShowDeveloperToolsCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<DeveloperToolsLoadParameters>(null));
        }

        [Fact]
        public void AboutNeoCommand_ShowDeveloperToolsDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.AboutNeoCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<AboutLoadParameters>(null));
        }

        [Fact]
        public void ShowUpdateDialogCommand_ShowUpdateDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.ShowUpdateDialogCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<UpdateLoadParameters>(null));
        }
    }
}
