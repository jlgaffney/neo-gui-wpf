using System;
using Neo.Core;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Home;
using Neo.UI.Core.Data;
using Neo.UI.Core.Messages;
using Neo.UI.Core.Messaging.Interfaces;
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
        public void TransactionAddedMessageReceived_TransactionsAdded()
        {
            // Arrange
            var viewModel = this.AutoMockContainer.Create<TransactionsViewModel>();
            var transactionAddedMessageHandler = viewModel as IMessageHandler<TransactionAddedMessage>;

            var hash = UInt256.Parse("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF");
            var transaction = new TransactionItem(hash, TransactionType.ContractTransaction, uint.MinValue, DateTime.Now);

            // Act
            var transactionsCount = viewModel.Transactions.Count;
            transactionAddedMessageHandler.HandleMessage(new TransactionAddedMessage(transaction));

            // Assert
            Assert.Equal(0, transactionsCount);
            Assert.Single(viewModel.Transactions);
        }

        [Fact]
        public void TransactionConfirmationsUpdatedMessageReceived_TransactionsAdded()
        {
            // Arrange
            var viewModel = this.AutoMockContainer.Create<TransactionsViewModel>();
            var transactionAddedMessageHandler = viewModel as IMessageHandler<TransactionAddedMessage>;
            var transactionConfirmationsUpdatedMessageHandler = viewModel as IMessageHandler<TransactionConfirmationsUpdatedMessage>;

            var hash = UInt256.Parse("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF");
            var transaction = new TransactionItem(hash, TransactionType.ContractTransaction, uint.MinValue, DateTime.Now);

            // Act
            transactionAddedMessageHandler.HandleMessage(new TransactionAddedMessage(transaction));

            var transactionConfirmations = viewModel.Transactions[0].Confirmations;

            transactionConfirmationsUpdatedMessageHandler.HandleMessage(new TransactionConfirmationsUpdatedMessage(10));

            // Assert
            Assert.True(transactionConfirmations < viewModel.Transactions[0].Confirmations);
        }

        [Fact]
        public void ClearTransactionsMessageReceived_TransactionsAreClear()
        {
            // Arrange
            var viewModel = this.AutoMockContainer.Create<TransactionsViewModel>();
            var transactionAddedMessageHandler = viewModel as IMessageHandler<TransactionAddedMessage>;
            var currentWalletHasChangedMessageHandler = viewModel as IMessageHandler<CurrentWalletHasChangedMessage>;

            var hash = UInt256.Parse("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF");
            var transaction = new TransactionItem(hash, TransactionType.ContractTransaction, uint.MinValue, DateTime.Now);
            
            // Act
            transactionAddedMessageHandler.HandleMessage(new TransactionAddedMessage(transaction));
            var afterAddTransactionsCount = viewModel.Transactions.Count;
            currentWalletHasChangedMessageHandler.HandleMessage(new CurrentWalletHasChangedMessage());

            // Assert
            Assert.Equal(1, afterAddTransactionsCount);
            Assert.Empty(viewModel.Transactions);
        }
    }
}
