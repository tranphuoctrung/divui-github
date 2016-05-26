using System;
using AutoMapper;
using Nop.Admin.Models.Blogs;
using Nop.Admin.Models.Catalog;
using Nop.Admin.Models.Cms;
using Nop.Admin.Models.Common;
using Nop.Admin.Models.Customers;
using Nop.Admin.Models.Directory;
using Nop.Admin.Models.Discounts;
using Nop.Admin.Models.ExternalAuthentication;
using Nop.Admin.Models.Forums;
using Nop.Admin.Models.Localization;
using Nop.Admin.Models.Logging;
using Nop.Admin.Models.Messages;
using Nop.Admin.Models.News;
using Nop.Admin.Models.Orders;
using Nop.Admin.Models.Payments;
using Nop.Admin.Models.Plugins;
using Nop.Admin.Models.Polls;
using Nop.Admin.Models.Settings;
using Nop.Admin.Models.Shipping;
using Nop.Admin.Models.Stores;
using Nop.Admin.Models.Tax;
using Nop.Admin.Models.Templates;
using Nop.Admin.Models.Topics;
using Nop.Admin.Models.Vendors;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Divui.Catalog;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Services.Authentication.External;
using Nop.Services.Cms;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Admin.Models.Divui.Catalog;

namespace Nop.Admin.Infrastructure
{
    public class DvAutoMapperStartupTask : IStartupTask
    {
        public int Order
        {
            get { return 0; }
        }

        public void Execute()
        {
            //DvGroupProduct
            Mapper.CreateMap<ProductOption, ProductModel.ProductOptionModel>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore())
                .ForMember(dest => dest.ProductOptionId, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.SeName, mo => mo.Ignore())
                ;
            Mapper.CreateMap<ProductModel.ProductOptionModel, ProductOption>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Products, mo => mo.Ignore())
                .ForMember(dest => dest.Product, mo => mo.Ignore())
                ;
            
            //products
            Mapper.CreateMap<Product, ProductModel>()
                .ForMember(dest => dest.ProductTypeName, mo => mo.Ignore())
                .ForMember(dest => dest.AssociatedToProductId, mo => mo.Ignore())
                .ForMember(dest => dest.AssociatedToProductName, mo => mo.Ignore())
                .ForMember(dest => dest.StockQuantityStr, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.ProductTags, mo => mo.Ignore())
                .ForMember(dest => dest.PictureThumbnailUrl, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableVendors, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableProductTemplates, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCategories, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCollections, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableAttractions, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableManufacturers, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableProductAttributes, mo => mo.Ignore())
                .ForMember(dest => dest.AddPictureModel, mo => mo.Ignore())
                .ForMember(dest => dest.ProductPictureModels, mo => mo.Ignore())
                .ForMember(dest => dest.AddSpecificationAttributeModel, mo => mo.Ignore())
                .ForMember(dest => dest.CopyProductModel, mo => mo.Ignore())
                .ForMember(dest => dest.ProductWarehouseInventoryModels, mo => mo.Ignore())
                .ForMember(dest => dest.IsLoggedInAsVendor, mo => mo.Ignore())
                .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(0, true, false)))
                .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableTaxCategories, mo => mo.Ignore())
                .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                .ForMember(dest => dest.BaseDimensionIn, mo => mo.Ignore())
                .ForMember(dest => dest.BaseWeightIn, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableDiscounts, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedDiscountIds, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableDeliveryDates, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableWarehouses, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableBasepriceUnits, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableBasepriceBaseUnits, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableProductOptions, mo => mo.Ignore())
                ;
            Mapper.CreateMap<ProductModel, Product>()
                .ForMember(dest => dest.ProductTags, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.ParentGroupedProductId, mo => mo.Ignore())
                .ForMember(dest => dest.ProductType, mo => mo.Ignore())
                .ForMember(dest => dest.Deleted, mo => mo.Ignore())
                .ForMember(dest => dest.ApprovedRatingSum, mo => mo.Ignore())
                .ForMember(dest => dest.NotApprovedRatingSum, mo => mo.Ignore())
                .ForMember(dest => dest.ApprovedTotalReviews, mo => mo.Ignore())
                .ForMember(dest => dest.NotApprovedTotalReviews, mo => mo.Ignore())
                .ForMember(dest => dest.ProductCategories, mo => mo.Ignore())
                .ForMember(dest => dest.ProductManufacturers, mo => mo.Ignore())
                .ForMember(dest => dest.ProductPictures, mo => mo.Ignore())
                .ForMember(dest => dest.ProductReviews, mo => mo.Ignore())
                .ForMember(dest => dest.ProductSpecificationAttributes, mo => mo.Ignore())
                .ForMember(dest => dest.ProductWarehouseInventory, mo => mo.Ignore())
                .ForMember(dest => dest.HasTierPrices, mo => mo.Ignore())
                .ForMember(dest => dest.HasDiscountsApplied, mo => mo.Ignore())
                .ForMember(dest => dest.BackorderMode, mo => mo.Ignore())
                .ForMember(dest => dest.DownloadActivationType, mo => mo.Ignore())
                .ForMember(dest => dest.GiftCardType, mo => mo.Ignore())
                .ForMember(dest => dest.LowStockActivity, mo => mo.Ignore())
                .ForMember(dest => dest.ManageInventoryMethod, mo => mo.Ignore())
                .ForMember(dest => dest.RecurringCyclePeriod, mo => mo.Ignore())
                .ForMember(dest => dest.RentalPricePeriod, mo => mo.Ignore())
                .ForMember(dest => dest.ProductAttributeMappings, mo => mo.Ignore())
                .ForMember(dest => dest.ProductAttributeCombinations, mo => mo.Ignore())
                .ForMember(dest => dest.TierPrices, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedDiscounts, mo => mo.Ignore());

            //Banner
            Mapper.CreateMap<Banner, BannerModel>();
            Mapper.CreateMap<BannerModel, Banner>();
        }
    }
}