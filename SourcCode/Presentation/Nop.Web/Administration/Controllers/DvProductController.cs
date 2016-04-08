using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Infrastructure.Cache;
using Nop.Admin.Models.Catalog;
using Nop.Admin.Models.Orders;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Core.Caching;
using Nop.Services.Divui.Catalog;
using Nop.Core.Domain.Divui.Catalog;

namespace Nop.Admin.Controllers
{
    public partial class ProductController
    {
        #region Fields

        private readonly ICollectionService _collectionService;
        private readonly IAttractionService _attractionService;


        #endregion

        #region Constructors

        public ProductController(IProductService productService,
            IProductTemplateService productTemplateService,
            ICategoryService categoryService,
            ICollectionService collectionService,
            IAttractionService attractionService,
            IManufacturerService manufacturerService,
            ICustomerService customerService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            ISpecificationAttributeService specificationAttributeService,
            IPictureService pictureService,
            ITaxCategoryService taxCategoryService,
            IProductTagService productTagService,
            ICopyProductService copyProductService,
            IPdfService pdfService,
            IExportManager exportManager,
            IImportManager importManager,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
            IAclService aclService,
            IStoreService storeService,
            IOrderService orderService,
            IStoreMappingService storeMappingService,
             IVendorService vendorService,
            IShippingService shippingService,
            IShipmentService shipmentService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IMeasureService measureService,
            MeasureSettings measureSettings,
            ICacheManager cacheManager,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IProductAttributeService productAttributeService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            IShoppingCartService shoppingCartService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IDownloadService downloadService,
            IProductOptionService dvGroupProductService)
        {
            this._productService = productService;
            this._productTemplateService = productTemplateService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._customerService = customerService;
            this._urlRecordService = urlRecordService;
            this._workContext = workContext;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._specificationAttributeService = specificationAttributeService;
            this._pictureService = pictureService;
            this._taxCategoryService = taxCategoryService;
            this._productTagService = productTagService;
            this._copyProductService = copyProductService;
            this._pdfService = pdfService;
            this._exportManager = exportManager;
            this._importManager = importManager;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._aclService = aclService;
            this._storeService = storeService;
            this._orderService = orderService;
            this._storeMappingService = storeMappingService;
            this._vendorService = vendorService;
            this._shippingService = shippingService;
            this._shipmentService = shipmentService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
            this._cacheManager = cacheManager;
            this._dateTimeHelper = dateTimeHelper;
            this._discountService = discountService;
            this._productAttributeService = productAttributeService;
            this._backInStockSubscriptionService = backInStockSubscriptionService;
            this._shoppingCartService = shoppingCartService;
            this._productAttributeFormatter = productAttributeFormatter;
            this._productAttributeParser = productAttributeParser;
            this._downloadService = downloadService;
            this._productOptionService = dvGroupProductService;
            this._collectionService = collectionService;
            this._attractionService = attractionService;
        }

        #endregion 

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(id);
            if (product == null || product.Deleted)
                //No product found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            var model = product.ToModel();
            DvPrepareProductModel(model, product, false, false);

            // trungtp
            PrepareProductModel(model, product);

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = product.GetLocalized(x => x.Name, languageId, false, false);
                locale.ShortDescription = product.GetLocalized(x => x.ShortDescription, languageId, false, false);
                locale.FullDescription = product.GetLocalized(x => x.FullDescription, languageId, false, false);
                locale.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = product.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = product.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = product.GetSeName(languageId, false, false);
            });

            PrepareAclModel(model, product, false);
            PrepareStoresMappingModel(model, product, false);
            return View(model);
        }


        [NonAction]
        protected virtual void DvPrepareProductModel(ProductModel model, Product product,
            bool setPredefinedValues, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (product != null)
            {
                var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct != null)
                {
                    model.AssociatedToProductId = product.ParentGroupedProductId;
                    model.AssociatedToProductName = parentGroupedProduct.Name;
                }
            }

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;
            model.BaseDimensionIn = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId).Name;
            if (product != null)
            {
                model.CreatedOn = _dateTimeHelper.ConvertToUserTime(product.CreatedOnUtc, DateTimeKind.Utc);
                model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(product.UpdatedOnUtc, DateTimeKind.Utc);
            }

            //little performance hack here
            //there's no need to load attributes, categories, manufacturers when creating a new product
            //anyway they're not used (you need to save a product before you map add them)
            if (product != null)
            {
                //product attributes
                foreach (var productAttribute in _productAttributeService.GetAllProductAttributes())
                {
                    model.AvailableProductAttributes.Add(new SelectListItem
                    {
                        Text = productAttribute.Name,
                        Value = productAttribute.Id.ToString()
                    });
                }

                //manufacturers
                foreach (var manufacturer in _manufacturerService.GetAllManufacturers(showHidden: true))
                {
                    model.AvailableManufacturers.Add(new SelectListItem
                    {
                        Text = manufacturer.Name,
                        Value = manufacturer.Id.ToString()
                    });
                }

                //categories
                var allDestinations = _categoryService.GetAllCategories(showHidden: true, categoryTypeId: (int)CategoryType.Destination);
                foreach (var category in allDestinations)
                {
                    model.AvailableDestinations.Add(new SelectListItem
                    {
                        Text = category.GetFormattedBreadCrumb(allDestinations),
                        Value = category.Id.ToString()
                    });
                }

                //collections
                var allCollections = _categoryService.GetAllCategories(showHidden: true, categoryTypeId: (int)CategoryType.Collection);
                foreach (var collection in allCollections)
                {
                    model.AvailableCollections.Add(new SelectListItem
                    {
                        Text = collection.GetFormattedBreadCrumb(allCollections),
                        Value = collection.Id.ToString()
                    });
                }

                //attractions
                var allAttractions = _categoryService.GetAllCategories(showHidden: true, categoryTypeId: (int)CategoryType.Attraction);
                foreach (var attraction in allAttractions)
                {
                    model.AvailableAttractions.Add(new SelectListItem
                    {
                        Text = attraction.GetFormattedBreadCrumb(allAttractions),
                        Value = attraction.Id.ToString()
                    });
                }

                //attractions
                var allCategories = _categoryService.GetAllCategories(showHidden: true, categoryTypeId: (int)CategoryType.Category);
                foreach (var category in allCategories)
                {
                    model.AvailableCategories.Add(new SelectListItem
                    {
                        Text = category.GetFormattedBreadCrumb(allCategories),
                        Value = category.Id.ToString()
                    });
                }


                //specification attributes
                model.AddSpecificationAttributeModel.AvailableAttributes = _cacheManager
                    .Get(string.Format(ModelCacheEventConsumer.SPEC_ATTRIBUTES_MODEL_KEY, _workContext.WorkingLanguage.Id), () =>
                    {
                        var availableSpecificationAttributes = new List<SelectListItem>();
                        foreach (var sa in _specificationAttributeService.GetSpecificationAttributes())
                        {
                            availableSpecificationAttributes.Add(new SelectListItem
                            {
                                Text = sa.GetLocalized(s => s.Name),
                                Value = sa.Id.ToString()
                            });
                        }
                        return availableSpecificationAttributes;
                    });

                //options of preselected specification attribute
                if (model.AddSpecificationAttributeModel.AvailableAttributes.Any())
                {
                    var selectedAttributeId = int.Parse(model.AddSpecificationAttributeModel.AvailableAttributes.First().Value);
                    foreach (var sao in _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(selectedAttributeId))
                        model.AddSpecificationAttributeModel.AvailableOptions.Add(new SelectListItem
                        {
                            Text = sao.GetLocalized(s => s.Name),
                            Value = sao.Id.ToString()
                        });
                }
                //default specs values
                model.AddSpecificationAttributeModel.ShowOnProductPage = true;
            }


            //copy product
            if (product != null)
            {
                model.CopyProductModel.Id = product.Id;
                model.CopyProductModel.Name = "Copy of " + product.Name;
                model.CopyProductModel.Published = true;
                model.CopyProductModel.CopyImages = true;
            }

            //templates
            var templates = _productTemplateService.GetAllProductTemplates();
            foreach (var template in templates)
            {
                model.AvailableProductTemplates.Add(new SelectListItem
                {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }

            //vendors
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.AvailableVendors.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.Vendor.None"),
                Value = "0"
            });
            var vendors = _vendorService.GetAllVendors(showHidden: true);
            foreach (var vendor in vendors)
            {
                model.AvailableVendors.Add(new SelectListItem
                {
                    Text = vendor.Name,
                    Value = vendor.Id.ToString()
                });
            }

            //delivery dates
            model.AvailableDeliveryDates.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.DeliveryDate.None"),
                Value = "0"
            });
            var deliveryDates = _shippingService.GetAllDeliveryDates();
            foreach (var deliveryDate in deliveryDates)
            {
                model.AvailableDeliveryDates.Add(new SelectListItem
                {
                    Text = deliveryDate.Name,
                    Value = deliveryDate.Id.ToString()
                });
            }

            //warehouses
            var warehouses = _shippingService.GetAllWarehouses();
            model.AvailableWarehouses.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.Warehouse.None"),
                Value = "0"
            });
            foreach (var warehouse in warehouses)
            {
                model.AvailableWarehouses.Add(new SelectListItem
                {
                    Text = warehouse.Name,
                    Value = warehouse.Id.ToString()
                });
            }

            //multiple warehouses
            foreach (var warehouse in warehouses)
            {
                var pwiModel = new ProductModel.ProductWarehouseInventoryModel
                {
                    WarehouseId = warehouse.Id,
                    WarehouseName = warehouse.Name
                };
                if (product != null)
                {
                    var pwi = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouse.Id);
                    if (pwi != null)
                    {
                        pwiModel.WarehouseUsed = true;
                        pwiModel.StockQuantity = pwi.StockQuantity;
                        pwiModel.ReservedQuantity = pwi.ReservedQuantity;
                        pwiModel.PlannedQuantity = _shipmentService.GetQuantityInShipments(product, pwi.WarehouseId, true, true);
                    }
                }
                model.ProductWarehouseInventoryModels.Add(pwiModel);
            }

            //product tags
            if (product != null)
            {
                var result = new StringBuilder();
                for (int i = 0; i < product.ProductTags.Count; i++)
                {
                    var pt = product.ProductTags.ToList()[i];
                    result.Append(pt.Name);
                    if (i != product.ProductTags.Count - 1)
                        result.Append(", ");
                }
                model.ProductTags = result.ToString();
            }

            //tax categories
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            model.AvailableTaxCategories.Add(new SelectListItem { Text = "---", Value = "0" });
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString(), Selected = product != null && !setPredefinedValues && tc.Id == product.TaxCategoryId });

            //baseprice units
            var measureWeights = _measureService.GetAllMeasureWeights();
            foreach (var mw in measureWeights)
                model.AvailableBasepriceUnits.Add(new SelectListItem { Text = mw.Name, Value = mw.Id.ToString(), Selected = product != null && !setPredefinedValues && mw.Id == product.BasepriceUnitId });
            foreach (var mw in measureWeights)
                model.AvailableBasepriceBaseUnits.Add(new SelectListItem { Text = mw.Name, Value = mw.Id.ToString(), Selected = product != null && !setPredefinedValues && mw.Id == product.BasepriceBaseUnitId });

            //discounts
            model.AvailableDiscounts = _discountService
                .GetAllDiscounts(DiscountType.AssignedToSkus, showHidden: true)
                .Select(d => d.ToModel())
                .ToList();
            if (!excludeProperties && product != null)
            {
                model.SelectedDiscountIds = product.AppliedDiscounts.Select(d => d.Id).ToArray();
            }

            //default values
            if (setPredefinedValues)
            {
                model.MaximumCustomerEnteredPrice = 1000;
                model.MaxNumberOfDownloads = 10;
                model.RecurringCycleLength = 100;
                model.RecurringTotalCycles = 10;
                model.RentalPriceLength = 1;
                model.StockQuantity = 10000;
                model.NotifyAdminForQuantityBelow = 1;
                model.OrderMinimumQuantity = 1;
                model.OrderMaximumQuantity = 10000;

                model.UnlimitedDownloads = true;
                model.IsShipEnabled = true;
                model.AllowCustomerReviews = true;
                model.Published = true;
                model.VisibleIndividually = true;
            }
        }


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(ProductModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.Id);
            if (product == null || product.Deleted)
                //No product found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null)
                {
                    model.VendorId = _workContext.CurrentVendor.Id;
                }
                //vendors cannot edit "Show on home page" property
                if (_workContext.CurrentVendor != null && model.ShowOnHomePage != product.ShowOnHomePage)
                {
                    model.ShowOnHomePage = product.ShowOnHomePage;
                }
                //some previously used values
                var prevStockQuantity = product.GetTotalStockQuantity();
                int prevDownloadId = product.DownloadId;
                int prevSampleDownloadId = product.SampleDownloadId;

                //product
                product = model.ToEntity(product);
                product.UpdatedOnUtc = DateTime.UtcNow;
                _productService.UpdateProduct(product);
                //search engine name
                model.SeName = product.ValidateSeName(model.SeName, product.Name, true);
                _urlRecordService.SaveSlug(product, model.SeName, 0);
                //locales
                UpdateLocales(product, model);
                //tags
                SaveProductTags(product, ParseProductTags(model.ProductTags));
                //warehouses
                SaveProductWarehouseInventory(product, model);
                //ACL (customer roles)
                SaveProductAcl(product, model);
                //Stores
                SaveStoreMappings(product, model);
                //picture seo names
                UpdatePictureSeoNames(product);
                //discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, showHidden: true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    {
                        //new discount
                        if (product.AppliedDiscounts.Count(d => d.Id == discount.Id) == 0)
                            product.AppliedDiscounts.Add(discount);
                    }
                    else
                    {
                        //remove discount
                        if (product.AppliedDiscounts.Count(d => d.Id == discount.Id) > 0)
                            product.AppliedDiscounts.Remove(discount);
                    }
                }
                _productService.UpdateProduct(product);
                _productService.UpdateHasDiscountsApplied(product);
                //back in stock notifications
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    product.BackorderMode == BackorderMode.NoBackorders &&
                    product.AllowBackInStockSubscriptions &&
                    product.GetTotalStockQuantity() > 0 &&
                    prevStockQuantity <= 0 &&
                    product.Published &&
                    !product.Deleted)
                {
                    _backInStockSubscriptionService.SendNotificationsToSubscribers(product);
                }
                //delete an old "download" file (if deleted or updated)
                if (prevDownloadId > 0 && prevDownloadId != product.DownloadId)
                {
                    var prevDownload = _downloadService.GetDownloadById(prevDownloadId);
                    if (prevDownload != null)
                        _downloadService.DeleteDownload(prevDownload);
                }
                //delete an old "sample download" file (if deleted or updated)
                if (prevSampleDownloadId > 0 && prevSampleDownloadId != product.SampleDownloadId)
                {
                    var prevSampleDownload = _downloadService.GetDownloadById(prevSampleDownloadId);
                    if (prevSampleDownload != null)
                        _downloadService.DeleteDownload(prevSampleDownload);
                }

                //activity log
                _customerActivityService.InsertActivity("EditProduct", _localizationService.GetResource("ActivityLog.EditProduct"), product.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = product.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            DvPrepareProductModel(model, product, false, true);
            PrepareAclModel(model, product, true);
            PrepareStoresMappingModel(model, product, true);
            return View(model);
        }

        #region Product Option For Page Edit Product GroupedProduct

        
        // In page Product Detail for ProductType.GroupedProduct
        [HttpPost]
        public ActionResult GroupedProductProductOptionList(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var dvProductOptionsModel = _productOptionService.GetAllProductOptions(productId: productId, showHidden: true)
                .Select(x => new
                {
                    Id = x.Id,
                    ProductOptionId = x.Id,
                    Name = x.Name,
                    Published = x.Published,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = dvProductOptionsModel,
                Total = dvProductOptionsModel.Count
            };

            return Json(gridModel);
        }
        
        #endregion

        #region Product Option For Page Edit Product SimpleProduct
        // In page Product Detail for ProductType.GroupedProduct
        [HttpPost]
        public ActionResult SimpleProductProductOptionList(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var dvProductOptionsModel = product.ProductOptions
                .Select(x => new
                {
                    Id = x.Id,
                    ProductOptionId = x.Id,
                    Name = x.Name,
                    Published = x.Published,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = dvProductOptionsModel,
                Total = dvProductOptionsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult SimpleProductProductOptionInsert(ProductModel.ProductOptionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();


            var productOptionId = model.ProductOptionId;

            var product = _productService.GetProductById(model.ProductId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var productgroup = _productOptionService.GetProductOptionById(model.ProductOptionId);
            if (productgroup == null)
                throw new ArgumentException("No product group found with the specified id");

            if (!product.ProductOptions.Contains(productgroup))
            {
                product.ProductOptions.Add(productgroup);
                _productService.UpdateProduct(product);
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult SimpleProductProductOptionUpdate(ProductModel.ProductOptionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();


            var product = _productService.GetProductById(model.ProductId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var productgroup = _productOptionService.GetProductOptionById(model.ProductOptionId);
            if (productgroup == null)
                throw new ArgumentException("No product group found with the specified id");

            var oldproductgroup = _productOptionService.GetProductOptionById(model.Id);
            if (oldproductgroup == null)
                throw new ArgumentException("No product group found with the specified id");

            if (product.ProductOptions.Contains(oldproductgroup))
            {
                product.ProductOptions.Remove(oldproductgroup);

            }
            if (!product.ProductOptions.Contains(productgroup))
            {
                product.ProductOptions.Add(productgroup);

            }

            _productService.UpdateProduct(product);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult SimpleProductProductOptionDelete(int id, int ProductId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productOption = _productOptionService.GetProductOptionById(id);
            if (productOption == null)
                throw new ArgumentException("No product group found with the specified id");
            var prod = _productService.GetProductById(ProductId);
            productOption.Products.Remove(prod);
            _productOptionService.UpdateProductOption(productOption);

            return new NullJsonResult();
        }


        #endregion

        #region product option

        #region For Page Edit DvProductOption


        [HttpPost]
        public ActionResult ProductOptionProductList(DataSourceRequest command, int ProductOptionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();


            var groupProduct = _productOptionService.GetProductOptionById(ProductOptionId);
            if (groupProduct == null)
                throw new ArgumentException("No group product found with the specified id");

            var products = groupProduct.Products;
            var gridModel = new DataSourceResult
            {
                Data = products.Select(x => new
                {
                    Id = x.Id,
                    ProductId = x.Id,
                    ProductName = x.Name
                }),
                Total = products.Count
            };

            return Json(gridModel);
        }

        //[HttpPost]
        //public ActionResult DvProductOptionInsert(ProductModel.ProductOptionModel model)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
        //        return AccessDeniedView();

        //    var productId = model.ProductId;
        //    var groupProductId = model.ProductOptionId;

        //    var product = _productService.GetProductById(productId);
        //    //a vendor should have access only to his products
        //    if (_workContext.CurrentVendor != null)
        //    {

        //        if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
        //        {
        //            return Content("This is not your product");
        //        }
        //    }

        //    var productgroup = _dvGroupProductService.GetDvGroupProductById(model.Id);
        //    if (productgroup == null)
        //        throw new ArgumentException("No product group found with the specified id");

        //    if (!product.DvGroupProducts.Contains(productgroup))
        //    {
        //        product.DvGroupProducts.Add(productgroup);
        //        _productService.UpdateProduct(product);
        //    }

        //    return new NullJsonResult();
        //}

        public ActionResult ProductOptionProductUpdate(int Id, int ProductId, int ProductOptionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productOption = _productOptionService.GetProductOptionById(ProductOptionId);
            if (productOption == null)
                throw new ArgumentException("No group product found with the specified id");


            var oldProd = _productService.GetProductById(Id);
            var newProd = _productService.GetProductById(ProductId);

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (oldProd != null && oldProd.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }

                if (newProd != null && newProd.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            if (productOption.Products.Contains(oldProd))
                productOption.Products.Remove(oldProd);

            if (!productOption.Products.Contains(newProd))
                productOption.Products.Add(newProd);

            _productOptionService.UpdateProductOption(productOption);

            return new NullJsonResult();
        }

        public ActionResult ProductOptionProductDelete(int Id, int ProductOptionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productOption = _productOptionService.GetProductOptionById(ProductOptionId);
            if (productOption == null)
                throw new ArgumentException("No group product found with the specified id");

            var prod = _productService.GetProductById(Id);
            productOption.Products.Remove(prod);
            _productOptionService.UpdateProductOption(productOption);

            return new NullJsonResult();
        }

        public ActionResult ProductOptionAddPopup(int ProductOptionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            ViewBag.ProductOptionId = ProductOptionId;
            return View();
        }

        [HttpPost]
        public ActionResult ProductOptionAddPopupList(DataSourceRequest command, int ProductOptionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productOption = _productOptionService.GetProductOptionById(ProductOptionId);
            if (productOption == null)
                throw new ArgumentException("No group product found with the specified id");

            var gridModel = new DataSourceResult();
            var products = _productService.GetAssociatedProducts(productOption.ProductId);

            var list = new PagedList<Product>(products, pageIndex: command.Page - 1, pageSize: command.PageSize);

            gridModel.Data = list.Select(x => x.ToModel());
            gridModel.Total = list.TotalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult ProductOptionAddPopup(string btnId, string formId, int[] SelectedProductIds, int ProductOptionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productOption = _productOptionService.GetProductOptionById(ProductOptionId);

            if (productOption == null)
                throw new ArgumentException("No group product found with the specified id");

            if (SelectedProductIds != null)
            {
                foreach (int id in SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        if (!productOption.Products.Contains(product))
                        {
                            productOption.Products.Add(product);
                            _productOptionService.UpdateProductOption(productOption);
                        }
                    }
                }
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View();
        }
        #endregion

        #region Insert / Update / Delete ProductOption
        public ActionResult CreateProductOption(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new ProductModel.ProductOptionModel();

            model.ProductId = productId;

            //locales
            AddLocales(_languageService, model.Locales);


            return View(model);
        }


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult CreateProductOption(ProductModel.ProductOptionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productId = model.ProductId;

            var product = _productService.GetProductById(productId);

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }


            if (ModelState.IsValid)
            {
                var productgroup = model.ToEntity();
                productgroup.CreatedOnUtc = DateTime.UtcNow;
                productgroup.UpdatedOnUtc = DateTime.UtcNow;
                _productOptionService.InsertProductOption(productgroup);

                //search engine name
                model.SeName = productgroup.ValidateSeName(model.SeName, productgroup.Name, true);
                _urlRecordService.SaveSlug(productgroup, model.SeName, 0);

                //locales
                UpdateLocales(productgroup, model);

                _productOptionService.UpdateProductOption(productgroup);

                //activity log
                _customerActivityService.InsertActivity("AddProductOption", _localizationService.GetResource("ActivityLog.AddProductOption"), productgroup.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.ProductOptions.Added"));
                return continueEditing ? RedirectToAction("EditProductOption", new { id = productgroup.Id }) : RedirectToAction("Edit", new { id = productgroup.ProductId });
            }

            return View(model);
        }


        public ActionResult EditProductOption(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productgroup = _productOptionService.GetProductOptionById(id);
            if (productgroup == null || productgroup.Deleted)
                //No category found with the specified id
                return RedirectToAction("List");

            var model = productgroup.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = productgroup.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = productgroup.GetLocalized(x => x.Description, languageId, false, false);
                locale.SeName = productgroup.GetSeName(languageId, false, false);
            });

            model.AvailableProducts.AddRange(_productService.GetAssociatedProducts(productgroup.ProductId).Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() }));

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult EditProductOption(ProductModel.ProductOptionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productgroup = _productOptionService.GetProductOptionById(model.Id);
            if (productgroup == null)
                throw new ArgumentException("No product group found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productgroup.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            if (ModelState.IsValid)
            {
                productgroup = model.ToEntity(productgroup);
                productgroup.UpdatedOnUtc = DateTime.UtcNow;
                _productOptionService.UpdateProductOption(productgroup);
                //search engine name
                model.SeName = productgroup.ValidateSeName(model.SeName, productgroup.Name, true);
                _urlRecordService.SaveSlug(productgroup, model.SeName, 0);
                //locales
                UpdateLocales(productgroup, model);

            }

            //activity log
            _customerActivityService.InsertActivity("EditProductOption", _localizationService.GetResource("ActivityLog.EditProductOption"), productgroup.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.ProductOptions.Updated"));

            return continueEditing ? RedirectToAction("EditProductOption", new { id = productgroup.Id }) : RedirectToAction("Edit", new { id = productgroup.ProductId });
        }

        [HttpPost]
        public ActionResult DeleteProductOption(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var dvGroupProduct = _productOptionService.GetProductOptionById(id);
            if (dvGroupProduct == null)
                throw new ArgumentException("No dv group product ound with the specified id");

            var productId = dvGroupProduct.ProductId;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            dvGroupProduct.Products.Clear();
            _productOptionService.UpdateProductOption(dvGroupProduct);
            _productOptionService.DeleteProductOption(dvGroupProduct);

            return new NullJsonResult();
        }

        #endregion

        #endregion


        #region Utilities

        [NonAction]
        protected virtual void PrepareProductModel(ProductModel model, Product product)
        {
            if (product.ParentGroupedProductId > 0)
            {
                var allProductOptions = _productOptionService.GetAllProductOptions(productId: product.ParentGroupedProductId, showHidden: true);
                foreach (var group in allProductOptions)
                {
                    model.AvailableProductOptions.Add(new SelectListItem
                    {
                        Text = group.Name,
                        Value = group.Id.ToString()
                    });
                }
            }

        }
        [NonAction]
        protected virtual void UpdateLocales(ProductOption groupProduct, ProductModel.ProductOptionModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(groupProduct,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(groupProduct,
                                                           x => x.Description,
                                                           localized.Description,
                                                           localized.LanguageId);

                //search engine name
                var seName = groupProduct.ValidateSeName(localized.SeName, localized.Name, false);
                _urlRecordService.SaveSlug(groupProduct, seName, localized.LanguageId);
            }
        }
        #endregion

        #region Product collections

        [HttpPost]
        public ActionResult ProductCollectionList(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var productCollections = _collectionService.GetProductCollectionsByProductId(productId, true);
            var productCollectionsModel = productCollections
                .Select(x => new ProductModel.ProductCollectionModel
                {
                    Id = x.Id,
                    Collection = _collectionService.GetCollectionById(x.CollectionId).GetFormattedBreadCrumb(_collectionService),
                    ProductId = x.ProductId,
                    CollectionId = x.CollectionId,
                    IsFeaturedProduct = x.IsFeaturedProduct,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = productCollectionsModel,
                Total = productCollectionsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult ProductCollectionInsert(ProductModel.ProductCollectionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productId = model.ProductId;
            var collectionId = model.CollectionId;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var existingProductCollections = _collectionService.GetProductCollectionsByCollectionId(collectionId, showHidden: true);
            if (existingProductCollections.FindProductCollection(productId, collectionId) == null)
            {
                var productCollection = new ProductCollection
                {
                    ProductId = productId,
                    CollectionId = collectionId,
                    DisplayOrder = model.DisplayOrder
                };
                //a vendor cannot edit "IsFeaturedProduct" property
                if (_workContext.CurrentVendor == null)
                {
                    productCollection.IsFeaturedProduct = model.IsFeaturedProduct;
                }
                _collectionService.InsertProductCollection(productCollection);
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult ProductCollectionUpdate(ProductModel.ProductCollectionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productCollection = _collectionService.GetProductCollectionById(model.Id);
            if (productCollection == null)
                throw new ArgumentException("No product collection mapping found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productCollection.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            productCollection.CollectionId = model.CollectionId;
            productCollection.DisplayOrder = model.DisplayOrder;
            //a vendor cannot edit "IsFeaturedProduct" property
            if (_workContext.CurrentVendor == null)
            {
                productCollection.IsFeaturedProduct = model.IsFeaturedProduct;
            }
            _collectionService.UpdateProductCollection(productCollection);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult ProductCollectionDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productCollection = _collectionService.GetProductCollectionById(id);
            if (productCollection == null)
                throw new ArgumentException("No product collection mapping found with the specified id");

            var productId = productCollection.ProductId;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            _collectionService.DeleteProductCollection(productCollection);

            return new NullJsonResult();
        }

        #endregion


        #region Product attractions

        [HttpPost]
        public ActionResult ProductAttractionList(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var productAttractions = _attractionService.GetProductAttractionsByProductId(productId, true);
            var productAttractionsModel = productAttractions
                .Select(x => new ProductModel.ProductAttractionModel
                {
                    Id = x.Id,
                    Attraction = _attractionService.GetAttractionById(x.AttractionId).GetFormattedBreadCrumb(_attractionService),
                    ProductId = x.ProductId,
                    AttractionId = x.AttractionId,
                    IsFeaturedProduct = x.IsFeaturedProduct,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = productAttractionsModel,
                Total = productAttractionsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult ProductAttractionInsert(ProductModel.ProductAttractionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productId = model.ProductId;
            var attractionId = model.AttractionId;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var existingProductAttractions = _attractionService.GetProductAttractionsByAttractionId(attractionId, showHidden: true);
            if (existingProductAttractions.FindProductAttraction(productId, attractionId) == null)
            {
                var productAttraction = new ProductAttraction
                {
                    ProductId = productId,
                    AttractionId = attractionId,
                    DisplayOrder = model.DisplayOrder
                };
                //a vendor cannot edit "IsFeaturedProduct" property
                if (_workContext.CurrentVendor == null)
                {
                    productAttraction.IsFeaturedProduct = model.IsFeaturedProduct;
                }
                _attractionService.InsertProductAttraction(productAttraction);
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult ProductAttractionUpdate(ProductModel.ProductAttractionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productAttraction = _attractionService.GetProductAttractionById(model.Id);
            if (productAttraction == null)
                throw new ArgumentException("No product attraction mapping found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productAttraction.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            productAttraction.AttractionId = model.AttractionId;
            productAttraction.DisplayOrder = model.DisplayOrder;
            //a vendor cannot edit "IsFeaturedProduct" property
            if (_workContext.CurrentVendor == null)
            {
                productAttraction.IsFeaturedProduct = model.IsFeaturedProduct;
            }
            _attractionService.UpdateProductAttraction(productAttraction);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult ProductAttractionDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productAttraction = _attractionService.GetProductAttractionById(id);
            if (productAttraction == null)
                throw new ArgumentException("No product attraction mapping found with the specified id");

            var productId = productAttraction.ProductId;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            _attractionService.DeleteProductAttraction(productAttraction);

            return new NullJsonResult();
        }

        #endregion
        #region product categories
        [HttpPost]
        public ActionResult ProductCategoryList(DataSourceRequest command, int productId, int categoryTypeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var productCategories = _categoryService.GetProductCategoriesByProductIdAndCategoryTypeId(productId, categoryTypeId, true);
            var productCategoriesModel = productCategories
                .Select(x => new ProductModel.ProductCategoryModel
                {
                    Id = x.Id,
                    Category = _categoryService.GetCategoryById(x.CategoryId).GetFormattedBreadCrumb(_categoryService),
                    ProductId = x.ProductId,
                    CategoryId = x.CategoryId,
                    IsFeaturedProduct = x.IsFeaturedProduct,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = productCategoriesModel,
                Total = productCategoriesModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult ProductCategoryInsert(ProductModel.ProductCategoryModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productId = model.ProductId;
            var categoryId = model.CategoryId;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var existingProductCategies = _categoryService.GetProductCategoriesByCategoryId(categoryId, showHidden: true);
            if (existingProductCategies.FindProductCategory(productId, categoryId) == null)
            {
                var productCategory = new ProductCategory
                {
                    ProductId = productId,
                    CategoryId = categoryId,
                    DisplayOrder = model.DisplayOrder
                };
                //a vendor cannot edit "IsFeaturedProduct" property
                if (_workContext.CurrentVendor == null)
                {
                    productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
                }
                _categoryService.InsertProductCategory(productCategory);
            }

            return new NullJsonResult();
        }


        [HttpPost]
        public ActionResult ProductCategoryUpdate(ProductModel.ProductCategoryModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productCategory = _categoryService.GetProductCategoryById(model.Id);
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productCategory.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            productCategory.CategoryId = model.CategoryId;
            productCategory.DisplayOrder = model.DisplayOrder;
            //a vendor cannot edit "IsFeaturedProduct" property
            if (_workContext.CurrentVendor == null)
            {
                productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
            }
            _categoryService.UpdateProductCategory(productCategory);

            return new NullJsonResult();
        }


        #endregion
    }
}