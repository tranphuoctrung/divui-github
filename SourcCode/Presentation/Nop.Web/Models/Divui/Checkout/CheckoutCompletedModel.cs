using Nop.Web.Models.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Checkout
{
    public partial class CheckoutCompletedModel
    {
        public OrderDetailsModel OrderDetails { get; set; }
    }
}