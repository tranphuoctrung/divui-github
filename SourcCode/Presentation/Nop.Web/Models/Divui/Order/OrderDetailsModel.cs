using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Order
{
    public partial class OrderDetailsModel
    {
        public OrderDetailsModel()
        {
            TaxRates = new List<TaxRate>();
            GiftCards = new List<GiftCard>();
            Items = new List<OrderItemModel>();
            OrderNotes = new List<OrderNote>();
            Shipments = new List<ShipmentBriefModel>();

            BillingAddress = new AddressModel();
            ShippingAddress = new AddressModel();

            CustomValues = new Dictionary<string, object>();

            OptionItems = new List<OrderItemOptionModel>();
        }

        public List<OrderItemOptionModel> OptionItems { get; set; }

        

        #region Nested Classes

        public class OrderItemOptionModel
        {
            public OrderItemOptionModel()
            {
                Items = new List<OrderItemModel>();
            }
            public int ProductOptionId { get; set; }

            public string ProductOptionName { get; set; }
            public string SeName { get; set; }
            public decimal Total { get; set; }

            public string strTotal { get; set; }
            public OrderItemAttributeModel SelectedDateAttribute { get; set; }



            public OrderItemAttributeModel PickupAttribute { get; set; }

            public List<OrderItemModel> Items { get; set; }

            public PictureModel Picture { get; set; }

            public int AdultNumber { get; set; }

            public int ChildNumber { get; set; }

            public int KidNumber { get; set; }

            public int SeniorNumber { get; set; }
        }

        public partial class OrderItemModel
        {
            public int AgeRangeTypeId { get; set; }
            public string AgeRangeCondition { get; set; }
        }

        public partial class OrderItemAttributeModel
        {
            public int ProductId { get; set; }
            public int ProductAttributeId { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public AttributeControlType AttributeControlType { get; set; }
        }

        #endregion
    }
}