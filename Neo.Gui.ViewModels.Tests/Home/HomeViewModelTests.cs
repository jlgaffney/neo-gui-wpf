using Xunit;
using Neo.Gui.Wpf.Views.Home;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Helpers.Interfaces;
using Moq;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.MVVM;
using Neo.Gui.Wpf.Views.Contracts;
using Neo.Gui.Base.Dialogs.Results.Contracts;

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
            uint nodeCount = 1;

            var expectedHeightStatus = $"{walletHeight}/{blockChainHeight}/{blockChainHeaderHeight}";

            var viewModel = this.AutoMockContainer.Create<HomeViewModel>();
            var walletStatusMessageHandler = viewModel as IMessageHandler<WalletStatusMessage>;

            // Act
            walletStatusMessageHandler.HandleMessage(new WalletStatusMessage(
                new WalletStatus(
                    walletHeight, 
                    blockChainHeight, 
                    blockChainHeaderHeight, 
                    nextBlockProgressIsIndeterminate, 
                    nextBlockProgressFraction, 
                    nodeCount)));

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
            dialogManagerMock.Verify(x => x.ShowDialog<InvokeContractDialogResult, InvokeContractLoadParameters>(It.IsAny<LoadParameters<InvokeContractLoadParameters>>()));
        }
    }
}
