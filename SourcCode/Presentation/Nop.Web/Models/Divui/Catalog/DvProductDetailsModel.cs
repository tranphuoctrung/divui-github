using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Catalog
{
    public partial class ProductDetailsModel
    {
        public ProductDetailsModel()
        {
            DefaultPictureModel = new PictureModel();
            PictureModels = new List<PictureModel>();
            GiftCard = new GiftCardModel();
            ProductPrice = new ProductPriceModel();
            AddToCart = new AddToCartModel();
            ProductAttributes = new List<ProductAttributeModel>();
            AssociatedProducts = new List<ProductDetailsModel>();
            VendorModel = new VendorBriefInfoModel();
            Breadcrumb = new ProductBreadcrumbModel();
            ProductTags = new List<ProductTagModel>();
            ProductSpecifications = new List<ProductSpecificationModel>();
            ProductManufacturers = new List<ManufacturerModel>();
            ProductReviewOverview = new ProductReviewOverviewModel();
            TierPrices = new List<TierPriceModel>();
            ProductOptions = new List<ProductOptionModel>();
            ProductReviews = new ProductReviewsModel();
        }

        public string Overview { get; set; }

        public string HightLight { get; set; }
        public string Condition { get; set; }

        public string GuideToUse { get; set; }

        public string Tip { get; set; }

        public List<ProductOptionModel> ProductOptions { get; set; }

        public ProductReviewsModel ProductReviews { get; set; }

        public CategoryModel ProductDestination { get; set; }
        public CategoryModel ProductAttraction { get; set; }
        public partial class ProductPriceModel {
            public decimal SavePercent { get; set; }
            public decimal SaveValue { get; set; }
            public string strSaveValue { get; set; }
        }
    }
}