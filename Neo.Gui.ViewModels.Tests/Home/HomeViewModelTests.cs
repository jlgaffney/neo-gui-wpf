using Xunit;
using Neo.Gui.Wpf.Views.Home;
using Neo.Gui.Base.Messaging.Interfaces;

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
    }
}
