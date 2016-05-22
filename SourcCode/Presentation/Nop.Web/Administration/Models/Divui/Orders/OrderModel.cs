using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Admin.Models.Common;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;


namespace Nop.Admin.Models.Orders
{
    public partial class OrderModel
    {
        #region Nested Classes

        public partial class OrderItemModel
        {
            public OrderItemModel()
            {
                ReturnRequestIds = new List<int>();
                PurchasedGiftCardIds = new List<int>();
                OrderItemAttributes = new List<OrderItemAttributeMappingModel>();
            }
            public List<OrderItemAttributeMappingModel> OrderItemAttributes { get; set; }
        }

        public partial class OrderItemAttributeMappingModel : BaseNopEntityModel
        {
            public int ProductAttributeId { get; set; }

            public string ProductAttributeName { get; set; }

            public int AttributeControlTypeId { get; set; }

            public AttributeControlType AttributeControlType
            {
                get
                {
                    return (AttributeControlType)this.AttributeControlTypeId;
                }
                set
                {
                    this.AttributeControlTypeId = (int)value;
                }
            }

            public string Value { get; set; }
        }
        #endregion
    }
}