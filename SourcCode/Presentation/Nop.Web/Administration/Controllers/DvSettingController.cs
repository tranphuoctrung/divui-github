using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Models.Common;
using Nop.Admin.Models.Settings;
using Nop.Admin.Models.Stores;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Security.Captcha;
using Nop.Web.Framework.Themes;

namespace Nop.Admin.Controllers
{
    public partial class SettingController
    {
        public ActionResult Catalog()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();


            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var catalogSettings = _settingService.LoadSetting<CatalogSettings>(storeScope);
            var model = catalogSettings.ToModel();
            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.AllowViewUnpublishedProductPage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowViewUnpublishedProductPage, storeScope);
                model.DisplayDiscontinuedMessageForUnpublishedProducts_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayDiscontinuedMessageForUnpublishedProducts, storeScope);
                model.ShowProductSku_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowProductSku, storeScope);
                model.ShowManufacturerPartNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowManufacturerPartNumber, storeScope);
                model.ShowGtin_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowGtin, storeScope);
                model.ShowFreeShippingNotification_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowFreeShippingNotification, storeScope);
                model.AllowProductSorting_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowProductSorting, storeScope);
                model.AllowProductViewModeChanging_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowProductViewModeChanging, storeScope);
                model.ShowProductsFromSubcategories_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowProductsFromSubcategories, storeScope);
                model.ShowCategoryProductNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowCategoryProductNumber, storeScope);
                model.ShowCategoryProductNumberIncludingSubcategories_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowCategoryProductNumberIncludingSubcategories, storeScope);
                model.CategoryBreadcrumbEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.CategoryBreadcrumbEnabled, storeScope);
                model.ShowShareButton_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowShareButton, storeScope);
                model.PageShareCode_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.PageShareCode, storeScope);
                model.ProductReviewsMustBeApproved_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductReviewsMustBeApproved, storeScope);
                model.AllowAnonymousUsersToReviewProduct_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowAnonymousUsersToReviewProduct, storeScope);
                model.NotifyStoreOwnerAboutNewProductReviews_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NotifyStoreOwnerAboutNewProductReviews, storeScope);
                model.EmailAFriendEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.EmailAFriendEnabled, storeScope);
                model.AllowAnonymousUsersToEmailAFriend_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowAnonymousUsersToEmailAFriend, storeScope);
                model.RecentlyViewedProductsNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.RecentlyViewedProductsNumber, storeScope);
                model.RecentlyViewedProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.RecentlyViewedProductsEnabled, storeScope);
                model.NewProductsNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NewProductsNumber, storeScope);
                model.NewProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NewProductsEnabled, storeScope);
                model.CompareProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.CompareProductsEnabled, storeScope);
                model.ShowBestsellersOnHomepage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowBestsellersOnHomepage, storeScope);
                model.NumberOfBestsellersOnHomepage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NumberOfBestsellersOnHomepage, storeScope);
                model.SearchPageProductsPerPage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.SearchPageProductsPerPage, storeScope);
                model.SearchPageAllowCustomersToSelectPageSize_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.SearchPageAllowCustomersToSelectPageSize, storeScope);
                model.SearchPagePageSizeOptions_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.SearchPagePageSizeOptions, storeScope);
                model.ProductSearchAutoCompleteEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductSearchAutoCompleteEnabled, storeScope);
                model.ProductSearchAutoCompleteNumberOfProducts_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductSearchAutoCompleteNumberOfProducts, storeScope);
                model.ShowProductImagesInSearchAutoComplete_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowProductImagesInSearchAutoComplete, storeScope);
                model.ProductSearchTermMinimumLength_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductSearchTermMinimumLength, storeScope);
                model.ProductsAlsoPurchasedEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductsAlsoPurchasedEnabled, storeScope);
                model.ProductsAlsoPurchasedNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductsAlsoPurchasedNumber, storeScope);
                model.NumberOfProductTags_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NumberOfProductTags, storeScope);
                model.ProductsByTagPageSize_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductsByTagPageSize, storeScope);
                model.ProductsByTagAllowCustomersToSelectPageSize_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductsByTagAllowCustomersToSelectPageSize, storeScope);
                model.ProductsByTagPageSizeOptions_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductsByTagPageSizeOptions, storeScope);
                model.IncludeShortDescriptionInCompareProducts_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.IncludeShortDescriptionInCompareProducts, storeScope);
                model.IncludeFullDescriptionInCompareProducts_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.IncludeFullDescriptionInCompareProducts, storeScope);
                model.IgnoreDiscounts_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.IgnoreDiscounts, storeScope);
                model.IgnoreFeaturedProducts_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.IgnoreFeaturedProducts, storeScope);
                model.IgnoreAcl_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.IgnoreAcl, storeScope);
                model.IgnoreStoreLimitations_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.IgnoreStoreLimitations, storeScope);
                model.CacheProductPrices_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.CacheProductPrices, storeScope);
                model.ManufacturersBlockItemsToDisplay_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ManufacturersBlockItemsToDisplay, storeScope);
                model.DisplayTaxShippingInfoFooter_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayTaxShippingInfoFooter, storeScope);
                model.DisplayTaxShippingInfoProductDetailsPage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayTaxShippingInfoProductDetailsPage, storeScope);
                model.DisplayTaxShippingInfoProductBoxes_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayTaxShippingInfoProductBoxes, storeScope);
                model.DisplayTaxShippingInfoShoppingCart_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayTaxShippingInfoShoppingCart, storeScope);
                model.DisplayTaxShippingInfoWishlist_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayTaxShippingInfoWishlist, storeScope);
                model.DisplayTaxShippingInfoOrderDetailsPage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayTaxShippingInfoOrderDetailsPage, storeScope);
                model.DefaultAttractionProductNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DefaultAttractionProductNumber, storeScope);
                model.DefaultCategoryProductNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DefaultCategoryProductNumber, storeScope);
                model.DefaultCollectionProductNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DefaultCollectionProductNumber, storeScope);
                model.DefaultDestinationProductNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DefaultDestinationProductNumber, storeScope);
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult Catalog(CatalogSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();


            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var catalogSettings = _settingService.LoadSetting<CatalogSettings>(storeScope);
            catalogSettings = model.ToEntity(catalogSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.AllowViewUnpublishedProductPage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.AllowViewUnpublishedProductPage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.AllowViewUnpublishedProductPage, storeScope);

            if (model.DisplayDiscontinuedMessageForUnpublishedProducts_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.DisplayDiscontinuedMessageForUnpublishedProducts, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.DisplayDiscontinuedMessageForUnpublishedProducts, storeScope);

            if (model.ShowProductSku_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ShowProductSku, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ShowProductSku, storeScope);

            if (model.ShowManufacturerPartNumber_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ShowManufacturerPartNumber, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ShowManufacturerPartNumber, storeScope);

            if (model.ShowGtin_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ShowGtin, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ShowGtin, storeScope);

            if (model.ShowFreeShippingNotification_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ShowFreeShippingNotification, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ShowFreeShippingNotification, storeScope);

            if (model.AllowProductSorting_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.AllowProductSorting, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.AllowProductSorting, storeScope);

            if (model.AllowProductViewModeChanging_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.AllowProductViewModeChanging, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.AllowProductViewModeChanging, storeScope);

            if (model.ShowProductsFromSubcategories_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ShowProductsFromSubcategories, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ShowProductsFromSubcategories, storeScope);

            if (model.ShowCategoryProductNumber_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ShowCategoryProductNumber, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ShowCategoryProductNumber, storeScope);

            if (model.ShowCategoryProductNumberIncludingSubcategories_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ShowCategoryProductNumberIncludingSubcategories, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ShowCategoryProductNumberIncludingSubcategories, storeScope);

            if (model.CategoryBreadcrumbEnabled_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.CategoryBreadcrumbEnabled, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.CategoryBreadcrumbEnabled, storeScope);

            if (model.ShowShareButton_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ShowShareButton, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ShowShareButton, storeScope);

            if (model.PageShareCode_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.PageShareCode, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.PageShareCode, storeScope);

            if (model.ProductReviewsMustBeApproved_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ProductReviewsMustBeApproved, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ProductReviewsMustBeApproved, storeScope);

            if (model.AllowAnonymousUsersToReviewProduct_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.AllowAnonymousUsersToReviewProduct, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.AllowAnonymousUsersToReviewProduct, storeScope);

            if (model.NotifyStoreOwnerAboutNewProductReviews_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.NotifyStoreOwnerAboutNewProductReviews, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.NotifyStoreOwnerAboutNewProductReviews, storeScope);

            if (model.EmailAFriendEnabled_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.EmailAFriendEnabled, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.EmailAFriendEnabled, storeScope);

            if (model.AllowAnonymousUsersToEmailAFriend_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.AllowAnonymousUsersToEmailAFriend, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.AllowAnonymousUsersToEmailAFriend, storeScope);

            if (model.RecentlyViewedProductsNumber_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.RecentlyViewedProductsNumber, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.RecentlyViewedProductsNumber, storeScope);

            if (model.RecentlyViewedProductsEnabled_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.RecentlyViewedProductsEnabled, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.RecentlyViewedProductsEnabled, storeScope);

            if (model.NewProductsNumber_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.NewProductsNumber, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.NewProductsNumber, storeScope);

            if (model.NewProductsEnabled_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.NewProductsEnabled, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.NewProductsEnabled, storeScope);

            if (model.CompareProductsEnabled_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.CompareProductsEnabled, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.CompareProductsEnabled, storeScope);

            if (model.ShowBestsellersOnHomepage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ShowBestsellersOnHomepage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ShowBestsellersOnHomepage, storeScope);

            if (model.NumberOfBestsellersOnHomepage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.NumberOfBestsellersOnHomepage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.NumberOfBestsellersOnHomepage, storeScope);

            if (model.SearchPageProductsPerPage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.SearchPageProductsPerPage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.SearchPageProductsPerPage, storeScope);

            if (model.SearchPageAllowCustomersToSelectPageSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.SearchPageAllowCustomersToSelectPageSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.SearchPageAllowCustomersToSelectPageSize, storeScope);

            if (model.SearchPagePageSizeOptions_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.SearchPagePageSizeOptions, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.SearchPagePageSizeOptions, storeScope);

            if (model.ProductSearchAutoCompleteEnabled_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ProductSearchAutoCompleteEnabled, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ProductSearchAutoCompleteEnabled, storeScope);

            if (model.ProductSearchAutoCompleteNumberOfProducts_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ProductSearchAutoCompleteNumberOfProducts, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ProductSearchAutoCompleteNumberOfProducts, storeScope);

            if (model.ShowProductImagesInSearchAutoComplete_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ShowProductImagesInSearchAutoComplete, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ShowProductImagesInSearchAutoComplete, storeScope);

            if (model.ProductSearchTermMinimumLength_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ProductSearchTermMinimumLength, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ProductSearchTermMinimumLength, storeScope);

            if (model.ProductsAlsoPurchasedEnabled_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ProductsAlsoPurchasedEnabled, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ProductsAlsoPurchasedEnabled, storeScope);

            if (model.ProductsAlsoPurchasedNumber_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ProductsAlsoPurchasedNumber, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ProductsAlsoPurchasedNumber, storeScope);

            if (model.NumberOfProductTags_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.NumberOfProductTags, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.NumberOfProductTags, storeScope);

            if (model.ProductsByTagPageSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ProductsByTagPageSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ProductsByTagPageSize, storeScope);

            if (model.ProductsByTagAllowCustomersToSelectPageSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ProductsByTagAllowCustomersToSelectPageSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ProductsByTagAllowCustomersToSelectPageSize, storeScope);

            if (model.ProductsByTagPageSizeOptions_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ProductsByTagPageSizeOptions, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ProductsByTagPageSizeOptions, storeScope);

            if (model.IncludeShortDescriptionInCompareProducts_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.IncludeShortDescriptionInCompareProducts, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.IncludeShortDescriptionInCompareProducts, storeScope);

            if (model.IncludeFullDescriptionInCompareProducts_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.IncludeFullDescriptionInCompareProducts, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.IncludeFullDescriptionInCompareProducts, storeScope);

            if (model.IgnoreDiscounts_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.IgnoreDiscounts, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.IgnoreDiscounts, storeScope);

            if (model.IgnoreFeaturedProducts_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.IgnoreFeaturedProducts, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.IgnoreFeaturedProducts, storeScope);

            if (model.IgnoreAcl_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.IgnoreAcl, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.IgnoreAcl, storeScope);

            if (model.IgnoreStoreLimitations_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.IgnoreStoreLimitations, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.IgnoreStoreLimitations, storeScope);

            if (model.CacheProductPrices_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.CacheProductPrices, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.CacheProductPrices, storeScope);

            if (model.ManufacturersBlockItemsToDisplay_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.ManufacturersBlockItemsToDisplay, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.ManufacturersBlockItemsToDisplay, storeScope);

            if (model.DisplayTaxShippingInfoFooter_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.DisplayTaxShippingInfoFooter, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.DisplayTaxShippingInfoFooter, storeScope);

            if (model.DisplayTaxShippingInfoProductDetailsPage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.DisplayTaxShippingInfoProductDetailsPage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.DisplayTaxShippingInfoProductDetailsPage, storeScope);

            if (model.DisplayTaxShippingInfoProductBoxes_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.DisplayTaxShippingInfoProductBoxes, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.DisplayTaxShippingInfoProductBoxes, storeScope);

            if (model.DisplayTaxShippingInfoShoppingCart_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.DisplayTaxShippingInfoShoppingCart, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.DisplayTaxShippingInfoShoppingCart, storeScope);

            if (model.DisplayTaxShippingInfoWishlist_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.DisplayTaxShippingInfoWishlist, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.DisplayTaxShippingInfoWishlist, storeScope);

            if (model.DisplayTaxShippingInfoOrderDetailsPage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.DisplayTaxShippingInfoOrderDetailsPage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.DisplayTaxShippingInfoOrderDetailsPage, storeScope);

            if (model.DefaultAttractionProductNumber_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.DefaultAttractionProductNumber, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.DefaultAttractionProductNumber, storeScope);

            if (model.DefaultCategoryProductNumber_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.DefaultCategoryProductNumber, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.DefaultCategoryProductNumber, storeScope);

            if (model.DefaultCollectionProductNumber_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.DefaultCollectionProductNumber, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.DefaultCollectionProductNumber, storeScope);

            if (model.DefaultDestinationProductNumber_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(catalogSettings, x => x.DefaultDestinationProductNumber, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(catalogSettings, x => x.DefaultDestinationProductNumber, storeScope);


            //now clear settings cache
            _settingService.ClearCache();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));

            //selected tab
            SaveSelectedTabIndex();

            return RedirectToAction("Catalog");
        }
    }
}