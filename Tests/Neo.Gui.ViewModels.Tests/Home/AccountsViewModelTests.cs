using Neo.Gui.Base.Messaging.Interfaces;
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
    }
}
