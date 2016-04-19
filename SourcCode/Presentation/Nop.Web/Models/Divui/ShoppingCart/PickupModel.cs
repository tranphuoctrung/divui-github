using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Divui.ShoppingCart
{
    public class PickupModel
    {
        public string Value { get; set; }

        public int ProductId { get; set; }

        public int ProductOptionId { get; set; }

        public int ProductAttributeId { get; set; }

        public int AttributeControlTypeId { get; set; }
    }
}