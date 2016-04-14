using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Catalog
{
    public partial class ProductOverviewModel
    {
        public ProductOverviewModel()
        {
            ProductPrice = new ProductPriceModel();
            DefaultPictureModel = new PictureModel();
            SpecificationAttributeModels = new List<ProductSpecificationModel>();
            ReviewOverviewModel = new ProductReviewOverviewModel();
            ProductOptions = new List<ProductOptionModel>();
        }

        public ProductDetailsModel.ProductPriceModel DvProductPrice { get; set; }
        public List<ProductOptionModel> ProductOptions { get; set; }


        #region Nested Classes

        public partial class ProductPriceModel
        {
            public decimal SavePercent { get; internal set; }
        }
        #endregion
    }
}