using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Nop.Web.Models.ShoppingCart
{
    public partial class ShoppingCartModel
    {
        public ShoppingCartModel()
        {
            Items = new List<ShoppingCartItemModel>();
            Warnings = new List<string>();
            EstimateShipping = new EstimateShippingModel();
            DiscountBox = new DiscountBoxModel();
            GiftCardBox = new GiftCardBoxModel();
            CheckoutAttributes = new List<CheckoutAttributeModel>();
            OrderReviewData = new OrderReviewDataModel();

            ButtonPaymentMethodActionNames = new List<string>();
            ButtonPaymentMethodControllerNames = new List<string>();
            ButtonPaymentMethodRouteValues = new List<RouteValueDictionary>();
            OptionItems = new List<ShoppingCartOptionModel>();
            OrderTotals = new OrderTotalsModel();

        }

        public List<ShoppingCartOptionModel> OptionItems { get; set; }

        public OrderTotalsModel OrderTotals { get; set; }

        #region Nested Classes
        public class ShoppingCartOptionModel
        {
            public ShoppingCartOptionModel()
            {
                Items = new List<ShoppingCartItemModel>();
            }
            public int ProductOptionId { get; set; }

            public string ProductOptionName { get; set; }
            public decimal Total { get; set; }

            public string strTotal { get; set; }
            public DateTime SelectedDate { get; set; }

            public List<ShoppingCartItemModel> Items { get; set; }
        }

        public partial class ShoppingCartItemModel {
            public int AgeRangeTypeId { get; set; }
            public string AgeRangeCondition { get; set; }
            public ProductDetailsModel.ProductAttributeModel ProductAttribute { get; set; }
        }
        #endregion
    }

}