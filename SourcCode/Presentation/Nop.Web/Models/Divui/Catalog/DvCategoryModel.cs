using Nop.Core.Domain.Catalog;
using Nop.Web.Framework;
using Nop.Web.Models.Divui.Catalog;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Web.Models.Catalog
{
    public partial class CategoryModel
    {
        public CategoryModel()
        {
            PictureModel = new PictureModel();
            FeaturedProducts = new List<ProductOverviewModel>();
            Products = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogPagingFilteringModel();
            SubCategories = new List<SubCategoryModel>();
            CategoryBreadcrumb = new List<CategoryModel>();
            Collections = new List<CategoryModel>();
            Attractions = new List<CategoryModel>();
            Categories = new List<CategoryModel>();
            CategorySpecifications = new List<CategorySpecificationModel>();
        }


        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.ShortDescription")]
        [AllowHtml]
        public string ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the AttractionDescription
        /// </summary>
        public string AttractionDescription { get; set; }

        /// <summary>
        /// Gets or sets the Guide
        /// </summary>
        public string Guide { get; set; }

        public List<SpecificationAttribute> SpecificationAttributes { get; set; }

        public List<int> AlreadyFilteredSpecOptionIds { get; set; }

        public int NumberOfProducts { get; set; }

        public List<CategoryModel> Collections { get; set; }

        public List<CategoryModel> Attractions { get; set; }

        public List<CategoryModel> Categories { get; set; }

        public string ParentName { get; set; }

        public List<CategorySpecificationModel> CategorySpecifications { get; set; }

        public int ParentCategoryId { get; set; }

        #region Nested Classes

        public partial class SubCategoryModel 
        {
            public int NumberOfProducts { get; set; }
        }

        #endregion
    }


}