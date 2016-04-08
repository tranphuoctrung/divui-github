using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    public partial class PriceSetup : BaseEntity
    {
        public virtual int ProductId { get; set; }
        public virtual int CustomerRoleId { get; set; }
        public virtual DateTime FromDate { get; set; }
        public virtual DateTime ToDate { get; set; }
        public virtual int Quantity { get; set; }
        public virtual decimal Price { get; set; }
        public virtual Product Product { get; set; }
        public virtual CustomerRole CustomerRole { get; set; }

    }
}
