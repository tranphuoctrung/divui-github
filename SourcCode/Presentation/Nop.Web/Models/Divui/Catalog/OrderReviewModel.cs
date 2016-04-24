using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Catalog
{
    public class OrderReviewModel
    {
        public OrderReviewModel()
        {
            Items = new List<ProductReviewModel>();
        }
        public DateTime CreatedOn { get; set; }

        public int OrderId { get; set; }

        public string Date { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        public PictureModel Picture { get; set; }

        public List<ProductReviewModel> Items { get; set; }
        public string OrderStatus { get; internal set; }
        public string PaymentMethod { get; internal set; }
        public string PaymentMethodStatus { get; internal set; }
        public string OrderTotal { get; internal set; }
    }
}