using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
    public partial class OrderItem
    {
        private ICollection<OrderItemAttributeMapping> _orderItemAttributeMapping;
        public virtual ICollection<OrderItemAttributeMapping> OrderItemAttributeMappings
        {
            get { return _orderItemAttributeMapping ?? (_orderItemAttributeMapping = new List<OrderItemAttributeMapping>()); }
            protected set { _orderItemAttributeMapping = value; }
        }
    }
}
