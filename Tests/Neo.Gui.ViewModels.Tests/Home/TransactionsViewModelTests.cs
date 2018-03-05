using System;
using System.Linq;
using Neo.Core;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Home;
using Neo.UI.Core.Data;
using Neo.UI.Core.Messaging.Interfaces;
using Neo.UI.Core.Wallet.Messages;
using Xunit;

namespace Neo.Gui.ViewModels.Tests.Home
{
    public class TransactionsViewModelTests : TestBase
    {
        private const string TestTransactionId = "0x83ce6a47e8b38a925d421ddaec7c0e6f5b165edf63175b7cc94ef7dfe7c569fb";

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

            // Act
            var transactionsCount = viewModel.Transactions.Count;
            transactionAddedMessageHandler.HandleMessage(GetTestTransactionAddedMessage());

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
            var walletStatusMessageHandler = viewModel as IMessageHandler<WalletStatusMessage>;

            // Act
            transactionAddedMessageHandler.HandleMessage(GetTestTransactionAddedMessage());
            var transactionConfirmations = viewModel.Transactions.Single().ConfirmationsValue;
            walletStatusMessageHandler.HandleMessage(new WalletStatusMessage(uint.MinValue + 10, new BlockchainStatus(uint.MinValue + 20, uint.MinValue + 30, true, 0.5, new TimeSpan(0, 0, 10), 5)));

            // Assert
            Assert.True(transactionConfirmations < viewModel.Transactions.Single().ConfirmationsValue);
        }

        [Fact]
        public void ClearTransactionsMessageReceived_TransactionsAreClear()
        {
            // Arrange
            var viewModel = this.AutoMockContainer.Create<TransactionsViewModel>();
            var transactionAddedMessageHandler = viewModel as IMessageHandler<TransactionAddedMessage>;
            var currentWalletHasChangedMessageHandler = viewModel as IMessageHandler<CurrentWalletHasChangedMessage>;

            // Act
            transactionAddedMessageHandler.HandleMessage(GetTestTransactionAddedMessage());
            var afterAddTransactionsCount = viewModel.Transactions.Count;
            currentWalletHasChangedMessageHandler.HandleMessage(new CurrentWalletHasChangedMessage());

            // Assert
            Assert.Equal(1, afterAddTransactionsCount);
            Assert.Empty(viewModel.Transactions);
        }

        private static TransactionAddedMessage GetTestTransactionAddedMessage(TransactionType transactionType = TransactionType.ContractTransaction)
        {
            return new TransactionAddedMessage(TestTransactionId, DateTime.UtcNow, uint.MinValue, transactionType.ToString());
        }
    }
}
