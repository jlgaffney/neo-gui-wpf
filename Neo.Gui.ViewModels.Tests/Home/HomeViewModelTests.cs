using System;
using Xunit;

using Moq;

using Neo.Gui.Globalization.Resources;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.LoadParameters.Contracts;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Dialogs.Results.Contracts;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Dialogs.Results.Development;
using Neo.Gui.Base.Dialogs.Results.Settings;
using Neo.Gui.Base.Dialogs.Results.Assets;
using Neo.Gui.Base.Dialogs.Results.Transactions;
using Neo.Gui.Base.Dialogs.Results.Voting;
using Neo.Gui.Base.Helpers;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Status;

using Neo.Gui.ViewModels.Home;

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
        public void NewVersionAvailableMessageReceived_CorrectLabelsUpdated()
        {
            // Arrange
            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();
            var newVersionAvailableMessageHandler = viewModel as IMessageHandler<NewVersionAvailableMessage>;

            var expectedVersion = new System.Version(1, 0);

            // Act
            newVersionAvailableMessageHandler.HandleMessage(new NewVersionAvailableMessage(expectedVersion));

            // Assert
            Assert.Equal($"{Strings.DownloadNewVersion}: {expectedVersion}", viewModel.NewVersionLabel);
            Assert.True(viewModel.NewVersionVisible);
        }

        [Fact]
        public void UpdateApplicationMessageReceived_ExitAppMessagePublished()
        {
            // Arrange
            var processHelperMock = this.AutoMockContainer.GetMock<IProcessHelper>();
            var messagePublisherMock = this.AutoMockContainer.GetMock<IMessagePublisher>();

            var expectedScriptPath = "scriptPath";

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();
            var updateApplicationMessageHandler = viewModel as IMessageHandler<UpdateApplicationMessage>;

            // Act
            updateApplicationMessageHandler.HandleMessage(new UpdateApplicationMessage(expectedScriptPath));

            // Assert
            processHelperMock.Verify(x => x.Run(expectedScriptPath));
            messagePublisherMock.Verify(x => x.Publish(It.IsAny<ExitAppMessage>()));
        }

        [Fact]
        public void InvokeContractMessageReceived_ShowInvokeContractDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();
            var InvokeContractMessageHandler = viewModel as IMessageHandler<InvokeContractMessage>;

            // Act
            InvokeContractMessageHandler.HandleMessage(new InvokeContractMessage(new Core.InvocationTransaction()));

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<InvokeContractDialogResult, InvokeContractLoadParameters>(It.IsAny<InvokeContractLoadParameters>()));
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
                .Setup(x => x.ShowDialog<CreateWalletDialogResult>())
                .Returns(createWalletDialogResult);

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.CreateWalletCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<CreateWalletDialogResult>());
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
                .Setup(x => x.ShowDialog<OpenWalletDialogResult>())
                .Returns(openWalletDialogResult);

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.OpenWalletCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<OpenWalletDialogResult>());
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
            var openWalletDialogResult = new OpenWalletDialogResult(walletPath, walletPassword);

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
            walletControllerMock
                .Setup(x => x.WalletCanBeMigrated(walletPath))
                .Returns(true);

            var settingsManagerMock = this.AutoMockContainer.GetMock<ISettingsManager>();

            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();
            dialogManagerMock
                .Setup(x => x.ShowDialog<OpenWalletDialogResult>())
                .Returns(openWalletDialogResult);
            dialogManagerMock
                .Setup(x => x.ShowMessageDialog(Strings.MigrateWalletCaption, Strings.MigrateWalletMessage, MessageDialogType.YesNo, MessageDialogResult.Ok))
                .Returns(MessageDialogResult.Yes);

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.OpenWalletCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<OpenWalletDialogResult>());
            walletControllerMock.Verify(x => x.OpenWallet(walletPath, walletPassword));
            walletControllerMock.Verify(x => x.MigrateWallet(walletPath, walletPassword, null));
            settingsManagerMock.VerifySet(x => x.LastWalletPath = walletPath);
            settingsManagerMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void ChangePasswordCommand_ShowChangePasswordDialog()
        {
            // Arrange
            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.ChangePasswordCommand.Execute(null);

            // Assert
            dialogManagerMock.Verify(x => x.ShowDialog<ChangePasswordDialogResult>());
        }

        [Fact]
        public void ExitCommand_PublicExitAppMessage()
        {
            // Arrange
            var messagePublisherMock = this.AutoMockContainer.GetMock<IMessagePublisher>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.ExitCommand.Execute(null);

            // Assert
            messagePublisherMock.Verify(x => x.Publish(It.IsAny<ExitAppMessage>()));
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
            dialogManagerMock.Verify(x => x.ShowDialog<TransferDialogResult>());
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
            dialogManagerMock.Verify(x => x.ShowDialog<TradeDialogResult>());
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
            dialogManagerMock.Verify(x => x.ShowDialog<SigningDialogResult>());
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
            dialogManagerMock.Verify(x => x.ShowDialog<ClaimDialogResult>());
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
            dialogManagerMock.Verify(x => x.ShowDialog<CertificateApplicationDialogResult>());
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
            dialogManagerMock.Verify(x => x.ShowDialog<AssetRegistrationDialogResult>());
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
            dialogManagerMock.Verify(x => x.ShowDialog<AssetDistributionDialogResult>());
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
            dialogManagerMock.Verify(x => x.ShowDialog<DeployContractDialogResult>());
        }

        [Fact]
        public void InvokeContractCommand_PublishInvokeContractMessage()
        {
            // Arrange
            var messagePublisherMock = this.AutoMockContainer.GetMock<IMessagePublisher>();

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();

            // Act
            viewModel.InvokeContractCommand.Execute(null);

            // Assert
            messagePublisherMock.Verify(x => x.Publish(It.IsAny<InvokeContractMessage>()));
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
            dialogManagerMock.Verify(x => x.ShowDialog<ElectionDialogResult>());
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
            dialogManagerMock.Verify(x => x.ShowDialog<SettingsDialogResult>());
        }

        [Fact]
        public void ShowOfficialWebsiteCommand_OpenNeoWebSite()
        {
            // Arrange
            const string officialWebsiteUrl = "https://neo.org/";
            var processHelperMock = this.AutoMockContainer.GetMock<IProcessHelper>();

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
            dialogManagerMock.Verify(x => x.ShowDialog<DeveloperToolsDialogResult>());
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
            dialogManagerMock.Verify(x => x.ShowDialog<AboutDialogResult>());
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
            dialogManagerMock.Verify(x => x.ShowDialog<UpdateDialogResult>());
        }
    }
}
