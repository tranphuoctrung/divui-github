using Nop.Web.Framework.Mvc;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Nop.Web.Models.Checkout
{
    public partial class OnePageCheckoutModel
    {
        public OnePageCheckoutModel()
        {
            DiscountBox = new DiscountBoxModel();
        }
        public OrderTotalsModel OrderTotals { get; set; }

        public CheckoutBillingAddressModel BillingAddress { get; set; }

        public DiscountBoxModel DiscountBox { get; set; }

        public bool TermsOfServiceOnShoppingCartPage { get; set; }

        public bool CouponCollapsed { get; set; }

        public partial class DiscountBoxModel : BaseNopModel
        {
            public bool Display { get; set; }
            public string Message { get; set; }
            public string CurrentCode { get; set; }
            public bool IsApplied { get; set; }
        }
    }
}