using System.Linq;
using Moq;
using Neo.Gui.Dialogs;
using Neo.Gui.Dialogs.Interfaces;
using Neo.UI.Core.Globalization.Resources;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Home;
using Neo.UI.Core.Data.Enums;
using Neo.UI.Core.Messaging.Interfaces;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Wallet;
using Neo.UI.Core.Wallet.Messages;
using Xunit;

namespace Neo.Gui.ViewModels.Tests.Home
{
    public class AssetsViewModelTests : TestBase
    {
        private const string TestFirstClassAssetId = "0xa32e21715c0b0dcd10fba7b38a6a13797a94ddfad8743d626512f454753570f9";
        private const string TestNEP5AssetId = "0x4f05a52bc2b103886685e08cefb740cfe1bf82cd";
        private const string TestAssetName = "NEO Asset";

        [Fact]
        public void Ctr_CreateValidAssetsViewModel()
        {
            // Arrange
            // Act
            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();

            //Assert
            Assert.IsType<AssetsViewModel>(viewModel);
            Assert.NotNull(viewModel.Assets);
        }

        [Fact]
        public void Load_SubscribeMessages_SubscribeMethodCalledWithRightParameter()
        {
            // Arrange
            var messageSubscriberMock = this.AutoMockContainer.GetMock<IMessageSubscriber>();
            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();

            // Act
            viewModel.OnLoad();

            // Assert
            messageSubscriberMock.Verify(x => x.Subscribe(viewModel));
            Assert.Empty(viewModel.Assets);
        }

        [Fact]
        public void Unload_UnsubscibeMessages_UnsubscribeMethodCalledWithRightParameter()
        {
            // Arrange
            var messageSubscriberMock = this.AutoMockContainer.GetMock<IMessageSubscriber>();
            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();

            // Act
            viewModel.OnUnload();

            // Assert
            messageSubscriberMock.Verify(x => x.Unsubscribe(viewModel));
        }

        // TODO Reimplement tests
        /*[Fact]
        public void AssetAddedMessageReceived_AssetAdded()
        {
            // Arrange
            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();
            var assetAddedMessageHandler = viewModel as IMessageHandler<AssetAddedMessage>;

            // Act
            assetAddedMessageHandler.HandleMessage(GetTestAssetAddedMessage());

            // Assert
            Assert.True(viewModel.Assets.Count == 1);
            Assert.Equal(TestFirstClassAssetId, viewModel.Assets.Single().AssetId);
        }

        [Fact]
        public void ClearAssetsMessageReceived_AssetRemoved()
        {
            // Arrange
            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();
            var assetAddedMessageHandler = viewModel as IMessageHandler<AssetAddedMessage>;
            var currentWalletHasChangedMessageHandler = viewModel as IMessageHandler<CurrentWalletHasChangedMessage>;

            // Act
            assetAddedMessageHandler.HandleMessage(GetTestAssetAddedMessage());
            var afterAddAssetsCount = viewModel.Assets.Count;
            currentWalletHasChangedMessageHandler.HandleMessage(new CurrentWalletHasChangedMessage());

            // Assert
            Assert.Equal(1, afterAddAssetsCount);
            Assert.Empty(viewModel.Assets);
        }

        [Fact]
        public void ViewCertificateCommand_ShowCertificate()
        {
            // Arrange
            var expectedCertificatePath = @"X:\";

            var processHelperMock = this.AutoMockContainer.GetMock<IProcessManager>();

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
            walletControllerMock
                .Setup(x => x.ViewCertificate(TestFirstClassAssetId))
                .Returns(expectedCertificatePath);
            walletControllerMock
                .Setup(x => x.CanViewCertificate(TestFirstClassAssetId))
                .Returns(true);

            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();
            var assetAddedMessageHandler = viewModel as IMessageHandler<AssetAddedMessage>;

            // Act
            assetAddedMessageHandler.HandleMessage(GetTestAssetAddedMessage());
            viewModel.SelectedAsset = viewModel.Assets.Single();
            viewModel.ViewCertificateCommand.Execute(null);

            // Assert
            processHelperMock.Verify(x => x.Run(expectedCertificatePath));
        }

        [Fact]
        public void DeleteAssetCommand_DeleteFirstClassAsset()
        {
            // Arrange
            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();

            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();
            dialogManagerMock
                .Setup(x => x.ShowMessageDialog(Strings.DeleteConfirmation, It.IsAny<string>(), MessageDialogType.YesNo, MessageDialogResult.No))
                .Returns(MessageDialogResult.Yes);

            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();
            var assetAddedMessageHandler = viewModel as IMessageHandler<AssetAddedMessage>;

            // Act
            assetAddedMessageHandler.HandleMessage(GetTestAssetAddedMessage());
            viewModel.SelectedAsset = viewModel.Assets.Single();
            viewModel.DeleteAssetCommand.Execute(null);

            // Assert
            walletControllerMock.Verify(x => x.DeleteFirstClassAsset(viewModel.SelectedAsset.AssetId));
        }

        [Fact]
        public void ViewSelectedAssetDetailsCommand_OpenBrowserWithAssetDetails()
        {
            // Arrange
            var expectedAssetURLFormat = @"https://www.xpto.com/{0}";

            var processHelperMock = this.AutoMockContainer.GetMock<IProcessManager>();

            var settingsManagerMock = this.AutoMockContainer.GetMock<ISettingsManager>();
            settingsManagerMock
                .SetupGet(x => x.AssetURLFormat)
                .Returns(expectedAssetURLFormat);

            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();
            var assetAddedMessageHandler = viewModel as IMessageHandler<AssetAddedMessage>;

            // Act
            assetAddedMessageHandler.HandleMessage(GetTestAssetAddedMessage());
            viewModel.SelectedAsset = viewModel.Assets.Single();
            viewModel.ViewSelectedAssetDetailsCommand.Execute(null);

            // Assert
            processHelperMock.Verify(x => x.OpenInExternalBrowser(string.Format(expectedAssetURLFormat, viewModel.SelectedAsset.AssetId.Substring(2))));
        }

        private static AssetAddedMessage GetTestAssetAddedMessage(TokenType tokenType = TokenType.FirstClassToken)
        {
            return new AssetAddedMessage(tokenType == TokenType.FirstClassToken ? TestFirstClassAssetId : TestNEP5AssetId, TestAssetName, string.Empty, string.Empty, tokenType, false, "0");
        }*/
    }
}
