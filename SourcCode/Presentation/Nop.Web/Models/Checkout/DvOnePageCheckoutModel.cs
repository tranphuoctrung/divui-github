using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Divui.ShoppingCart;
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
            CheckoutPaymentMethod = new CheckoutPaymentMethodModel();
            CheckoutConfirm = new CheckoutConfirmModel();
            BillingAddress = new CheckoutBillingAddressModel();
            Pickups = new List<PickupModel>();
        }
        public OrderTotalsModel OrderTotals { get; set; }

        public CheckoutBillingAddressModel BillingAddress { get; set; }

        //phương thức thanh toán
        public CheckoutPaymentMethodModel CheckoutPaymentMethod { get; set; }

        public string SelectedPaymentMethodSystem { get; set; }

        public DiscountBoxModel DiscountBox { get; set; }

        public bool TermsOfServiceOnShoppingCartPage { get; set; }

        public bool CouponCollapsed { get; set; }

        //thông tin giỏ hàng
        public CheckoutConfirmModel CheckoutConfirm { get; set; }

        public List<PickupModel> Pickups { get; set; }

        public partial class DiscountBoxModel : BaseNopModel
        {
            public bool Display { get; set; }
            public string Message { get; set; }
            public string CurrentCode { get; set; }
            public bool IsApplied { get; set; }
        }
    }
}