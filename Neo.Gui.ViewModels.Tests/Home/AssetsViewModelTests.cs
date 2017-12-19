using System.IO;
using System.Linq;
using Moq;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.ViewModels.Home;
using Neo.Gui.ViewModels.Tests.Builders;
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
            var expectedAssetItem = new AssetItemBuilder().Build();

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
            var expectedAssetItem = new AssetItemBuilder().Build();

            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();
            var assetAddedMessageHandler = viewModel as IMessageHandler<AssetAddedMessage>;
            var clearAssetsMessageHandler = viewModel as IMessageHandler<ClearAssetsMessage>;

            // Act
            assetAddedMessageHandler.HandleMessage(new AssetAddedMessage(expectedAssetItem));
            clearAssetsMessageHandler.HandleMessage(new ClearAssetsMessage());

            // Assert
            Assert.Empty(viewModel.Assets);
        }

        [Fact]
        public void ViewCertificateCommand_ShowCertificate()
        {
            // Arrange
            var expectedAddress = "Address";
            var expectedCertificatePath = @"X:\";

            var selectedAssetItem = new AssetItemBuilder().Build();

            var processHelperMock = this.AutoMockContainer.GetMock<IProcessHelper>();

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();
            walletControllerMock
                .Setup(x => x.ToAddress(It.IsAny<UInt160>()))
                .Returns(expectedAddress);

            var settingsManagerMock = this.AutoMockContainer.GetMock<ISettingsManager>();
            settingsManagerMock
                .SetupGet(x => x.CertificateCachePath)
                .Returns(expectedCertificatePath);

            var expectedPath = Path.Combine(expectedCertificatePath, $"{expectedAddress}.cer");

            var viewModel = this.AutoMockContainer.Create<AssetsViewModel>();

            // Act
            viewModel.SelectedAsset = selectedAssetItem;
            viewModel.ViewCertificateCommand.Execute(null);

            // Assert
            processHelperMock.Verify(x => x.Run(expectedPath));
        }
    }
}
