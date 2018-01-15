using System.Linq;
using Moq;
using Neo.Gui.Dialogs;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.Gui.Globalization.Resources;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Home;
using Neo.Gui.ViewModels.Tests.Builders;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Data;
using Neo.UI.Core.Managers.Interfaces;
using Neo.UI.Core.Messages;
using Neo.UI.Core.Messaging.Interfaces;
using Xunit;

namespace Neo.Gui.ViewModels.Tests.Home
{
    public class AssetsViewModelTests : TestBase
    {
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

        [Fact]
        public void AssetAddedMessageReceived_AssetAdded()
        {
            // Arrange
            var expectedAssetItem = new FirstClassAssetItemBuilder().Build();

            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();
            var assetAddedMessageHandler = viewModel as IMessageHandler<AssetAddedMessage>;

            // Act
            assetAddedMessageHandler.HandleMessage(new AssetAddedMessage(expectedAssetItem));

            // Assert
            Assert.True(viewModel.Assets.Count == 1);
            Assert.Same(expectedAssetItem, viewModel.Assets.Single());
        }

        [Fact]
        public void ClearAssetsMessageReceived_AssetRemoved()
        {
            // Arrange
            var expectedAssetItem = new FirstClassAssetItemBuilder().Build();

            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();
            var assetAddedMessageHandler = viewModel as IMessageHandler<AssetAddedMessage>;
            var currentWalletHasChangedMessageHandler = viewModel as IMessageHandler<CurrentWalletHasChangedMessage>;

            // Act
            assetAddedMessageHandler.HandleMessage(new AssetAddedMessage(expectedAssetItem));
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

            var selectedAssetItem = new FirstClassAssetItemBuilder()
                .WithCustomToken()
                .Build();

            var processHelperMock = this.AutoMockContainer.GetMock<IProcessManager>();

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
            walletControllerMock
                .Setup(x => x.ViewCertificate(selectedAssetItem as FirstClassAssetItem))
                .Returns(expectedCertificatePath);
            walletControllerMock
                .Setup(x => x.CanViewCertificate(selectedAssetItem as FirstClassAssetItem))
                .Returns(true);

            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();

            // Act
            viewModel.SelectedAsset = selectedAssetItem;
            viewModel.ViewCertificateCommand.Execute(null);

            // Assert
            processHelperMock.Verify(x => x.Run(expectedCertificatePath));
        }

        [Fact]
        public void DeleteAssetCommand_ShowCertificate()
        {
            // Arrange
            var tokenId = UInt256.Parse("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF");

            var selectedAssetItem = new FirstClassAssetItemBuilder()
                .WithAssetId(tokenId)
                .WithCustomToken()
                .Build();

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
            walletControllerMock
                .Setup(x => x.GetAvailable(tokenId))
                .Returns(Fixed8.Zero);

            var dialogManagerMock = this.AutoMockContainer.GetMock<IDialogManager>();
            dialogManagerMock
                .Setup(x => x.ShowMessageDialog(Strings.DeleteConfirmation, It.IsAny<string>(), MessageDialogType.YesNo, MessageDialogResult.No))
                .Returns(MessageDialogResult.Yes);

            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();

            // Act
            viewModel.SelectedAsset = selectedAssetItem;
            viewModel.DeleteAssetCommand.Execute(null);

            // Assert
            walletControllerMock.Verify(x => x.DeleteFirstClassAsset(selectedAssetItem as FirstClassAssetItem));
        }

        [Fact]
        public void ViewSelectedAssetDetailsCommand_OpenBrowserWithAssetDetails()
        {
            // Arrange
            var expectedAssetURLFormat = @"https://www.xpto.com/{0}";
            var tokenName = "1234";
            var tokenId = UInt256.Parse("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF");

            var selectedAssetItem = new FirstClassAssetItemBuilder()
                .WithAssetId(tokenId)
                .WithName(tokenName)
                .Build();

            var processHelperMock = this.AutoMockContainer.GetMock<IProcessManager>();

            var settingsManagerMock = this.AutoMockContainer.GetMock<ISettingsManager>();
            settingsManagerMock
                .SetupGet(x => x.AssetURLFormat)
                .Returns(expectedAssetURLFormat);

            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();

            // Act
            viewModel.SelectedAsset = selectedAssetItem;
            viewModel.ViewSelectedAssetDetailsCommand.Execute(null);

            // Assert
            processHelperMock.Verify(x => x.OpenInExternalBrowser(string.Format(expectedAssetURLFormat, tokenName.Substring(2))));
        }
    }
}
