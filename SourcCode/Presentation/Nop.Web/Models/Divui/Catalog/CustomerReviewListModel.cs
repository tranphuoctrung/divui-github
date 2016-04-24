using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Catalog
{
    public partial class ProductReviewsModel
    {
        public string Date { get; set; }
        public int OrderId { get; set; }

        public PictureModel Picture { get; set; }

    }
    public class CustomerReviewListModel
    {
        public CustomerReviewListModel()
        {
            OrderReviews = new List<OrderReviewModel>();
        }
        public List<OrderReviewModel> OrderReviews { get; set; }
    }
}