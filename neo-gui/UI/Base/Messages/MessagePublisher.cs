using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.UI.Base.Messages
{
    public class MessagePublisher : IMessagePublisher
    {
        #region Private Fields 
        private readonly IMessageAggregator _messageAggregator;
        #endregion

        #region Constructor 
        public MessagePublisher(IMessageAggregator messageAggregator)
        {
            this._messageAggregator = messageAggregator;
        }
        #endregion

        #region IMessagePublisher implementation
        public void Publish<T>(T message)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
