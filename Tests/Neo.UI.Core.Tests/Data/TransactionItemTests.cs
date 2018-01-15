using System;
using Neo.Core;
using Neo.Gui.Globalization.Resources;
using Neo.UI.Core.Data;
using Xunit;

namespace Neo.Gui.Base.Tests.Data
{
    public class TransactionItemTests
    {
        [Fact]
        public void Ctor_WithValidHash_CreateValidTransactionItem()
        {
            // Arrange
            var hash = UInt256.Parse("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF");

            // Act
            var transactionItem = new TransactionItem(hash, TransactionType.ContractTransaction, uint.MinValue, DateTime.Now);

            // Assert
            Assert.IsType<TransactionItem>(transactionItem);
            Assert.Equal(Strings.Unconfirmed, transactionItem.ConfirmationsText);
        }

        [Fact]
        public void Ctor_NullHash_ThrowArgumentNullException()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new TransactionItem(null, TransactionType.ContractTransaction, uint.MinValue, DateTime.Now));
        }

        [Fact]
        public void SetConfirmations_SetOneConfirmation_OneConfirmationIsSetToConfirmationField()
        {
            // Arrange
            uint expectedConfirmations = 1;
            var hash = UInt256.Parse("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF");

            // Act
            var transactionItem = new TransactionItem(hash, TransactionType.ContractTransaction, uint.MinValue, DateTime.Now);
            transactionItem.Confirmations = expectedConfirmations;

            // Assert
            Assert.Equal(expectedConfirmations.ToString(), transactionItem.ConfirmationsText);
        }
    }
}
