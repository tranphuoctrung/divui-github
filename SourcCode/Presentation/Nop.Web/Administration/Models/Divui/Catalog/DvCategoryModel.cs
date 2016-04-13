using Nop.Core.Domain.Divui.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Admin.Models.Catalog
{
    public partial class CategoryModel
    {
        public CategoryModel()
        {
            if (PageSize < 1)
            {
                PageSize = 5;
            }
            Locales = new List<CategoryLocalizedModel>();
            AvailableCategoryTemplates = new List<SelectListItem>();
            AvailableCategories = new List<SelectListItem>();
            AvailableCategoryTypes = new List<SelectListItem>();
            AddSpecificationAttributeModel = new AddCategorySpecificationAttributeModel();
        }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.ShortDescription")]
        [AllowHtml]
        public string ShortDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.AttractionDescription")]
        [AllowHtml]
        public string AttractionDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Guide")]
        [AllowHtml]
        public string Guide { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.IsNew")]
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets the product type identifier
        /// </summary>
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.CategoryType")]
        public int CategoryTypeId { get; set; }

        public List<SelectListItem> AvailableCategoryTypes { get; set; }

        //add specification attribute model
        public AddCategorySpecificationAttributeModel AddSpecificationAttributeModel { get; set; }

        public partial class AddCategorySpecificationAttributeModel : BaseNopModel
        {
            public AddCategorySpecificationAttributeModel()
            {
                AvailableAttributes = new List<SelectListItem>();
                AvailableOptions = new List<SelectListItem>();
            }

            [NopResourceDisplayName("Admin.Catalog.Categorys.SpecificationAttributes.Fields.SpecificationAttribute")]
            public int SpecificationAttributeId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Categorys.SpecificationAttributes.Fields.AttributeType")]
            public int AttributeTypeId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Categorys.SpecificationAttributes.Fields.SpecificationAttributeOption")]
            public int SpecificationAttributeOptionId { get; set; }

            [AllowHtml]
            [NopResourceDisplayName("Admin.Catalog.Categorys.SpecificationAttributes.Fields.CustomValue")]
            public string CustomValue { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Categorys.SpecificationAttributes.Fields.AllowFiltering")]
            public bool AllowFiltering { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Categorys.SpecificationAttributes.Fields.ShowOnCategoryPage")]
            public bool ShowOnCategoryPage { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Categorys.SpecificationAttributes.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }

            public IList<SelectListItem> AvailableAttributes { get; set; }
            public IList<SelectListItem> AvailableOptions { get; set; }
        }
    }

    public partial class CategoryLocalizedModel
    {
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.ShortDescription")]
        [AllowHtml]
        public string ShortDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.AttractionDescription")]
        [AllowHtml]
        public string AttractionDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Guide")]
        [AllowHtml]
        public string Guide { get; set; }

    }
}