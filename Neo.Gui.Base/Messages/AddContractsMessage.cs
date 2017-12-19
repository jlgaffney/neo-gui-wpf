using System.Collections.Generic;
using System.Linq;

using Neo.SmartContract;

namespace Neo.Gui.Base.Messages
{
    public class AddContractsMessage
    {
        #region Public Properties 
        public List<Contract> Contracts { get; }
        #endregion

        #region Constructor 
        public AddContractsMessage(IEnumerable<Contract> contracts)
        {
            this.Contracts = contracts.ToList();
        }
        #endregion
    }
}
