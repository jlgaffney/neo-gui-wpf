using System;
using System.Collections.Generic;
using Neo.Core;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Home;
using Xunit;

namespace Neo.Gui.ViewModels.Tests.Home
{
    public class TransactionsViewModelTests : TestBase
    {
        [Fact]
        public void Ctr_CreateValidHomeViewModel()
        {
            // Arrange
            // Act
            var viewModel = this.AutoMockContainer.Create<TransactionsViewModel>();

            //Assert
            Assert.IsType<TransactionsViewModel>(viewModel);
        }

        [Fact]
        public void Load_SubscribeMessages_SubscribeMethodCalledWithRightParameter()
        {
            // Arrange
            var messageSubscriberMock = this.AutoMockContainer.GetMock<IMessageSubscriber>();
            var viewModel = this.AutoMockContainer.Create<TransactionsViewModel>();

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
            var viewModel = this.AutoMockContainer.Create<TransactionsViewModel>();

            // Act
            viewModel.OnUnload();

            // Assert
            messageSubscriberMock.Verify(x => x.Unsubscribe(viewModel));
        }

        [Fact]
        public void TransactionsHaveChangedMessageReceived_TransactionsAdded()
        {
            // Arrange
            var viewModel = this.AutoMockContainer.Create<TransactionsViewModel>();
            var transactionsHaveChangedMessageHandler = viewModel as IMessageHandler<TransactionsHaveChangedMessage>;

            var hash = UInt256.Parse("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF");
            var transactions = new List<TransactionItem>
            {
                new TransactionItem(hash, TransactionType.ContractTransaction, uint.MinValue, DateTime.Now)
            };

            // Act
            var transactionsCount = viewModel.Transactions.Count;
            transactionsHaveChangedMessageHandler.HandleMessage(new TransactionsHaveChangedMessage(transactions));

            // Assert
            Assert.Equal(0, transactionsCount);
            Assert.Single(viewModel.Transactions);
        }

        [Fact]
        public void ClearTransactionsMessageReceived_TransactionsAreClear()
        {
            // Arrange
            var viewModel = this.AutoMockContainer.Create<TransactionsViewModel>();
            var transactionsHaveChangedMessageHandler = viewModel as IMessageHandler<TransactionsHaveChangedMessage>;
            var clearTransactionsMessageHandler = viewModel as IMessageHandler<ClearTransactionsMessage>;

            var hash = UInt256.Parse("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF");
            var transactions = new List<TransactionItem>
            {
                new TransactionItem(hash, TransactionType.ContractTransaction, uint.MinValue, DateTime.Now)
            };

            // Act
            transactionsHaveChangedMessageHandler.HandleMessage(new TransactionsHaveChangedMessage(transactions));
            var afterAddTransactionsCount = viewModel.Transactions.Count;
            clearTransactionsMessageHandler.HandleMessage(new ClearTransactionsMessage());

            // Assert
            Assert.Equal(1, afterAddTransactionsCount);
            Assert.Empty(viewModel.Transactions);
        }
    }
}
