using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Extensions;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Security.Captcha;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Services.Divui.Catalog;
using Nop.Core.Domain.Divui.Catalog;

namespace Nop.Web.Controllers
{
    public partial class ProductController 
    {
        #region Fields
        private readonly IProductOptionService _productOptionService;
        private readonly IPriceSetupService _priceSetupService;
        private readonly IAvailabilitySetupService _availabilitySetupService;
        #endregion

        #region Constructors

        public ProductController(ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductService productService,
            IVendorService vendorService,
            IProductTemplateService productTemplateService,
            IProductAttributeService productAttributeService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IMeasureService measureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IWebHelper webHelper,
            ISpecificationAttributeService specificationAttributeService,
            IDateTimeHelper dateTimeHelper,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            ICompareProductsService compareProductsService,
            IWorkflowMessageService workflowMessageService,
            IProductTagService productTagService,
            IOrderReportService orderReportService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            IDownloadService downloadService,
            ICustomerActivityService customerActivityService,
            IProductAttributeParser productAttributeParser,
            IShippingService shippingService,
            IEventPublisher eventPublisher,
            MediaSettings mediaSettings,
            CatalogSettings catalogSettings,
            VendorSettings vendorSettings,
            ShoppingCartSettings shoppingCartSettings,
            LocalizationSettings localizationSettings,
            CustomerSettings customerSettings,
            CaptchaSettings captchaSettings,
            SeoSettings seoSettings,
            ICacheManager cacheManager,
            IProductOptionService productOptionService,
            IPriceSetupService priceSetupService,
            IAvailabilitySetupService availabilitySetupService)
        {
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._vendorService = vendorService;
            this._productTemplateService = productTemplateService;
            this._productAttributeService = productAttributeService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._measureService = measureService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._webHelper = webHelper;
            this._specificationAttributeService = specificationAttributeService;
            this._dateTimeHelper = dateTimeHelper;
            this._recentlyViewedProductsService = recentlyViewedProductsService;
            this._compareProductsService = compareProductsService;
            this._workflowMessageService = workflowMessageService;
            this._productTagService = productTagService;
            this._orderReportService = orderReportService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;
            this._downloadService = downloadService;
            this._customerActivityService = customerActivityService;
            this._productAttributeParser = productAttributeParser;
            this._shippingService = shippingService;
            this._eventPublisher = eventPublisher;
            this._mediaSettings = mediaSettings;
            this._catalogSettings = catalogSettings;
            this._vendorSettings = vendorSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._localizationSettings = localizationSettings;
            this._customerSettings = customerSettings;
            this._captchaSettings = captchaSettings;
            this._seoSettings = seoSettings;
            this._cacheManager = cacheManager;
            this._productOptionService = productOptionService;
            this._priceSetupService = priceSetupService;
            this._availabilitySetupService = availabilitySetupService;

        }

        #endregion

        [NonAction]
        protected virtual List<int> GetChildCategoryIds(int parentCategoryId)
        {
            string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_CHILD_IDENTIFIERS_MODEL_KEY,
                parentCategoryId,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(cacheKey, () =>
            {
                var categoriesIds = new List<int>();
                var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId);
                foreach (var category in categories)
                {
                    categoriesIds.Add(category.Id);
                    categoriesIds.AddRange(GetChildCategoryIds(category.Id));
                }
                return categoriesIds;
            });
        }

        [NonAction]
        protected virtual ProductDetailsModel PrepareProductDetailsPageModel(Product product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            #region Standard properties

            var model = new ProductDetailsModel
            {
                Id = product.Id,
                Name = product.GetLocalized(x => x.Name),
                ShortDescription = product.GetLocalized(x => x.ShortDescription),
                FullDescription = product.GetLocalized(x => x.FullDescription),
                Overview = product.GetLocalized(x => x.Overview),
                HightLight = product.GetLocalized(x => x.HightLight),
                GuideToUse = product.GetLocalized(x => x.GuideToUse),
                Tip = product.GetLocalized(x => x.Tip),
                Condition = product.GetLocalized(x => x.Condition),
                MetaKeywords = product.GetLocalized(x => x.MetaKeywords),
                MetaDescription = product.GetLocalized(x => x.MetaDescription),
                MetaTitle = product.GetLocalized(x => x.MetaTitle),
                SeName = product.GetSeName(),
                ShowSku = _catalogSettings.ShowProductSku,
                Sku = product.Sku,
                ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber,
                FreeShippingNotificationEnabled = _catalogSettings.ShowFreeShippingNotification,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                ShowGtin = _catalogSettings.ShowGtin,
                Gtin = product.Gtin,
                StockAvailability = product.FormatStockMessage("", _localizationService, _productAttributeParser),
                HasSampleDownload = product.IsDownload && product.HasSampleDownload,
                DisplayDiscontinuedMessage = !product.Published && _catalogSettings.DisplayDiscontinuedMessageForUnpublishedProducts
            };

            //automatically generate product description?
            if (_seoSettings.GenerateProductMetaDescription && String.IsNullOrEmpty(model.MetaDescription))
            {
                //based on short description
                model.MetaDescription = model.ShortDescription;
            }

            //shipping info
            model.IsShipEnabled = product.IsShipEnabled;
            if (product.IsShipEnabled)
            {
                model.IsFreeShipping = product.IsFreeShipping;
                //delivery date
                var deliveryDate = _shippingService.GetDeliveryDateById(product.DeliveryDateId);
                if (deliveryDate != null)
                {
                    model.DeliveryDate = deliveryDate.GetLocalized(dd => dd.Name);
                }
            }

            //email a friend
            model.EmailAFriendEnabled = _catalogSettings.EmailAFriendEnabled;
            //compare products
            model.CompareProductsEnabled = _catalogSettings.CompareProductsEnabled;

            #endregion

            #region Vendor details

            //vendor
            if (_vendorSettings.ShowVendorOnProductDetailsPage)
            {
                var vendor = _vendorService.GetVendorById(product.VendorId);
                if (vendor != null && !vendor.Deleted && vendor.Active)
                {
                    model.ShowVendor = true;

                    model.VendorModel = new VendorBriefInfoModel
                    {
                        Id = vendor.Id,
                        Name = vendor.GetLocalized(x => x.Name),
                        SeName = vendor.GetSeName(),
                    };
                }
            }

            #endregion

            #region Page sharing

            if (_catalogSettings.ShowShareButton && !String.IsNullOrEmpty(_catalogSettings.PageShareCode))
            {
                var shareCode = _catalogSettings.PageShareCode;
                if (_webHelper.IsCurrentConnectionSecured())
                {
                    //need to change the addthis link to be https linked when the page is, so that the page doesnt ask about mixed mode when viewed in https...
                    shareCode = shareCode.Replace("http://", "https://");
                }
                model.PageShareCode = shareCode;
            }

            #endregion

            #region Back in stock subscriptions

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.GetTotalStockQuantity() <= 0)
            {
                //out of stock
                model.DisplayBackInStockSubscription = true;
            }

            #endregion

            #region Breadcrumb

            //do not prepare this model for the associated products. anyway it's not used
            if (_catalogSettings.CategoryBreadcrumbEnabled && !isAssociatedProduct)
            {
                var breadcrumbCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_BREADCRUMB_MODEL_KEY,
                    product.Id,
                    _workContext.WorkingLanguage.Id,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                    _storeContext.CurrentStore.Id);
                model.Breadcrumb = _cacheManager.Get(breadcrumbCacheKey, () =>
                {
                    var breadcrumbModel = new ProductDetailsModel.ProductBreadcrumbModel
                    {
                        Enabled = _catalogSettings.CategoryBreadcrumbEnabled,
                        ProductId = product.Id,
                        ProductName = product.GetLocalized(x => x.Name),
                        ProductSeName = product.GetSeName()
                    };
                    var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
                    if (productCategories.Count > 0)
                    {
                        var category = productCategories[0].Category;
                        if (category != null)
                        {
                            foreach (var catBr in category.GetCategoryBreadCrumb(_categoryService, _aclService, _storeMappingService))
                            {
                                breadcrumbModel.CategoryBreadcrumb.Add(new CategorySimpleModel
                                {
                                    Id = catBr.Id,
                                    Name = catBr.GetLocalized(x => x.Name),
                                    SeName = catBr.GetSeName(),
                                    IncludeInTopMenu = catBr.IncludeInTopMenu
                                });
                            }
                        }
                    }
                    return breadcrumbModel;
                });
            }

            #endregion

            #region Product tags

            //do not prepare this model for the associated products. anyway it's not used
            if (!isAssociatedProduct)
            {
                var productTagsCacheKey = string.Format(ModelCacheEventConsumer.PRODUCTTAG_BY_PRODUCT_MODEL_KEY, product.Id, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
                model.ProductTags = _cacheManager.Get(productTagsCacheKey, () =>
                    product.ProductTags
                    //filter by store
                    .Where(x => _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id) > 0)
                    .Select(x => new ProductTagModel
                    {
                        Id = x.Id,
                        Name = x.GetLocalized(y => y.Name),
                        SeName = x.GetSeName(),
                        ProductCount = _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id)
                    })
                    .ToList());
            }

            #endregion

            #region Templates

            var templateCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_TEMPLATE_MODEL_KEY, product.ProductTemplateId);
            model.ProductTemplateViewPath = _cacheManager.Get(templateCacheKey, () =>
            {
                var template = _productTemplateService.GetProductTemplateById(product.ProductTemplateId);
                if (template == null)
                    template = _productTemplateService.GetAllProductTemplates().FirstOrDefault();
                if (template == null)
                    throw new Exception("No default template could be loaded");
                return template.ViewPath;
            });

            #endregion

            #region Pictures

            model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;
            //default picture
            var defaultPictureSize = isAssociatedProduct ?
                _mediaSettings.AssociatedProductPictureSize :
                _mediaSettings.ProductDetailsPictureSize;
            //prepare picture models
            var productPicturesCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DETAILS_PICTURES_MODEL_KEY, product.Id, defaultPictureSize, isAssociatedProduct, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
            var cachedPictures = _cacheManager.Get(productPicturesCacheKey, () =>
            {
                var pictures = _pictureService.GetPicturesByProductId(product.Id);
                var defaultPicture = pictures.FirstOrDefault();
                var defaultPictureModel = new PictureModel
                {
                    ImageUrl = _pictureService.GetPictureUrl(defaultPicture, defaultPictureSize, !isAssociatedProduct),
                    FullSizeImageUrl = _pictureService.GetPictureUrl(defaultPicture, 0, !isAssociatedProduct),
                    Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), model.Name),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), model.Name),
                };
                //"title" attribute
                defaultPictureModel.Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                    defaultPicture.TitleAttribute :
                    string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), model.Name);
                //"alt" attribute
                defaultPictureModel.AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                    defaultPicture.AltAttribute :
                    string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), model.Name);

                //all pictures
                var pictureModels = new List<PictureModel>();
                foreach (var picture in pictures)
                {
                    var pictureModel = new PictureModel
                    {
                        ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), model.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), model.Name),
                    };
                    //"title" attribute
                    pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                        picture.TitleAttribute :
                        string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), model.Name);
                    //"alt" attribute
                    pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                        picture.AltAttribute :
                        string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), model.Name);

                    pictureModels.Add(pictureModel);
                }

                return new { DefaultPictureModel = defaultPictureModel, PictureModels = pictureModels };
            });
            model.DefaultPictureModel = cachedPictures.DefaultPictureModel;
            model.PictureModels = cachedPictures.PictureModels;

            #endregion

            #region Product price

            model.ProductPrice.ProductId = product.Id;
            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {
                model.ProductPrice.HidePrices = false;
                if (product.CustomerEntersPrice)
                {
                    model.ProductPrice.CustomerEntersPrice = true;
                }
                else
                {
                    model.ProductPrice = new ProductDetailsModel.ProductPriceModel();
                    model.ProductPrice.PriceValue = decimal.MaxValue;

                }
            }
            else
            {
                model.ProductPrice.HidePrices = true;
                model.ProductPrice.OldPrice = null;
                model.ProductPrice.Price = null;
            }
            #endregion

            #region productoption

            var options = _productOptionService.GetAllProductOptions(productId: product.Id);
            model.ProductOptions = options.Select(op =>
            {
                var m = new ProductOptionModel()
                {
                    Name = op.GetLocalized(o => o.Name),
                    Description = op.GetLocalized(o => o.Description)
                };

                foreach (var p in op.Products)
                {
                    var productSimple = new ProductSimpleModel() {
                        Name = p.GetLocalized(c => c.Name),
                        Overview = p.GetLocalized(c => c.Overview),
                        ProductId = p.Id,
                        AgeRange = p.AgeRange,
                        AgeRangeCondition = p.AgeRangeCondition
                    };

                    productSimple.AgeRangeName = p.AgeRangeType.GetLocalizedEnum(_localizationService, _workContext);

                    productSimple.ProductPrice.HidePrices = false;
                    if (p.CustomerEntersPrice)
                    {
                        productSimple.ProductPrice.CustomerEntersPrice = true;
                    }
                    else
                    {
                        if (p.CallForPrice)
                        {
                            productSimple.ProductPrice.CallForPrice = true;
                        }
                        else
                        {
                            decimal taxRate;
                            decimal oldPriceBase = _taxService.GetProductPrice(p, p.OldPrice, out taxRate);
                            decimal finalPriceWithoutDiscountBase = _taxService.GetProductPrice(p, _priceCalculationService.GetFinalPrice(p, _workContext.CurrentCustomer, includeDiscounts: false), out taxRate);
                            decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(p, _priceCalculationService.GetFinalPrice(p, _workContext.CurrentCustomer, includeDiscounts: true), out taxRate);

                            decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                            decimal finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);
                            
                            // get setup prices
                            var setupPrices = _priceSetupService.GetAllPriceSetups(productId: p.Id,
                                customerRoleIds: _workContext.CurrentCustomer.CustomerRoles.Select(r => r.Id).ToList(), toDate: DateTime.Now);

                            foreach (var setupprice in setupPrices)
                            {
                                if (setupprice.Quantity == 1)
                                {
                                    finalPriceWithoutDiscount = setupprice.Price;
                                }
                            }

                            decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);


                            if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                                productSimple.ProductPrice.OldPrice = _priceFormatter.FormatPrice(oldPrice);

                            productSimple.ProductPrice.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);

                            if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                                productSimple.ProductPrice.PriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount);

                            productSimple.ProductPrice.PriceValue = finalPriceWithDiscount;

                            

                            //property for German market
                            //we display tax/shipping info only with "shipping enabled" for this product
                            //we also ensure this it's not free shipping
                            productSimple.ProductPrice.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductDetailsPage
                                && p.IsShipEnabled &&
                                !p.IsFreeShipping;

                            //PAngV baseprice (used in Germany)
                            productSimple.ProductPrice.BasePricePAngV = p.FormatBasePrice(finalPriceWithDiscountBase,
                                _localizationService, _measureService, _currencyService, _workContext, _priceFormatter);

                            //currency code
                            productSimple.ProductPrice.CurrencyCode = _workContext.WorkingCurrency.CurrencyCode;

                            if(p.AgeRangeType == DvAgeRangeType.Adult)
                            {
                                if (finalPriceWithDiscount < model.ProductPrice.PriceValue)
                                {
                                    model.ProductPrice = productSimple.ProductPrice;
                                    model.ProductPrice.SavePercent = oldPrice * 100 / finalPriceWithoutDiscount;
                                }
                                    
                            }

                            /*productSimple.AvaliableQuantities.Add(new SelectListItem()
                            {
                                Text = _localizationService.GetResource("Products.SelectQuantity"),
                                Value = "0"
                            });

                            for (int i = 1; i < 31; i++)
                            {
                                decimal priceWithDiscountBase = _taxService.GetProductPrice(p, _priceCalculationService.GetFinalPrice(p, _workContext.CurrentCustomer, includeDiscounts: true, quantity: i), out taxRate);
                                productSimple.AvaliableQuantities.Add(new SelectListItem() {
                                    Text = i + "(x) " + _priceFormatter.FormatPrice(priceWithDiscountBase),
                                    Value = i.ToString()
                                });

                            }*/
                        }
                    }

                }
                return m;

            }).ToList();

            #endregion

           

            #region 'Add to cart' model

            model.AddToCart.ProductId = product.Id;
            model.AddToCart.UpdatedShoppingCartItemId = updatecartitem != null ? updatecartitem.Id : 0;

            //quantity
            model.AddToCart.EnteredQuantity = updatecartitem != null ? updatecartitem.Quantity : product.OrderMinimumQuantity;
            //allowed quantities
            var allowedQuantities = product.ParseAllowedQuantities();
            foreach (var qty in allowedQuantities)
            {
                model.AddToCart.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = updatecartitem != null && updatecartitem.Quantity == qty
                });
            }
            //minimum quantity notification
            if (product.OrderMinimumQuantity > 1)
            {
                model.AddToCart.MinimumQuantityNotification = string.Format(_localizationService.GetResource("Products.MinimumQuantityNotification"), product.OrderMinimumQuantity);
            }

            //'add to cart', 'add to wishlist' buttons
            model.AddToCart.DisableBuyButton = product.DisableBuyButton || !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);
            model.AddToCart.DisableWishlistButton = product.DisableWishlistButton || !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist);
            if (!_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {
                model.AddToCart.DisableBuyButton = true;
                model.AddToCart.DisableWishlistButton = true;
            }
            //pre-order
            if (product.AvailableForPreOrder)
            {
                model.AddToCart.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                    product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
                model.AddToCart.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
            }
            //rental
            model.AddToCart.IsRental = product.IsRental;

            //customer entered price
            model.AddToCart.CustomerEntersPrice = product.CustomerEntersPrice;
            if (model.AddToCart.CustomerEntersPrice)
            {
                decimal minimumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.MinimumCustomerEnteredPrice, _workContext.WorkingCurrency);
                decimal maximumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.MaximumCustomerEnteredPrice, _workContext.WorkingCurrency);

                model.AddToCart.CustomerEnteredPrice = updatecartitem != null ? updatecartitem.CustomerEnteredPrice : minimumCustomerEnteredPrice;
                model.AddToCart.CustomerEnteredPriceRange = string.Format(_localizationService.GetResource("Products.EnterProductPrice.Range"),
                    _priceFormatter.FormatPrice(minimumCustomerEnteredPrice, false, false),
                    _priceFormatter.FormatPrice(maximumCustomerEnteredPrice, false, false));
            }

            #endregion

            #region Gift card

            model.GiftCard.IsGiftCard = product.IsGiftCard;
            if (model.GiftCard.IsGiftCard)
            {
                model.GiftCard.GiftCardType = product.GiftCardType;

                if (updatecartitem == null)
                {
                    model.GiftCard.SenderName = _workContext.CurrentCustomer.GetFullName();
                    model.GiftCard.SenderEmail = _workContext.CurrentCustomer.Email;
                }
                else
                {
                    string giftCardRecipientName, giftCardRecipientEmail, giftCardSenderName, giftCardSenderEmail, giftCardMessage;
                    _productAttributeParser.GetGiftCardAttribute(updatecartitem.AttributesXml,
                        out giftCardRecipientName, out giftCardRecipientEmail,
                        out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                    model.GiftCard.RecipientName = giftCardRecipientName;
                    model.GiftCard.RecipientEmail = giftCardRecipientEmail;
                    model.GiftCard.SenderName = giftCardSenderName;
                    model.GiftCard.SenderEmail = giftCardSenderEmail;
                    model.GiftCard.Message = giftCardMessage;
                }
            }

            #endregion

            #region Product attributes

            //performance optimization
            //We cache a value indicating whether a product has attributes
            IList<ProductAttributeMapping> productAttributeMapping = null;
            string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_HAS_PRODUCT_ATTRIBUTES_KEY, product.Id);
            var hasProductAttributesCache = _cacheManager.Get<bool?>(cacheKey);
            if (!hasProductAttributesCache.HasValue)
            {
                //no value in the cache yet
                //let's load attributes and cache the result (true/false)
                productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                hasProductAttributesCache = productAttributeMapping.Count > 0;
                _cacheManager.Set(cacheKey, hasProductAttributesCache, 60);
            }
            if (hasProductAttributesCache.Value && productAttributeMapping == null)
            {
                //cache indicates that the product has attributes
                //let's load them
                productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            }
            if (productAttributeMapping == null)
            {
                productAttributeMapping = new List<ProductAttributeMapping>();
            }
            foreach (var attribute in productAttributeMapping)
            {
                var attributeModel = new ProductDetailsModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductId = product.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = attribute.ProductAttribute.GetLocalized(x => x.Name),
                    Description = attribute.ProductAttribute.GetLocalized(x => x.Description),
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = updatecartitem != null ? null : attribute.DefaultValue,
                    HasCondition = !String.IsNullOrEmpty(attribute.ConditionAttributeXml)
                };
                if (!String.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new ProductDetailsModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(valueModel);

                        //display price if allowed
                        if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                        {
                            decimal taxRate;
                            decimal attributeValuePriceAdjustment = _priceCalculationService.GetProductAttributeValuePriceAdjustment(attributeValue);
                            decimal priceAdjustmentBase = _taxService.GetProductPrice(product, attributeValuePriceAdjustment, out taxRate);
                            decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                            if (priceAdjustmentBase > decimal.Zero)
                                valueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment, false, false);
                            else if (priceAdjustmentBase < decimal.Zero)
                                valueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment, false, false);

                            valueModel.PriceAdjustmentValue = priceAdjustment;
                        }

                        //picture
                        if (attributeValue.PictureId > 0)
                        {
                            var productAttributePictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCTATTRIBUTE_PICTURE_MODEL_KEY,
                                attributeValue.PictureId,
                                _webHelper.IsCurrentConnectionSecured(),
                                _storeContext.CurrentStore.Id);
                            valueModel.PictureModel = _cacheManager.Get(productAttributePictureCacheKey, () =>
                            {
                                var valuePicture = _pictureService.GetPictureById(attributeValue.PictureId);
                                if (valuePicture != null)
                                {
                                    return new PictureModel
                                    {
                                        FullSizeImageUrl = _pictureService.GetPictureUrl(valuePicture),
                                        ImageUrl = _pictureService.GetPictureUrl(valuePicture, defaultPictureSize)
                                    };
                                }
                                return new PictureModel();
                            });
                        }
                    }
                }

                //set already selected attributes (if we're going to update the existing shopping cart item)
                if (updatecartitem != null)
                {
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                        case AttributeControlType.ColorSquares:
                            {
                                if (!String.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    //clear default selection
                                    foreach (var item in attributeModel.Values)
                                        item.IsPreSelected = false;

                                    //select new values
                                    var selectedValues = _productAttributeParser.ParseProductAttributeValues(updatecartitem.AttributesXml);
                                    foreach (var attributeValue in selectedValues)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                                item.IsPreSelected = true;
                                }
                            }
                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                            {
                                //do nothing
                                //values are already pre-set
                            }
                            break;
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                            {
                                if (!String.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    var enteredText = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                    if (enteredText.Count > 0)
                                        attributeModel.DefaultValue = enteredText[0];
                                }
                            }
                            break;
                        case AttributeControlType.Datepicker:
                            {
                                //keep in mind my that the code below works only in the current culture
                                var selectedDateStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                if (selectedDateStr.Count > 0)
                                {
                                    DateTime selectedDate;
                                    if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture,
                                                           DateTimeStyles.None, out selectedDate))
                                    {
                                        //successfully parsed
                                        attributeModel.SelectedDay = selectedDate.Day;
                                        attributeModel.SelectedMonth = selectedDate.Month;
                                        attributeModel.SelectedYear = selectedDate.Year;
                                    }
                                }

                            }
                            break;
                        case AttributeControlType.FileUpload:
                            {
                                if (!String.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    var downloadGuidStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id).FirstOrDefault();
                                    Guid downloadGuid;
                                    Guid.TryParse(downloadGuidStr, out downloadGuid);
                                    var download = _downloadService.GetDownloadByGuid(downloadGuid);
                                    if (download != null)
                                        attributeModel.DefaultValue = download.DownloadGuid.ToString();
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                model.ProductAttributes.Add(attributeModel);
            }

            #endregion 

            #region Product specifications

            //do not prepare this model for the associated products. any it's not used
            if (!isAssociatedProduct)
            {
                model.ProductSpecifications = this.PrepareProductSpecificationModel(_workContext,
                    _specificationAttributeService,
                    _cacheManager,
                    product);
            }

            #endregion

            #region Product review overview

            model.ProductReviewOverview = new ProductReviewOverviewModel
            {
                ProductId = product.Id,
                RatingSum = product.ApprovedRatingSum,
                TotalReviews = product.ApprovedTotalReviews,
                AllowCustomerReviews = product.AllowCustomerReviews
            };

            #endregion

            #region Tier prices

            if (product.HasTierPrices && _permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {
                model.TierPrices = product.TierPrices
                    .OrderBy(x => x.Quantity)
                    .ToList()
                    .FilterByStore(_storeContext.CurrentStore.Id)
                    .FilterForCustomer(_workContext.CurrentCustomer)
                    .RemoveDuplicatedQuantities()
                    .Select(tierPrice =>
                    {
                        var m = new ProductDetailsModel.TierPriceModel
                        {
                            Quantity = tierPrice.Quantity,
                        };
                        decimal taxRate;
                        decimal priceBase = _taxService.GetProductPrice(product, _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer, decimal.Zero, _catalogSettings.DisplayTierPricesWithDiscounts, tierPrice.Quantity), out taxRate);
                        decimal price = _currencyService.ConvertFromPrimaryStoreCurrency(priceBase, _workContext.WorkingCurrency);
                        m.Price = _priceFormatter.FormatPrice(price, false, false);
                        return m;
                    })
                    .ToList();
            }

            #endregion
            
            #region Associated products

            if (product.ProductType == ProductType.GroupedProduct)
            {
                //ensure no circular references
                if (!isAssociatedProduct)
                {
                    var associatedProducts = _productService.GetAssociatedProducts(product.Id, _storeContext.CurrentStore.Id);
                    foreach (var associatedProduct in associatedProducts)
                        model.AssociatedProducts.Add(PrepareProductDetailsPageModel(associatedProduct, null, true));
                }
            }

            #endregion

            #region productreviews
            
            PrepareProductReviewsModel(model.ProductReviews, product);

            #endregion


            #region product destination
            var destination = product.ProductCategories.Where(pc => pc.Category.CategoryType == CategoryType.Destination).OrderBy(pc => pc.DisplayOrder).FirstOrDefault();
            if(destination != null)
            {
                var destinationModel = destination.Category.ToModel();

                string key = string.Format(ModelCacheEventConsumer.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                    _storeContext.CurrentStore.Id,
                    destination.Category.Id);

                destinationModel.NumberOfProducts = _cacheManager.Get(key, () =>
                {
                    var categoryIds = new List<int>();
                    categoryIds.Add(destination.Category.Id);
                    //include subcategories
                    if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                        categoryIds.AddRange(GetChildCategoryIds(destination.Category.Id));
                    return _productService.GetCategoryProductNumber(categoryIds, _storeContext.CurrentStore.Id);
                });

                //prepare picture model
                int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, destination.Category.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                destinationModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                {
                    var picture = _pictureService.GetPictureById(destination.Category.PictureId);
                    var pictureModel = new PictureModel
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                        ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), destinationModel.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), destinationModel.Name)
                    };
                    return pictureModel;
                });

                model.ProductDestination = destinationModel;

            }



            #endregion


            #region product attraction
            var attraction = product.ProductCategories.Where(pc => pc.Category.CategoryType == CategoryType.Attraction).OrderBy(pc => pc.DisplayOrder).FirstOrDefault();
            if (attraction != null)
            {
                var attractionModel = attraction.Category.ToModel();

                string key = string.Format(ModelCacheEventConsumer.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                    _storeContext.CurrentStore.Id,
                    attraction.Category.Id);

                attractionModel.NumberOfProducts = _cacheManager.Get(key, () =>
                {
                    var categoryIds = new List<int>();
                    categoryIds.Add(attraction.Category.Id);
                    //include subcategories
                    if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                        categoryIds.AddRange(GetChildCategoryIds(attraction.Category.Id));
                    return _productService.GetCategoryProductNumber(categoryIds, _storeContext.CurrentStore.Id);
                });

                //prepare picture model
                int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, attraction.Category.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                attractionModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                {
                    var picture = _pictureService.GetPictureById(attraction.Category.PictureId);
                    var pictureModel = new PictureModel
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                        ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), attractionModel.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), attractionModel.Name)
                    };
                    return pictureModel;
                });

                model.ProductAttraction = attractionModel;
            }



            #endregion

            return model;
        }

        #region Product details page

        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult ProductDetails(int productId, int updatecartitemid = 0)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted)
                return InvokeHttp404();

            //published?
            if (!_catalogSettings.AllowViewUnpublishedProductPage)
            {
                //Check whether the current user has a "Manage catalog" permission
                //It allows him to preview a product before publishing
                if (!product.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                    return InvokeHttp404();
            }

            //ACL (access control list)
            if (!_aclService.Authorize(product))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(product))
                return InvokeHttp404();

            //availability dates
            if (!product.IsAvailable())
                return InvokeHttp404();

            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return RedirectToRoute("HomePage");

                return RedirectToRoute("Product", new { SeName = parentGroupedProduct.GetSeName() });
            }

            //update existing shopping cart item?
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found?
                if (updatecartitem == null)
                {
                    return RedirectToRoute("Product", new { SeName = product.GetSeName() });
                }
                //is it this product?
                if (product.Id != updatecartitem.ProductId)
                {
                    return RedirectToRoute("Product", new { SeName = product.GetSeName() });
                }
            }

            //prepare the model
            var model = PrepareProductDetailsPageModel(product, updatecartitem, false);

            //save as recently viewed
            _recentlyViewedProductsService.AddProductToRecentlyViewedList(product.Id);

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewProduct", _localizationService.GetResource("ActivityLog.PublicStore.ViewProduct"), product.Name);

            return View("ProductTemplate.Grouped", model);
        }

        public ActionResult GetProductOptions(int productGroupId, DateTime date)
        {
            var productGroup = _productService.GetProductById(productGroupId);
            var options = _productOptionService.GetAllProductOptions(productId: productGroupId);
            var productSimples = _productService.GetAssociatedProducts(productGroupId);
           

            

            return View("_ProductOptions");
        }

        #endregion

        #region Product Option

        public ActionResult ProductOptions(int productId, string date)
        {
            var productGroup = _productService.GetProductById(productId);
            DateTime selectedDate = DateTime.Now;
            CultureInfo provider = CultureInfo.InvariantCulture;
            var format = "d";

            if (!DateTime.TryParseExact(date,
                                        "dd/MM/yyyy",
                                        provider,
                                        DateTimeStyles.None,
                out selectedDate))
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            //try
            //{
            //    selectedDate = DateTime.ParseExact(date, format, provider);
                
            //}
            //catch (FormatException)
            //{
                
            //}

            if (productGroup == null)
            {
                return Json(new { success = false}, JsonRequestBehavior.AllowGet);
            }

            var options = _productOptionService.GetAllProductOptions(productId: productGroup.Id).ToList();
            var model = new ProductDetailsModel();

            //update existing shopping cart item?
            //ShoppingCartItem updatecartitem = null;
            //if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            //{
            //    var cart = _workContext.CurrentCustomer.ShoppingCartItems
            //        .Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart)
            //        .LimitPerStore(_storeContext.CurrentStore.Id)
            //        .ToList();
            //    updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
            //    //not found?
            //    if (updatecartitem == null)
            //    {
            //        return RedirectToRoute("Product", new { SeName = product.GetSeName() });
            //    }
            //    //is it this product?
            //    if (product.Id != updatecartitem.ProductId)
            //    {
            //        return RedirectToRoute("Product", new { SeName = product.GetSeName() });
            //    }
            //}

            #region Product specifications

            //do not prepare this model for the associated products. any it's not used


            #endregion
            model.ProductOptions = options.Select(op =>
            {
                var m = new ProductOptionModel()
                {
                    ProductOptionId = op.Id,
                    Name = op.GetLocalized(o => o.Name),
                    Description = op.GetLocalized(o => o.Description)
                };

                

                foreach (var p in op.Products)
                {
                    bool show = true;
                    foreach (var availiableSetup in p.AvailabilitySetups)
                    {
                        if (!(selectedDate <= availiableSetup.ToDate && selectedDate >= availiableSetup.FromDate))
                            show = false;
                    }

                    if (p.PriceSetups.Count > 0)
                        show = false;

                    if (show)
                    {
                        var productSimple = new ProductSimpleModel()
                        {
                            Name = p.GetLocalized(c => c.Name),
                            Overview = p.GetLocalized(c => c.Overview),
                            ProductId = p.Id,
                            SeName = p.GetSeName(),
                            AgeRange = p.AgeRange,
                            AgeRangeCondition = p.AgeRangeCondition
                        };

                        //productSimple.ProductSpecifications = this.PrepareProductSpecificationModel(_workContext,
                        //_specificationAttributeService,
                        //_cacheManager,
                        //p).ToList();

                        productSimple.AgeRangeName = p.AgeRangeType.GetLocalizedEnum(_localizationService, _workContext);
                        productSimple.ProductPrice.HidePrices = false;

                        if (p.CustomerEntersPrice)
                        {
                            productSimple.ProductPrice.CustomerEntersPrice = true;
                        }
                        else
                        {
                            if (p.CallForPrice)
                            {
                                productSimple.ProductPrice.CallForPrice = true;
                            }
                            else
                            {
                                decimal taxRate;
                                decimal oldPriceBase = _taxService.GetProductPrice(p, p.OldPrice, out taxRate);
                                decimal finalPriceWithoutDiscountBase = _taxService.GetProductPrice(p, _priceCalculationService.GetFinalPrice(p, _workContext.CurrentCustomer, includeDiscounts: false), out taxRate);
                                decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(p, _priceCalculationService.GetFinalPrice(p, _workContext.CurrentCustomer, includeDiscounts: true), out taxRate);

                                decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                                decimal finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);

                                // get setup prices
                                var setupPrices = _priceSetupService.GetAllPriceSetups(productId: p.Id,
                                    customerRoleIds: _workContext.CurrentCustomer.CustomerRoles.Select(r => r.Id).ToList(), toDate: DateTime.Now);

                               
                                decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);


                                if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                                    productSimple.ProductPrice.OldPrice = _priceFormatter.FormatPrice(oldPrice);

                                productSimple.ProductPrice.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);

                                if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                                    productSimple.ProductPrice.PriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount);

                                productSimple.ProductPrice.PriceValue = finalPriceWithDiscount;

                                
                                //property for German market
                                //we display tax/shipping info only with "shipping enabled" for this product
                                //we also ensure this it's not free shipping
                                productSimple.ProductPrice.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductDetailsPage
                                    && p.IsShipEnabled &&
                                    !p.IsFreeShipping;

                                //PAngV baseprice (used in Germany)
                                productSimple.ProductPrice.BasePricePAngV = p.FormatBasePrice(finalPriceWithDiscountBase,
                                    _localizationService, _measureService, _currencyService, _workContext, _priceFormatter);

                                //currency code
                                productSimple.ProductPrice.CurrencyCode = _workContext.WorkingCurrency.CurrencyCode;

                                if (p.AgeRangeType == DvAgeRangeType.Adult)
                                {
                                    if (finalPriceWithDiscount < model.ProductPrice.PriceValue)
                                    {
                                        model.ProductPrice = productSimple.ProductPrice;
                                        model.ProductPrice.SavePercent = oldPrice * 100 / finalPriceWithoutDiscount;
                                    }

                                }
                                else
                                {
                                    productSimple.AvaliableQuantities.Add(new SelectListItem()
                                    {
                                        Text = _localizationService.GetResource("Products.SelectQuantity"),
                                        Value = "0"
                                    });
                                }

                                

                                for (int i = 1; i < 31; i++)
                                {
                                    decimal priceWithDiscountBase = _taxService.GetProductPrice(p, _priceCalculationService.GetFinalPrice(p, _workContext.CurrentCustomer, includeDiscounts: true, quantity: i), out taxRate);
                                    productSimple.AvaliableQuantities.Add(new SelectListItem()
                                    {
                                        Text = i + "(x) " + priceWithDiscountBase.ToString("N"),
                                        Value = i.ToString()
                                    });

                                }

                            }
                        }

                        #region Product attributes

                        //performance optimization
                        //We cache a value indicating whether a product has attributes
                        IList<ProductAttributeMapping> productAttributeMapping = null;
                        string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_HAS_PRODUCT_ATTRIBUTES_KEY, p.Id);
                        var hasProductAttributesCache = _cacheManager.Get<bool?>(cacheKey);
                        if (!hasProductAttributesCache.HasValue)
                        {
                            //no value in the cache yet
                            //let's load attributes and cache the result (true/false)
                            productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(p.Id);
                            hasProductAttributesCache = productAttributeMapping.Count > 0;
                            _cacheManager.Set(cacheKey, hasProductAttributesCache, 60);
                        }
                        if (hasProductAttributesCache.Value && productAttributeMapping == null)
                        {
                            //cache indicates that the product has attributes
                            //let's load them
                            productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(p.Id)
                                .Where(pa => pa.ProductAttribute.SystemName == ProductAttributeSystemNames.StartDate).ToList();
                        }
                        if (productAttributeMapping == null)
                        {
                            productAttributeMapping = new List<ProductAttributeMapping>();
                        }
                        foreach (var attribute in productAttributeMapping)
                        {
                            var attributeModel = new ProductDetailsModel.ProductAttributeModel
                            {
                                Id = attribute.Id,
                                ProductId = p.Id,
                                ProductAttributeId = attribute.ProductAttributeId,
                                Name = attribute.ProductAttribute.GetLocalized(x => x.Name),
                                Description = attribute.ProductAttribute.GetLocalized(x => x.Description),
                                TextPrompt = attribute.TextPrompt,
                                IsRequired = attribute.IsRequired,
                                AttributeControlType = attribute.AttributeControlType,
                                DefaultValue = attribute.DefaultValue,
                                HasCondition = !String.IsNullOrEmpty(attribute.ConditionAttributeXml)
                            };
                            if (!String.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                            {
                                attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                    .ToList();
                            }

                            if (attribute.ShouldHaveValues())
                            {
                                //values
                                var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                                foreach (var attributeValue in attributeValues)
                                {
                                    var valueModel = new ProductDetailsModel.ProductAttributeValueModel
                                    {
                                        Id = attributeValue.Id,
                                        Name = attributeValue.GetLocalized(x => x.Name),
                                        ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                                        IsPreSelected = attributeValue.IsPreSelected
                                    };
                                    attributeModel.Values.Add(valueModel);

                                    //display price if allowed
                                    if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                                    {
                                        decimal taxRate;
                                        decimal attributeValuePriceAdjustment = _priceCalculationService.GetProductAttributeValuePriceAdjustment(attributeValue);
                                        decimal priceAdjustmentBase = _taxService.GetProductPrice(p, attributeValuePriceAdjustment, out taxRate);
                                        decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                                        if (priceAdjustmentBase > decimal.Zero)
                                            valueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment, false, false);
                                        else if (priceAdjustmentBase < decimal.Zero)
                                            valueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment, false, false);

                                        valueModel.PriceAdjustmentValue = priceAdjustment;
                                    }
                                    
                                }
                            }

                            attributeModel.SelectedDay = selectedDate.Day;
                            attributeModel.SelectedMonth = selectedDate.Month;
                            attributeModel.SelectedYear = selectedDate.Year;

                            productSimple.ProductAttributes.Add(attributeModel);
                        }

                        #endregion

                        productSimple.SelectedDate = selectedDate;

                        m.ProductSimples.Add(productSimple);
                    }
                    
                }
                return m;
                

            }).ToList();
            
            var strHtml = RenderPartialViewToString("_ProductDetailsOptions", model);
            return Json(new { success = true, data = strHtml }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}