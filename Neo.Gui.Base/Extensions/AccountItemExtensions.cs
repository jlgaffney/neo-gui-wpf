using System.Collections.Generic;
using System.Linq;
using Neo.Gui.Base.Data;

namespace Neo.Gui.Base.Extensions
{
    public static class AccountItemExtensions
    {
        public static AccountItem GetAccountItemForAddress(this IEnumerable<AccountItem> accountItems, string address)
        {
            return accountItems.FirstOrDefault(x => x.Address.Equals(address));
        }
    }
}
