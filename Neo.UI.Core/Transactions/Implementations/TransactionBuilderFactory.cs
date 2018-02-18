using System;
using System.Collections.Generic;
using System.Linq;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;

namespace Neo.UI.Core.Transactions.Implementations
{
    internal class TransactionBuilderFactory : ITransactionBuilderFactory
    {
        private readonly IReadOnlyList<ITransactionBuilder> builders;

        public TransactionBuilderFactory(
            IEnumerable<ITransactionBuilder> builders)
        {
            this.builders = builders.ToList();
        }

        public ITransactionBuilder<TParameters> GetBuilder<TParameters>(TParameters parameters) where TParameters : TransactionParameters
        {
            var builder = this.builders.FirstOrDefault(b => b is ITransactionBuilder<TParameters>);

            if (builder == null)
            {
                throw new InvalidOperationException("Requested an unregistered transaction builder!");
            }

            return builder as ITransactionBuilder<TParameters>;
        }
    }
}
