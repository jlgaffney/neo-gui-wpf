using System;
using System.Collections.Generic;

using Neo.Core;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;

namespace Neo.Gui.Base.Controllers
{
    public class BaseBlockchainController : IBaseBlockchainController
    {
        #region Private fields

        private readonly IMessagePublisher messagePublisher;

        private DateTime timeOfLastBlock = DateTime.MinValue;

        #endregion

        #region Constructor

        public BaseBlockchainController(
            IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;
        }

        #endregion

        #region IBaseBlockchainController implementation

        public RegisterTransaction GoverningToken => Blockchain.GoverningToken;

        public RegisterTransaction UtilityToken => Blockchain.UtilityToken;

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true)
        {
            return Blockchain.CalculateBonus(inputs, ignoreClaimed);
        }

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd)
        {
            return Blockchain.CalculateBonus(inputs, heightEnd);
        }

        #endregion

        #region Protected methods

        protected void BlockAdded(object sender, Block block)
        {
            this.timeOfLastBlock = DateTime.UtcNow;

            this.messagePublisher.Publish(new BlockAddedMessage());
        }

        protected TimeSpan GetTimeSinceLastBlock()
        {
            return DateTime.UtcNow - this.timeOfLastBlock;
        }

        #endregion
    }
}
