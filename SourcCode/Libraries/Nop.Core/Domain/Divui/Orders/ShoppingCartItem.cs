using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
    public partial class ShoppingCartItem
    {
        private ICollection<ShoppingCartItemAttributeMapping> _shoppingCartItemAttributeMapping;
        public virtual ICollection<ShoppingCartItemAttributeMapping> ShoppingCartItemAttributeMappings
        {
            get { return _shoppingCartItemAttributeMapping ?? (_shoppingCartItemAttributeMapping = new List<ShoppingCartItemAttributeMapping>()); }
            protected set { _shoppingCartItemAttributeMapping = value; }
        }
    }
}
