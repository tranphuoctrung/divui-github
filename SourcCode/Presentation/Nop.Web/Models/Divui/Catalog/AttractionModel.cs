using System.Collections.Generic;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial class AttractionModel : BaseNopEntityModel
    {
        public AttractionModel()
        {
            PictureModel = new PictureModel();
            FeaturedProducts = new List<ProductOverviewModel>();
            Products = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogPagingFilteringModel();
            SubAttractions = new List<SubAttractionModel>();
            AttractionBreadcrumb = new List<AttractionModel>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }

        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
        
        public PictureModel PictureModel { get; set; }

        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }

        public bool DisplayAttractionBreadcrumb { get; set; }
        public IList<AttractionModel> AttractionBreadcrumb { get; set; }
        
        public IList<SubAttractionModel> SubAttractions { get; set; }

        public IList<ProductOverviewModel> FeaturedProducts { get; set; }
        public IList<ProductOverviewModel> Products { get; set; }

        public int NumberOfProducts { get; set; }

        #region Nested Classes

        public partial class SubAttractionModel : BaseNopEntityModel
        {
            public SubAttractionModel()
            {
                PictureModel = new PictureModel();
            }

            public string Name { get; set; }

            public string SeName { get; set; }

            public string Description { get; set; }

            public PictureModel PictureModel { get; set; }
        }

		#endregion
    }
}