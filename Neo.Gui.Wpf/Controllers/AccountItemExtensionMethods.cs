using System.Collections.Generic;
using System.Linq;
using Neo.UI;

namespace Neo.Controllers
{
    public static class AccountItemExtensionMethods
    {
        public static AccountItem GetAccountItemForAddress(this IEnumerable<AccountItem> accountItems, string address)
        {
            return accountItems.FirstOrDefault(x => x.Address.Equals(address));
        }
    }
}
