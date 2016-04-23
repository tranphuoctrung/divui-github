using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.ShoppingCart
{
    public partial class WishlistModel
    {
        public WishlistModel()
        {
            Items = new List<ShoppingCartItemModel>();
            Products = new List<ProductOverviewModel>();
            Warnings = new List<string>();
        }
        public IList<ProductOverviewModel> Products { get; set; }
    }
}