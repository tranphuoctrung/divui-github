using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Models.Customers;
using Nop.Admin.Models.Discounts;
using Nop.Admin.Models.Stores;
using Nop.Admin.Validators.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Catalog
{
    public partial class ProductModel
    {

        public ProductModel()
        {
            Locales = new List<ProductLocalizedModel>();
            ProductPictureModels = new List<ProductPictureModel>();
            CopyProductModel = new CopyProductModel();
            AvailableBasepriceUnits = new List<SelectListItem>();
            AvailableBasepriceBaseUnits = new List<SelectListItem>();
            AvailableProductTemplates = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableTaxCategories = new List<SelectListItem>();
            AvailableDeliveryDates = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            AvailableCategories = new List<SelectListItem>();
            AvailableAttractions = new List<SelectListItem>();
            AvailableCollections = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableProductAttributes = new List<SelectListItem>();
            AddPictureModel = new ProductPictureModel();
            AddSpecificationAttributeModel = new AddProductSpecificationAttributeModel();
            ProductWarehouseInventoryModels = new List<ProductWarehouseInventoryModel>();

            AvailableProductOptions = new List<SelectListItem>();
            AvailableDestinations = new List<SelectListItem>();
        }
        //collection
        public IList<SelectListItem> AvailableCollections { get; set; }


        //attraction
        public IList<SelectListItem> AvailableAttractions { get; set; }


        //destination
        public IList<SelectListItem> AvailableDestinations { get; set; }
        public int ParentGroupedProductId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Overview")]
        [AllowHtml]
        public string Overview { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.HightLight")]
        [AllowHtml]
        public string HightLight { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Condition")]
        [AllowHtml]
        public string Condition { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.GuideToUse")]
        [AllowHtml]
        public string GuideToUse { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Tip")]
        [AllowHtml]
        public string Tip { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Type")]
        public int Type { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.AgeRangeCondition")]
        public string AgeRangeCondition { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.AgeRange")]
        public int AgeRange { get; set; }

        //categories
        public IList<SelectListItem> AvailableProductOptions { get; set; }
        public partial class ProductOptionLocalizedModel : ILocalizedModelLocal
        {
            public int LanguageId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.ProductOptions.Fields.Name")]
            [AllowHtml]
            public string Name { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.ProductOptions.Fields.Description")]
            [AllowHtml]
            public string Description { get; set; }

           
            [NopResourceDisplayName("Admin.Catalog.Products.ProductOptions.Fields.SeName")]
            [AllowHtml]
            public string SeName { get; set; }
        }

        public partial class ProductOptionModel : BaseNopEntityModel, ILocalizedModel<ProductOptionLocalizedModel>
        {
            public ProductOptionModel()
            {
                AvailableProducts = new List<SelectListItem>();
                Locales = new List<ProductOptionLocalizedModel>();
                Published = true;
            }

            [NopResourceDisplayName("Admin.Catalog.Products.ProductOptions.Fields.Category")]
            public string Name { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.ProductOptions.Fields.Description")]
            [AllowHtml]
            public string Description { get; set; }
            
            public int ProductId { get; set; }

            public int ProductOptionId { get; set; }

            public bool Published { get; set; }

           
            public bool Deleted { get; set; }


            [NopResourceDisplayName("Admin.Catalog.Products.ProductOptions.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.ProductOptions.Fields.SeName")]
            [AllowHtml]
            public string SeName { get; set; }

            public IList<ProductOptionLocalizedModel> Locales { get; set; }

            public List<SelectListItem> AvailableProducts { get; set; }

        }

        public partial class ProductCollectionModel : BaseNopEntityModel
        {
            [NopResourceDisplayName("Admin.Catalog.Products.Collections.Fields.Collection")]
            public string Collection { get; set; }

            public int ProductId { get; set; }

            public int CollectionId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Collections.Fields.IsFeaturedProduct")]
            public bool IsFeaturedProduct { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Collections.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }

        public partial class ProductAttractionModel : BaseNopEntityModel
        {
            [NopResourceDisplayName("Admin.Catalog.Products.Attractions.Fields.Attraction")]
            public string Attraction { get; set; }

            public int ProductId { get; set; }

            public int AttractionId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Attractions.Fields.IsFeaturedProduct")]
            public bool IsFeaturedProduct { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Attractions.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }

        public partial class ProductCategoryModel 
        {
            [NopResourceDisplayName("Admin.Catalog.Products.Categories.Fields.CategoryType")]
            public int CategoryTypeId { get; set; }

            
        }

        
    }
    public partial class ProductLocalizedModel
    {

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Overview")]
        [AllowHtml]
        public string Overview { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.HightLight")]
        [AllowHtml]
        public string HightLight { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Condition")]
        [AllowHtml]
        public string Condition { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.GuideToUse")]
        [AllowHtml]
        public string GuideToUse { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Tip")]
        [AllowHtml]
        public string Tip { get; set; }


        


        

    }
}