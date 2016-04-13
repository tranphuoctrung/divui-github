using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Extensions;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Security;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Services.Divui.Catalog;
using Nop.Web.Models.Divui.Banners;
using Nop.Core.Domain.Divui.Catalog;

namespace Nop.Web.Controllers
{
    public partial class CatalogController
    {
        #region Fields
        private readonly ICollectionService _collectionService;
        private readonly IAttractionService _attractionService;
        private readonly IBannerService _bannerService;

        #endregion

        #region Constructors

        public CatalogController(ICategoryService categoryService,
            ICollectionService collectionService,
            IAttractionService attractionService,
            IManufacturerService manufacturerService,
            IProductService productService,
            IVendorService vendorService,
            ICategoryTemplateService categoryTemplateService,
            IManufacturerTemplateService manufacturerTemplateService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IWebHelper webHelper,
            ISpecificationAttributeService specificationAttributeService,
            IProductTagService productTagService,
            IGenericAttributeService genericAttributeService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            ICustomerActivityService customerActivityService,
            ITopicService topicService,
            IEventPublisher eventPublisher,
            ISearchTermService searchTermService,
            MediaSettings mediaSettings,
            CatalogSettings catalogSettings,
            VendorSettings vendorSettings,
            BlogSettings blogSettings,
            ForumSettings forumSettings,
            ICacheManager cacheManager,
            IBannerService bannerService)
        {
            this._categoryService = categoryService;
            this._collectionService = collectionService;
            this._attractionService = attractionService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._vendorService = vendorService;
            this._categoryTemplateService = categoryTemplateService;
            this._manufacturerTemplateService = manufacturerTemplateService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._webHelper = webHelper;
            this._specificationAttributeService = specificationAttributeService;
            this._productTagService = productTagService;
            this._genericAttributeService = genericAttributeService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;
            this._customerActivityService = customerActivityService;
            this._topicService = topicService;
            this._eventPublisher = eventPublisher;
            this._searchTermService = searchTermService;
            this._mediaSettings = mediaSettings;
            this._catalogSettings = catalogSettings;
            this._vendorSettings = vendorSettings;
            this._blogSettings = blogSettings;
            this._forumSettings = forumSettings;
            this._cacheManager = cacheManager;
            this._bannerService = bannerService;
        }

        #endregion
        #region Categories

        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult Category(int categoryId, CatalogPagingFilteringModel command)
        {
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null || category.Deleted)
                return InvokeHttp404();

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a category before publishing
            if (!category.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(category))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(category))
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            var model = category.ToModel();




            //sorting
            PrepareSortingOptions(model.PagingFilteringContext, command);
            //view mode
            PrepareViewModes(model.PagingFilteringContext, command);
            //page size
            PreparePageSizeOptions(model.PagingFilteringContext, command,
                category.AllowCustomersToSelectPageSize,
                category.PageSizeOptions,
                category.PageSize);

            //price ranges
            model.PagingFilteringContext.PriceRangeFilter.LoadPriceRangeFilters(category.PriceRanges, _webHelper, _priceFormatter);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_webHelper, category.PriceRanges);
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _workContext.WorkingCurrency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _workContext.WorkingCurrency);
            }


            //category breadcrumb
            if (_catalogSettings.CategoryBreadcrumbEnabled)
            {
                model.DisplayCategoryBreadcrumb = true;

                string breadcrumbCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_BREADCRUMB_KEY,
                    categoryId,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                    _storeContext.CurrentStore.Id,
                    _workContext.WorkingLanguage.Id);
                model.CategoryBreadcrumb = _cacheManager.Get(breadcrumbCacheKey, () =>
                    category
                    .GetCategoryBreadCrumb(_categoryService, _aclService, _storeMappingService)
                    .Select(catBr => new CategoryModel
                    {
                        Id = catBr.Id,
                        Name = catBr.GetLocalized(x => x.Name),
                        SeName = catBr.GetSeName()
                    })
                    .ToList()
                );
            }

            model.ParentName = category.Name;

            model.ParentCategoryId = category.ParentCategoryId;

            var parentCategoryId = categoryId;
            if(category.ParentCategoryId != 0)
            {
                parentCategoryId = category.ParentCategoryId;
                model.ParentName = _categoryService.GetCategoryById(parentCategoryId).Name;
            }

            //subcategories
            string subCategoriesCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_SUBCATEGORIES_KEY,
                parentCategoryId,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id,
                _webHelper.IsCurrentConnectionSecured());

            model.SubCategories = _cacheManager.Get(subCategoriesCacheKey, () =>
                _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId)
                .Select(x =>
                {
                    var subCatModel = new CategoryModel.SubCategoryModel
                    {
                        Id = x.Id,
                        Name = x.GetLocalized(y => y.Name),
                        SeName = x.GetSeName(),
                        Description = x.GetLocalized(y => y.Description)
                    };

                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    subCatModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(x.PictureId);
                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), subCatModel.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), subCatModel.Name)
                        };
                        return pictureModel;
                    });

                    string cachekey = string.Format(ModelCacheEventConsumer.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY,
                            string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                            _storeContext.CurrentStore.Id,
                            x.Id);
                    
                    subCatModel.NumberOfProducts = _cacheManager.Get(cachekey, () =>
                    {
                        var cateIds = new List<int>();
                        cateIds.Add(category.Id);
                        //include subcategories
                        if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                            cateIds.AddRange(GetChildCategoryIds(x.Id));
                        return _productService.GetCategoryProductNumber(cateIds, _storeContext.CurrentStore.Id);
                    });

                    return subCatModel;
                })
                .ToList()
            );


            #region Product specifications

            model.CategorySpecifications = this.PrepareCategorySpecificationModel(_workContext,
                   _specificationAttributeService,
                   _cacheManager,
                   category).ToList();

            #endregion

            //prepare picture model
            int picSize = _mediaSettings.CategoryThumbPictureSize;
            var catePictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, model.Id, picSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
            model.PictureModel = _cacheManager.Get(catePictureCacheKey, () =>
            {
                var picture = _pictureService.GetPictureById(category.PictureId);
                var pictureModel = new PictureModel
                {
                    FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                    ImageUrl = _pictureService.GetPictureUrl(picture, picSize),
                    Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), model.Name),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), model.Name)
                };
                return pictureModel;
            });

            //featured products
            if (!_catalogSettings.IgnoreFeaturedProducts)
            {
                //We cache a value indicating whether we have featured products
                IPagedList<Product> featuredProducts = null;
                string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_HAS_FEATURED_PRODUCTS_KEY, categoryId,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()), _storeContext.CurrentStore.Id);
                var hasFeaturedProductsCache = _cacheManager.Get<bool?>(cacheKey);
                if (!hasFeaturedProductsCache.HasValue)
                {
                    //no value in the cache yet
                    //let's load products and cache the result (true/false)
                    featuredProducts = _productService.SearchProducts(
                       categoryIds: new List<int> { category.Id },
                       storeId: _storeContext.CurrentStore.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                    hasFeaturedProductsCache = featuredProducts.TotalCount > 0;
                    _cacheManager.Set(cacheKey, hasFeaturedProductsCache, 60);
                }
                if (hasFeaturedProductsCache.Value && featuredProducts == null)
                {
                    //cache indicates that the category has featured products
                    //let's load them
                    featuredProducts = _productService.SearchProducts(
                       categoryIds: new List<int> { category.Id },
                       storeId: _storeContext.CurrentStore.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                }
                if (featuredProducts != null)
                {
                    model.FeaturedProducts = PrepareProductOverviewModels(featuredProducts).ToList();
                }
            }


            var categoryIds = new List<int>();
            categoryIds.Add(category.Id);
            if (_catalogSettings.ShowProductsFromSubcategories)
            {
                //include subcategories
                categoryIds.AddRange(GetChildCategoryIds(category.Id));
            }
            //products
            IList<int> alreadyFilteredSpecOptionIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);
            IList<int> filterableSpecificationAttributeOptionIds;
            var products = _productService.SearchProducts(out filterableSpecificationAttributeOptionIds,
                true,
                categoryIds: categoryIds,
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                featuredProducts: null,
                priceMin: minPriceConverted,
                priceMax: maxPriceConverted,
                filteredSpecs: alreadyFilteredSpecOptionIds,
                orderBy: (ProductSortingEnum)command.OrderBy,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize
                );

            var orderedproducts = products.OrderBy(p => p.ApprovedTotalReviews).ThenBy(p => p.DisplayOrder).ToList();
            
            

            model.PagingFilteringContext.LoadPagedList(products);

            //specs
            model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
                filterableSpecificationAttributeOptionIds != null ? filterableSpecificationAttributeOptionIds.ToArray() : null,
                _specificationAttributeService,
                _webHelper,
                _workContext,
                _cacheManager);

            model.SpecificationAttributes = _specificationAttributeService.GetProductSpecificationAttributes(allowFiltering: true)
                .Select(ps => ps.SpecificationAttributeOption.SpecificationAttribute)
                .Distinct().OrderBy(spec => spec.DisplayOrder).ToList();

            model.AlreadyFilteredSpecOptionIds = alreadyFilteredSpecOptionIds.ToList();

            //template
            var templateCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_TEMPLATE_MODEL_KEY, category.CategoryTemplateId);
            var templateViewPath = _cacheManager.Get(templateCacheKey, () =>
            {
                var template = _categoryTemplateService.GetCategoryTemplateById(category.CategoryTemplateId);
                if (template == null)
                    template = _categoryTemplateService.GetAllCategoryTemplates().FirstOrDefault();
                if (template == null)
                    throw new Exception("No default template could be loaded");
                return template.ViewPath;
            });

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewCategory", _localizationService.GetResource("ActivityLog.PublicStore.ViewCategory"), category.Name);
            var productCategories = new List<ProductCategory>();
            foreach (var p in orderedproducts)
            {
                var pcs = _categoryService.GetProductCategoriesByProductId(p.Id);
                if (pcs != null && pcs.Count > 0)
                    productCategories.AddRange(pcs);
            }

            int defaultProductNumber = 8;

            switch (category.CategoryType)
            {
                case CategoryType.Destination:
                    defaultProductNumber = _catalogSettings.DefaultDestinationProductNumber == 0 ? 8 : _catalogSettings.DefaultDestinationProductNumber;

                    model.Products = PrepareProductOverviewModels(orderedproducts.Take(defaultProductNumber)).ToList();

                    var categories = productCategories.Where(c => c.Category.CategoryTypeId == (int)CategoryType.Category)
                        .OrderBy(pc => pc.DisplayOrder).Select(pc => pc.Category).Distinct();

                    model.Categories = categories.Select(x =>
                    {
                        var catModel = x.ToModel();

                        string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY,
                            string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                            _storeContext.CurrentStore.Id,
                            x.Id);

                        catModel.NumberOfProducts = _cacheManager.Get(cacheKey, () =>
                        {
                            var cateIds = new List<int>();
                            cateIds.Add(x.Id);
                            //include subcategories
                            if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                                cateIds.AddRange(GetChildCategoryIds(x.Id));
                            return _productService.GetCategoryProductNumber(cateIds, _storeContext.CurrentStore.Id);
                        });

                        //prepare picture model
                        int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                        var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                        catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                        {
                            var picture = _pictureService.GetPictureById(x.PictureId);
                            var pictureModel = new PictureModel
                            {
                                FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                                ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                                Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                                AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                            };
                            return pictureModel;
                        });

                        return catModel;
                    }).ToList();


                    var attractions = productCategories.Where(c => c.Category.CategoryTypeId == (int)CategoryType.Attraction)
                        .OrderBy(pc => pc.DisplayOrder).Select(pc => pc.Category).Distinct().ToList();

                    if(category.ParentCategoryId > 0)
                        attractions = attractions.Take(7).ToList();
                    
                    else
                        attractions = attractions.Take(10).ToList();

                    model.Attractions = attractions.Select(x =>
                    {
                        var catModel = x.ToModel();

                        string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY,
                            string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                            _storeContext.CurrentStore.Id,
                            x.Id);

                        catModel.NumberOfProducts = _cacheManager.Get(cacheKey, () =>
                        {
                            var cateIds = new List<int>();
                            cateIds.Add(x.Id);
                            //include subcategories
                            if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                                cateIds.AddRange(GetChildCategoryIds(x.Id));
                            return _productService.GetCategoryProductNumber(cateIds, _storeContext.CurrentStore.Id);
                        });

                        //prepare picture model
                        int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                        var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                        catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                        {
                            var picture = _pictureService.GetPictureById(x.PictureId);
                            var pictureModel = new PictureModel
                            {
                                FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                                ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                                Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                                AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                            };
                            return pictureModel;
                        });

                        return catModel;
                    }).ToList();

                    var collections = productCategories.Where(c => c.Category.CategoryTypeId == (int)CategoryType.Collection).OrderBy(pc => pc.DisplayOrder).Select(pc => pc.Category).Distinct()
                        .Take(3);
                    model.Collections = collections.Select(x =>
                    {
                        var catModel = x.ToModel();

                        string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY,
                            string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                            _storeContext.CurrentStore.Id,
                            x.Id);

                        catModel.NumberOfProducts = _cacheManager.Get(cacheKey, () =>
                        {
                            var cateIds = new List<int>();
                            cateIds.Add(x.Id);
                            //include subcategories
                            if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                                cateIds.AddRange(GetChildCategoryIds(x.Id));
                            return _productService.GetCategoryProductNumber(cateIds, _storeContext.CurrentStore.Id);
                        });

                        //prepare picture model
                        int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                        var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                        catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                        {
                            var picture = _pictureService.GetPictureById(x.PictureId);
                            var pictureModel = new PictureModel
                            {
                                FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                                ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                                Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                                AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                            };
                            return pictureModel;
                        });

                        return catModel;
                    }).ToList();
                    if(category.ParentCategoryId > 0)
                        return View("CityTemplate.ProductsInGridOrLines", model);
                    return View("CountryTemplate.ProductsInGridOrLines", model);
                case CategoryType.Category:
                    return View("CategoryTemplate.ProductsInGridOrLines", model);
                case CategoryType.Attraction:
                    defaultProductNumber = _catalogSettings.DefaultAttractionProductNumber == 0 ? 8 : _catalogSettings.DefaultDestinationProductNumber;

                    model.Products = PrepareProductOverviewModels(orderedproducts.Take(defaultProductNumber)).ToList();


                    var attractions2 = productCategories.OrderBy(pc => pc.DisplayOrder).Select(pc => pc.Category).Distinct()
                       .Where(c => c.CategoryTypeId == (int)CategoryType.Attraction).Take(7);
                    model.Attractions = attractions2.Select(x =>
                    {
                        var catModel = x.ToModel();

                        string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY,
                            string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                            _storeContext.CurrentStore.Id,
                            x.Id);

                        catModel.NumberOfProducts = _cacheManager.Get(cacheKey, () =>
                        {
                            var cateIds = new List<int>();
                            cateIds.Add(x.Id);
                            //include subcategories
                            if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                                cateIds.AddRange(GetChildCategoryIds(x.Id));
                            return _productService.GetCategoryProductNumber(cateIds, _storeContext.CurrentStore.Id);
                        });

                        //prepare picture model
                        int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                        var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                        catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                        {
                            var picture = _pictureService.GetPictureById(x.PictureId);
                            var pictureModel = new PictureModel
                            {
                                FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                                ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                                Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                                AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                            };
                            return pictureModel;
                        });

                        return catModel;
                    }).ToList();
                    return View("AttractionTemplate.ProductsInGridOrLines", model);
                case CategoryType.Collection:
                    defaultProductNumber = _catalogSettings.DefaultCollectionProductNumber == 0 ? 8 : _catalogSettings.DefaultDestinationProductNumber;

                    model.Products = PrepareProductOverviewModels(orderedproducts.Take(defaultProductNumber)).ToList();

                    return View("CollectionTemplate.ProductsInGridOrLines", model);
                default:
                    return View(templateViewPath, model);
            }
        }

        public ActionResult LoadMoreProductCategory(int categoryId, CatalogPagingFilteringModel command)
        {
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null || category.Deleted)
                return InvokeHttp404();

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a category before publishing
            if (!category.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(category))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(category))
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            var model = category.ToModel();
            var categoryIds = new List<int>();
            categoryIds.Add(category.Id);
            if (_catalogSettings.ShowProductsFromSubcategories)
            {
                //include subcategories
                categoryIds.AddRange(GetChildCategoryIds(category.Id));
            }
            //products
            IList<int> alreadyFilteredSpecOptionIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);
            IList<int> filterableSpecificationAttributeOptionIds;
            var products = _productService.DvSearchProducts(out filterableSpecificationAttributeOptionIds,
                true,
                categoryIds: categoryIds,
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                featuredProducts: false,
                filteredSpecs: alreadyFilteredSpecOptionIds,
                orderBy: (ProductSortingEnum)command.OrderBy,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize
                );
            model.Products = PrepareProductOverviewModels(products).ToList();

            model.PagingFilteringContext.LoadPagedList(products);

            var result = new
            {
                success = true,
                html = RenderPartialViewToString("_LoadMoreProductCategory", model),
                totalPages = model.PagingFilteringContext.TotalPages
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Collections

        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult Collection(int collectionId, CatalogPagingFilteringModel command)
        {
            var collection = _collectionService.GetCollectionById(collectionId);
            if (collection == null || collection.Deleted)
                return InvokeHttp404();

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a collection before publishing
            if (!collection.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(collection))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(collection))
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            var model = collection.ToModel();




            //sorting
            PrepareSortingOptions(model.PagingFilteringContext, command);
            //view mode
            PrepareViewModes(model.PagingFilteringContext, command);
            //page size
            PreparePageSizeOptions(model.PagingFilteringContext, command,
                collection.AllowCustomersToSelectPageSize,
                collection.PageSizeOptions,
                collection.PageSize);

            //price ranges
            model.PagingFilteringContext.PriceRangeFilter.LoadPriceRangeFilters(collection.PriceRanges, _webHelper, _priceFormatter);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_webHelper, collection.PriceRanges);
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _workContext.WorkingCurrency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _workContext.WorkingCurrency);
            }


            //collection breadcrumb
            if (_catalogSettings.CategoryBreadcrumbEnabled)
            {
                model.DisplayCollectionBreadcrumb = true;

                string breadcrumbCacheKey = string.Format(ModelCacheEventConsumer.COLLECTION_BREADCRUMB_KEY,
                    collectionId,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                    _storeContext.CurrentStore.Id,
                    _workContext.WorkingLanguage.Id);
                model.CollectionBreadcrumb = _cacheManager.Get(breadcrumbCacheKey, () =>
                    collection
                    .GetCollectionBreadCrumb(_collectionService, _aclService, _storeMappingService)
                    .Select(catBr => new CollectionModel
                    {
                        Id = catBr.Id,
                        Name = catBr.GetLocalized(x => x.Name),
                        SeName = catBr.GetSeName()
                    })
                    .ToList()
                );
            }





            //subcollections
            string subCollectionsCacheKey = string.Format(ModelCacheEventConsumer.COLLECTION_SUBCOLLECTIONS_KEY,
                collectionId,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id,
                _webHelper.IsCurrentConnectionSecured());
            model.SubCollections = _cacheManager.Get(subCollectionsCacheKey, () =>
                _collectionService.GetAllCollectionsByParentCollectionId(collectionId)
                .Select(x =>
                {
                    var subCatModel = new CollectionModel.SubCollectionModel
                    {
                        Id = x.Id,
                        Name = x.GetLocalized(y => y.Name),
                        SeName = x.GetSeName(),
                        Description = x.GetLocalized(y => y.Description)
                    };

                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var collectionPictureCacheKey = string.Format(ModelCacheEventConsumer.COLLECTION_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    subCatModel.PictureModel = _cacheManager.Get(collectionPictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(x.PictureId);
                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Collection.ImageLinkTitleFormat"), subCatModel.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Collection.ImageAlternateTextFormat"), subCatModel.Name)
                        };
                        return pictureModel;
                    });

                    return subCatModel;
                })
                .ToList()
            );




            //featured products
            if (!_catalogSettings.IgnoreFeaturedProducts)
            {
                //We cache a value indicating whether we have featured products
                IPagedList<Product> featuredProducts = null;
                string cacheKey = string.Format(ModelCacheEventConsumer.COLLECTION_HAS_FEATURED_PRODUCTS_KEY, collectionId,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()), _storeContext.CurrentStore.Id);
                var hasFeaturedProductsCache = _cacheManager.Get<bool?>(cacheKey);
                if (!hasFeaturedProductsCache.HasValue)
                {
                    //no value in the cache yet
                    //let's load products and cache the result (true/false)
                    featuredProducts = _productService.SearchProducts(
                       //collectionIds: new List<int> { collection.Id },
                       storeId: _storeContext.CurrentStore.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                    hasFeaturedProductsCache = featuredProducts.TotalCount > 0;
                    _cacheManager.Set(cacheKey, hasFeaturedProductsCache, 60);
                }
                if (hasFeaturedProductsCache.Value && featuredProducts == null)
                {
                    //cache indicates that the collection has featured products
                    //let's load them
                    featuredProducts = _productService.SearchProducts(
                       //collectionIds: new List<int> { collection.Id },
                       storeId: _storeContext.CurrentStore.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                }
                if (featuredProducts != null)
                {
                    model.FeaturedProducts = PrepareProductOverviewModels(featuredProducts).ToList();
                }
            }


            var collectionIds = new List<int>();
            collectionIds.Add(collection.Id);
            if (_catalogSettings.ShowProductsFromSubcategories)
            {
                //include subcollections
                collectionIds.AddRange(GetChildCollectionIds(collection.Id));
            }
            //products
            IList<int> alreadyFilteredSpecOptionIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);
            IList<int> filterableSpecificationAttributeOptionIds;
            var products = _productService.DvSearchProducts(out filterableSpecificationAttributeOptionIds,
                true,
                collectionIds: collectionIds,
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                featuredProducts: true,//_catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
                priceMin: minPriceConverted,
                priceMax: maxPriceConverted,
                filteredSpecs: alreadyFilteredSpecOptionIds,
                orderBy: (ProductSortingEnum)command.OrderBy,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize);
            model.Products = PrepareProductOverviewModels(products).ToList();

            model.PagingFilteringContext.LoadPagedList(products);

            //specs
            model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
                filterableSpecificationAttributeOptionIds != null ? filterableSpecificationAttributeOptionIds.ToArray() : null,
                _specificationAttributeService,
                _webHelper,
                _workContext,
                _cacheManager);


            //template
            //var templateCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_TEMPLATE_MODEL_KEY, collection.CollectionTemplateId);
            //var templateViewPath = _cacheManager.Get(templateCacheKey, () =>
            //{
            //    var template = _categoryTemplateService.GetCategoryTemplateById(collection.CollectionTemplateId);
            //    if (template == null)
            //        template = _categoryTemplateService.GetAllCategoryTemplates().FirstOrDefault();
            //    if (template == null)
            //        throw new Exception("No default template could be loaded");
            //    return template.ViewPath;
            //});

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewCollection", _localizationService.GetResource("ActivityLog.PublicStore.ViewCollection"), collection.Name);

            return View("CollectionTemplate.ProductsInGridOrLines", model);
        }

        [ChildActionOnly]
        public ActionResult CollectionNavigation(int currentCollectionId, int currentProductId)
        {
            //get active collection
            int activeCollectionId = 0;
            if (currentCollectionId > 0)
            {
                //collection details page
                activeCollectionId = currentCollectionId;
            }
            else if (currentProductId > 0)
            {
                //product details page
                var productCollections = _collectionService.GetProductCollectionsByProductId(currentProductId);
                if (productCollections.Count > 0)
                    activeCollectionId = productCollections[0].CollectionId;
            }

            string cacheKey = string.Format(ModelCacheEventConsumer.COLLECTION_NAVIGATION_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var cachedModel = _cacheManager.Get(cacheKey, () => PrepareCollectionSimpleModels(0).ToList());

            var model = new CollectionNavigationModel
            {
                CurrentCollectionId = activeCollectionId,
                Collections = cachedModel
            };

            return PartialView(model);
        }


        /// <summary>
        /// Prepare collection (simple) models
        /// </summary>
        /// <param name="rootCollectionId">Root collection identifier</param>
        /// <param name="loadSubCollections">A value indicating whether subcollections should be loaded</param>
        /// <param name="allCollections">All available collections; pass null to load them internally</param>
        /// <returns>Collection models</returns>
        [NonAction]
        protected virtual IList<CollectionSimpleModel> PrepareCollectionSimpleModels(int rootCollectionId,
            bool loadSubCollections = true, IList<Collection> allCollections = null)
        {
            var result = new List<CollectionSimpleModel>();

            //little hack for performance optimization.
            //we know that this method is used to load top and left menu for collections.
            //it'll load all collections anyway.
            //so there's no need to invoke "GetAllCollectionsByParentCollectionId" multiple times (extra SQL commands) to load childs
            //so we load all collections at once
            //if you don't like this implementation if you can uncomment the line below (old behavior) and comment several next lines (before foreach)
            //var collections = _collectionService.GetAllCollectionsByParentCollectionId(rootCollectionId);
            if (allCollections == null)
            {
                //load collections if null passed
                //we implemeneted it this way for performance optimization - recursive iterations (below)
                //this way all collections are loaded only once
                allCollections = _collectionService.GetAllCollections();
            }
            var collections = allCollections.Where(c => c.ParentCollectionId == rootCollectionId).ToList();
            foreach (var collection in collections)
            {
                var collectionModel = new CollectionSimpleModel
                {
                    Id = collection.Id,
                    Name = collection.GetLocalized(x => x.Name),
                    SeName = collection.GetSeName(),
                    IncludeInTopMenu = collection.IncludeInTopMenu
                };

                //product number for each collection
                if (_catalogSettings.ShowCategoryProductNumber)
                {
                    string cacheKey = string.Format(ModelCacheEventConsumer.COLLECTION_NUMBER_OF_PRODUCTS_MODEL_KEY,
                        string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                        _storeContext.CurrentStore.Id,
                        collection.Id);
                    collectionModel.NumberOfProducts = _cacheManager.Get(cacheKey, () =>
                    {
                        var collectionIds = new List<int>();
                        collectionIds.Add(collection.Id);
                        //include subcollections
                        if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                            collectionIds.AddRange(GetChildCollectionIds(collection.Id));
                        return _productService.GetCollectionProductNumber(collectionIds, _storeContext.CurrentStore.Id);
                    });
                }

                if (loadSubCollections)
                {
                    var subCollections = PrepareCollectionSimpleModels(collection.Id, loadSubCollections, allCollections);
                    collectionModel.SubCollections.AddRange(subCollections);
                }
                result.Add(collectionModel);
            }

            return result;
        }

        public ActionResult LoadMoreProductCollection(int collectionId, CatalogPagingFilteringModel command)
        {
            var collection = _collectionService.GetCollectionById(collectionId);
            if (collection == null || collection.Deleted)
                return InvokeHttp404();

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a collection before publishing
            if (!collection.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(collection))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(collection))
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            var model = collection.ToModel();
            var collectionIds = new List<int>();
            collectionIds.Add(collection.Id);
            if (_catalogSettings.ShowProductsFromSubcategories)
            {
                //include subcollections
                collectionIds.AddRange(GetChildCollectionIds(collection.Id));
            }
            //products
            IList<int> alreadyFilteredSpecOptionIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);
            IList<int> filterableSpecificationAttributeOptionIds;
            var products = _productService.DvSearchProducts(out filterableSpecificationAttributeOptionIds,
                true,
                collectionIds: collectionIds,
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                featuredProducts: false,
                filteredSpecs: alreadyFilteredSpecOptionIds,
                orderBy: (ProductSortingEnum)command.OrderBy,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize
                );
            model.Products = PrepareProductOverviewModels(products).ToList();

            model.PagingFilteringContext.LoadPagedList(products);

            var result = new
            {
                success = true,
                html = RenderPartialViewToString("_LoadMoreProductCollection", model),
                totalPages = model.PagingFilteringContext.TotalPages
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [NonAction]
        protected virtual List<int> GetChildCollectionIds(int parentCategoryId)
        {
            string cacheKey = string.Format(ModelCacheEventConsumer.COLLECTION_CHILD_IDENTIFIERS_MODEL_KEY,
                parentCategoryId,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(cacheKey, () =>
            {
                var collectionsIds = new List<int>();
                var collections = _collectionService.GetAllCollectionsByParentCollectionId(parentCategoryId);
                foreach (var category in collections)
                {
                    collectionsIds.Add(category.Id);
                    collectionsIds.AddRange(GetChildCollectionIds(category.Id));
                }
                return collectionsIds;
            });
        }

        #endregion

        #region Attraction

        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult Attraction(int attractionId, CatalogPagingFilteringModel command)
        {
            var attraction = _attractionService.GetAttractionById(attractionId);
            if (attraction == null || attraction.Deleted)
                return InvokeHttp404();

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a attraction before publishing
            if (!attraction.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageAttractions))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(attraction))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(attraction))
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            var model = attraction.ToModel();




            //sorting
            PrepareSortingOptions(model.PagingFilteringContext, command);
            //view mode
            PrepareViewModes(model.PagingFilteringContext, command);
            //page size
            PreparePageSizeOptions(model.PagingFilteringContext, command,
                attraction.AllowCustomersToSelectPageSize,
                attraction.PageSizeOptions,
                attraction.PageSize);

            //price ranges
            model.PagingFilteringContext.PriceRangeFilter.LoadPriceRangeFilters(attraction.PriceRanges, _webHelper, _priceFormatter);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_webHelper, attraction.PriceRanges);
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _workContext.WorkingCurrency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _workContext.WorkingCurrency);
            }


            //attraction breadcrumb
            if (_catalogSettings.CategoryBreadcrumbEnabled)
            {
                model.DisplayAttractionBreadcrumb = true;

                string breadcrumbCacheKey = string.Format(ModelCacheEventConsumer.ATTRACTION_BREADCRUMB_KEY,
                    attractionId,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                    _storeContext.CurrentStore.Id,
                    _workContext.WorkingLanguage.Id);
                model.AttractionBreadcrumb = _cacheManager.Get(breadcrumbCacheKey, () =>
                    attraction
                    .GetAttractionBreadCrumb(_attractionService, _aclService, _storeMappingService)
                    .Select(catBr => new AttractionModel
                    {
                        Id = catBr.Id,
                        Name = catBr.GetLocalized(x => x.Name),
                        SeName = catBr.GetSeName()
                    })
                    .ToList()
                );
            }





            //subattractions
            string subAttractionsCacheKey = string.Format(ModelCacheEventConsumer.ATTRACTION_SUBATTRACTIONS_KEY,
                attractionId,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id,
                _webHelper.IsCurrentConnectionSecured());
            model.SubAttractions = _cacheManager.Get(subAttractionsCacheKey, () =>
                _attractionService.GetAllAttractionsByParentAttractionId(attractionId)
                .Select(x =>
                {
                    var subCatModel = new AttractionModel.SubAttractionModel
                    {
                        Id = x.Id,
                        Name = x.GetLocalized(y => y.Name),
                        SeName = x.GetSeName(),
                        Description = x.GetLocalized(y => y.Description)
                    };

                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var attractionPictureCacheKey = string.Format(ModelCacheEventConsumer.ATTRACTION_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    subCatModel.PictureModel = _cacheManager.Get(attractionPictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(x.PictureId);
                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Attraction.ImageLinkTitleFormat"), subCatModel.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Attraction.ImageAlternateTextFormat"), subCatModel.Name)
                        };
                        return pictureModel;
                    });

                    return subCatModel;
                })
                .ToList()
            );




            //featured products
            if (!_catalogSettings.IgnoreFeaturedProducts)
            {
                //We cache a value indicating whether we have featured products
                IPagedList<Product> featuredProducts = null;
                string cacheKey = string.Format(ModelCacheEventConsumer.ATTRACTION_HAS_FEATURED_PRODUCTS_KEY, attractionId,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()), _storeContext.CurrentStore.Id);
                var hasFeaturedProductsCache = _cacheManager.Get<bool?>(cacheKey);
                if (!hasFeaturedProductsCache.HasValue)
                {
                    //no value in the cache yet
                    //let's load products and cache the result (true/false)
                    featuredProducts = _productService.SearchProducts(
                       //attractionIds: new List<int> { attraction.Id },
                       storeId: _storeContext.CurrentStore.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                    hasFeaturedProductsCache = featuredProducts.TotalCount > 0;
                    _cacheManager.Set(cacheKey, hasFeaturedProductsCache, 60);
                }
                if (hasFeaturedProductsCache.Value && featuredProducts == null)
                {
                    //cache indicates that the attraction has featured products
                    //let's load them
                    featuredProducts = _productService.SearchProducts(
                       //attractionIds: new List<int> { attraction.Id },
                       storeId: _storeContext.CurrentStore.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                }
                if (featuredProducts != null)
                {
                    model.FeaturedProducts = PrepareProductOverviewModels(featuredProducts).ToList();
                }
            }


            var attractionIds = new List<int>();
            attractionIds.Add(attraction.Id);
            if (_catalogSettings.ShowProductsFromSubcategories)
            {
                //include subattractions
                attractionIds.AddRange(GetChildAttractionIds(attraction.Id));
            }
            //products
            IList<int> alreadyFilteredSpecOptionIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);
            IList<int> filterableSpecificationAttributeOptionIds;
            var products = _productService.DvSearchProducts(out filterableSpecificationAttributeOptionIds,
                true,
                attractionIds: attractionIds,
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                featuredProducts: true,// _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
                priceMin: minPriceConverted,
                priceMax: maxPriceConverted,
                filteredSpecs: alreadyFilteredSpecOptionIds,
                orderBy: (ProductSortingEnum)command.OrderBy,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize);
            model.Products = PrepareProductOverviewModels(products).ToList();

            model.PagingFilteringContext.LoadPagedList(products);

            //specs
            model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
                filterableSpecificationAttributeOptionIds != null ? filterableSpecificationAttributeOptionIds.ToArray() : null,
                _specificationAttributeService,
                _webHelper,
                _workContext,
                _cacheManager);


            //template
            //var templateCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_TEMPLATE_MODEL_KEY, attraction.AttractionTemplateId);
            //var templateViewPath = _cacheManager.Get(templateCacheKey, () =>
            //{
            //    var template = _categoryTemplateService.GetCategoryTemplateById(attraction.AttractionTemplateId);
            //    if (template == null)
            //        template = _categoryTemplateService.GetAllCategoryTemplates().FirstOrDefault();
            //    if (template == null)
            //        throw new Exception("No default template could be loaded");
            //    return template.ViewPath;
            //});

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewAttraction", _localizationService.GetResource("ActivityLog.PublicStore.ViewAttraction"), attraction.Name);

            return View("AttractionTemplate.ProductsInGridOrLines", model);
        }

        [ChildActionOnly]
        public ActionResult AttractionNavigation(int currentAttractionId, int currentProductId)
        {
            //get active attraction
            int activeAttractionId = 0;
            if (currentAttractionId > 0)
            {
                //attraction details page
                activeAttractionId = currentAttractionId;
            }
            else if (currentProductId > 0)
            {
                //product details page
                var productAttractions = _attractionService.GetProductAttractionsByProductId(currentProductId);
                if (productAttractions.Count > 0)
                    activeAttractionId = productAttractions[0].AttractionId;
            }

            string cacheKey = string.Format(ModelCacheEventConsumer.ATTRACTION_NAVIGATION_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var cachedModel = _cacheManager.Get(cacheKey, () => PrepareAttractionSimpleModels(0).ToList());

            var model = new AttractionNavigationModel
            {
                CurrentAttractionId = activeAttractionId,
                Attractions = cachedModel
            };

            return PartialView(model);
        }


        /// <summary>
        /// Prepare attraction (simple) models
        /// </summary>
        /// <param name="rootAttractionId">Root attraction identifier</param>
        /// <param name="loadSubAttractions">A value indicating whether subattractions should be loaded</param>
        /// <param name="allAttractions">All available attractions; pass null to load them internally</param>
        /// <returns>Attraction models</returns>
        [NonAction]
        protected virtual IList<AttractionSimpleModel> PrepareAttractionSimpleModels(int rootAttractionId,
            bool loadSubAttractions = true, IList<Attraction> allAttractions = null)
        {
            var result = new List<AttractionSimpleModel>();

            //little hack for performance optimization.
            //we know that this method is used to load top and left menu for attractions.
            //it'll load all attractions anyway.
            //so there's no need to invoke "GetAllAttractionsByParentAttractionId" multiple times (extra SQL commands) to load childs
            //so we load all attractions at once
            //if you don't like this implementation if you can uncomment the line below (old behavior) and comment several next lines (before foreach)
            //var attractions = _attractionService.GetAllAttractionsByParentAttractionId(rootAttractionId);
            if (allAttractions == null)
            {
                //load attractions if null passed
                //we implemeneted it this way for performance optimization - recursive iterations (below)
                //this way all attractions are loaded only once
                allAttractions = _attractionService.GetAllAttractions();
            }
            var attractions = allAttractions.Where(c => c.ParentAttractionId == rootAttractionId).ToList();
            foreach (var attraction in attractions)
            {
                var attractionModel = new AttractionSimpleModel
                {
                    Id = attraction.Id,
                    Name = attraction.GetLocalized(x => x.Name),
                    SeName = attraction.GetSeName(),
                    IncludeInTopMenu = attraction.IncludeInTopMenu
                };

                //product number for each attraction
                if (_catalogSettings.ShowCategoryProductNumber)
                {
                    string cacheKey = string.Format(ModelCacheEventConsumer.ATTRACTION_NUMBER_OF_PRODUCTS_MODEL_KEY,
                        string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                        _storeContext.CurrentStore.Id,
                        attraction.Id);
                    attractionModel.NumberOfProducts = _cacheManager.Get(cacheKey, () =>
                    {
                        var attractionIds = new List<int>();
                        attractionIds.Add(attraction.Id);
                        //include subattractions
                        if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                            attractionIds.AddRange(GetChildAttractionIds(attraction.Id));
                        return _productService.GetAttractionProductNumber(attractionIds, _storeContext.CurrentStore.Id);
                    });
                }

                if (loadSubAttractions)
                {
                    var subAttractions = PrepareAttractionSimpleModels(attraction.Id, loadSubAttractions, allAttractions);
                    attractionModel.SubAttractions.AddRange(subAttractions);
                }
                result.Add(attractionModel);
            }

            return result;
        }

        [NonAction]
        protected virtual List<int> GetChildAttractionIds(int parentCategoryId)
        {
            string cacheKey = string.Format(ModelCacheEventConsumer.ATTRACTION_CHILD_IDENTIFIERS_MODEL_KEY,
                parentCategoryId,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(cacheKey, () =>
            {
                var attractionsIds = new List<int>();
                var attractions = _attractionService.GetAllAttractionsByParentAttractionId(parentCategoryId);
                foreach (var category in attractions)
                {
                    attractionsIds.Add(category.Id);
                    attractionsIds.AddRange(GetChildAttractionIds(category.Id));
                }
                return attractionsIds;
            });
        }

        /// <summary>
        /// Prepare category (simple) models
        /// </summary>
        /// <param name="rootCategoryId">Root category identifier</param>
        /// <param name="loadSubCategories">A value indicating whether subcategories should be loaded</param>
        /// <param name="allCategories">All available categories; pass null to load them internally</param>
        /// <returns>Category models</returns>
        [NonAction]
        protected virtual IList<CategorySimpleModel> PrepareCategorySimpleModels(int rootCategoryId,
            bool loadSubCategories = true, IList<Category> allCategories = null, int categoryType = 0)
        {
            var result = new List<CategorySimpleModel>();

            //little hack for performance optimization.
            //we know that this method is used to load top and left menu for categories.
            //it'll load all categories anyway.
            //so there's no need to invoke "GetAllCategoriesByParentCategoryId" multiple times (extra SQL commands) to load childs
            //so we load all categories at once
            //if you don't like this implementation if you can uncomment the line below (old behavior) and comment several next lines (before foreach)
            //var categories = _categoryService.GetAllCategoriesByParentCategoryId(rootCategoryId);
            if (allCategories == null)
            {
                //load categories if null passed
                //we implemeneted it this way for performance optimization - recursive iterations (below)
                //this way all categories are loaded only once
                allCategories = _categoryService.GetAllCategories(categoryTypeId: categoryType);
            }
            var categories = allCategories.Where(c => c.ParentCategoryId == rootCategoryId).ToList();
            foreach (var category in categories)
            {
                var categoryModel = new CategorySimpleModel
                {
                    Id = category.Id,
                    Name = category.GetLocalized(x => x.Name),
                    SeName = category.GetSeName(),
                    IncludeInTopMenu = category.IncludeInTopMenu,
                    DisplayedOnHomePage = category.ShowOnHomePage,
                    IsNewCategory = category.IsNew,
                    ParentCategoryId = category.ParentCategoryId
                };

                //product number for each category
                if (_catalogSettings.ShowCategoryProductNumber)
                {
                    string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY,
                        string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                        _storeContext.CurrentStore.Id,
                        category.Id);
                    categoryModel.NumberOfProducts = _cacheManager.Get(cacheKey, () =>
                    {
                        var categoryIds = new List<int>();
                        categoryIds.Add(category.Id);
                        //include subcategories
                        if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                            categoryIds.AddRange(GetChildCategoryIds(category.Id));
                        return _productService.GetCategoryProductNumber(categoryIds, _storeContext.CurrentStore.Id);
                    });
                }

                if (loadSubCategories)
                {
                    var subCategories = PrepareCategorySimpleModels(category.Id, loadSubCategories, allCategories);
                    categoryModel.SubCategories.AddRange(subCategories);
                }
                result.Add(categoryModel);
            }

            return result;
        }

        [ChildActionOnly]
        public ActionResult SearchHompageCategories()
        {
            //categories
            string categoryCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_MENU_MODEL_KEY,
                 _workContext.WorkingLanguage.Id,
                 string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                 _storeContext.CurrentStore.Id);
            var cachedCategoriesModel = _cacheManager.Get(categoryCacheKey, () => PrepareCategorySimpleModels(0));

            string topicCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_TOP_MENU_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()));
            var cachedTopicModel = _cacheManager.Get(topicCacheKey, () =>
                _topicService.GetAllTopics(_storeContext.CurrentStore.Id)
                .Where(t => t.IncludeInTopMenu)
                .Select(t => new TopMenuModel.TopMenuTopicModel
                {
                    Id = t.Id,
                    Name = t.GetLocalized(x => x.Title),
                    SeName = t.GetSeName()
                })
                .ToList()
            );
            var model = new TopMenuModel
            {
                Categories = cachedCategoriesModel,
                Topics = cachedTopicModel,
                NewProductsEnabled = _catalogSettings.NewProductsEnabled,
                BlogEnabled = _blogSettings.Enabled,
                ForumEnabled = _forumSettings.ForumsEnabled
            };
            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult DvHomepageDestinations()
        {
            string categoriesCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NEW_HOMEPAGE_KEY,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id,
                _webHelper.IsCurrentConnectionSecured(), (int)CategoryType.Destination);

            var model = _cacheManager.Get(categoriesCacheKey, () =>
                _categoryService.GetAllCategoriesDisplayedOnHomePage(categoryTypeId: (int)CategoryType.Destination)
                .Select(x =>
                {
                    var catModel = x.ToModel();

                    string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY,
                        string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                        _storeContext.CurrentStore.Id,
                        x.Id);

                    catModel.NumberOfProducts = _cacheManager.Get(cacheKey, () =>
                    {
                        var categoryIds = new List<int>();
                        categoryIds.Add(x.Id);
                        //include subcategories
                        if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                            categoryIds.AddRange(GetChildCategoryIds(x.Id));
                        return _productService.GetCategoryProductNumber(categoryIds, _storeContext.CurrentStore.Id);
                    });

                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(x.PictureId);
                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                        };
                        return pictureModel;
                    });

                    return catModel;
                })
                .ToList()
            );

            if (model.Count == 0)
                return Content("");

            return PartialView(model);
        }


        [ChildActionOnly]
        public ActionResult DvHomepageAttractions()
        {
            string categoriesCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NEW_HOMEPAGE_KEY,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id,
                _webHelper.IsCurrentConnectionSecured(), (int)CategoryType.Attraction);

            var model = _cacheManager.Get(categoriesCacheKey, () =>
                _categoryService.GetAllCategoriesDisplayedOnHomePage(categoryTypeId: (int)CategoryType.Attraction)
                .Select(x =>
                {
                    var catModel = x.ToModel();

                    string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY,
                        string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                        _storeContext.CurrentStore.Id,
                        x.Id);

                    catModel.NumberOfProducts = _cacheManager.Get(cacheKey, () =>
                    {
                        var categoryIds = new List<int>();
                        categoryIds.Add(x.Id);
                        //include subcategories
                        if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                            categoryIds.AddRange(GetChildCategoryIds(x.Id));
                        return _productService.GetCategoryProductNumber(categoryIds, _storeContext.CurrentStore.Id);
                    });

                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(x.PictureId);
                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                        };
                        return pictureModel;
                    });

                    return catModel;
                })
                .ToList()
            );

            if (model.Count == 0)
                return Content("");

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult DvHomepageCollections()
        {
            string categoriesCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NEW_HOMEPAGE_KEY,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id,
                _webHelper.IsCurrentConnectionSecured(), (int)CategoryType.Collection);

            var model = _cacheManager.Get(categoriesCacheKey, () =>
                _categoryService.GetAllCategoriesDisplayedOnHomePage(categoryTypeId: (int)CategoryType.Collection)
                .Select(x =>
                {
                    var catModel = x.ToModel();

                    string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY,
                        string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                        _storeContext.CurrentStore.Id,
                        x.Id);

                    catModel.NumberOfProducts = _cacheManager.Get(cacheKey, () =>
                    {
                        var categoryIds = new List<int>();
                        categoryIds.Add(x.Id);
                        //include subcategories
                        if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                            categoryIds.AddRange(GetChildCategoryIds(x.Id));
                        return _productService.GetCategoryProductNumber(categoryIds, _storeContext.CurrentStore.Id);
                    });

                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(x.PictureId);
                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                        };
                        return pictureModel;
                    });

                    return catModel;
                })
                .ToList()
            );

            if (model.Count == 0)
                return Content("");

            return PartialView(model);
        }

        public ActionResult LoadMoreProductAttraction(int attractionId, CatalogPagingFilteringModel command)
        {
            var attraction = _attractionService.GetAttractionById(attractionId);
            if (attraction == null || attraction.Deleted)
                return InvokeHttp404();

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a attraction before publishing
            if (!attraction.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageAttractions))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(attraction))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(attraction))
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            var model = attraction.ToModel();
            var attractionIds = new List<int>();
            attractionIds.Add(attraction.Id);
            if (_catalogSettings.ShowProductsFromSubcategories)
            {
                //include subattractions
                attractionIds.AddRange(GetChildAttractionIds(attraction.Id));
            }
            //products
            IList<int> alreadyFilteredSpecOptionIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);
            IList<int> filterableSpecificationAttributeOptionIds;
            var products = _productService.DvSearchProducts(out filterableSpecificationAttributeOptionIds,
                true,
                attractionIds: attractionIds,
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                featuredProducts: false,
                filteredSpecs: alreadyFilteredSpecOptionIds,
                orderBy: (ProductSortingEnum)command.OrderBy,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize
                );
            model.Products = PrepareProductOverviewModels(products).ToList();

            model.PagingFilteringContext.LoadPagedList(products);

            var result = new
            {
                success = true,
                html = RenderPartialViewToString("_LoadMoreProductAttraction", model),
                totalPages = model.PagingFilteringContext.TotalPages
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Banners
        public ActionResult HomePageBanners()
        {
            var banners = _bannerService.GetAllBanners().Select(b => {
                var model = new BannerModel() {
                    Name = b.Name,
                    Url = b.Url
                };

                int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                var picture = _pictureService.GetPictureById(b.PictureId);
                var pictureModel = new PictureModel
                {
                    FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                    ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                    Title = b.Name,
                    AlternateText = b.Name
                };

                model.PictureModel = pictureModel;

                return model;

            }).ToList();

            return View(banners);
        }

        #endregion

        #region Searching

        [NopHttpsRequirement(SslRequirement.No)]
        [ValidateInput(false)]
        public ActionResult Search(SearchModel model, CatalogPagingFilteringModel command)
        {
            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            if (model == null)
                model = new SearchModel();

            var searchTerms = model.q;
            if (searchTerms == null)
                searchTerms = "";
            searchTerms = searchTerms.Trim();



            //sorting
            PrepareSortingOptions(model.PagingFilteringContext, command);
            //view mode
            PrepareViewModes(model.PagingFilteringContext, command);
            //page size
            PreparePageSizeOptions(model.PagingFilteringContext, command,
                _catalogSettings.SearchPageAllowCustomersToSelectPageSize,
                _catalogSettings.SearchPagePageSizeOptions,
                _catalogSettings.SearchPageProductsPerPage);

            string SEARCH_CATEGORIES_MODEL_KEY = "Nop.pres.search.categories-{0}-{1}-{2}-{3}";
            string cacheKeyDestination = string.Format(SEARCH_CATEGORIES_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id, (int)CategoryType.Destination);
            var destinations = _cacheManager.Get(cacheKeyDestination, () => {
                var result = new List<CategorySimpleModel>();

                var allCategories = _categoryService.GetAllCategories(categoryTypeId: (int)CategoryType.Destination);
                
                foreach (var category in allCategories)
                {
                    var categoryModel = new CategorySimpleModel
                    {
                        Id = category.Id,
                        Name = category.GetLocalized(x => x.Name),
                        SeName = category.GetSeName(),
                        IncludeInTopMenu = category.IncludeInTopMenu,
                        DisplayedOnHomePage = category.ShowOnHomePage,
                        IsNewCategory = category.IsNew,
                        ParentCategoryId = category.ParentCategoryId
                    };
                    
                    result.Add(categoryModel);
                }

                return result;

            });

            if (destinations.Count > 0)
            {
                if (model.cityId == 0)
                {
                    if (model.countryId == 0)
                    {
                        var country = destinations.FirstOrDefault(d => d.ParentCategoryId == 0);
                        if (country != null)
                        {
                            model.countryId = country.Id;


                        }
                    }

                    var city = destinations.FirstOrDefault(d => d.ParentCategoryId == model.countryId);
                    if (city != null)
                        model.cityId = city.Id;
                }
                else
                {
                    var city = _categoryService.GetCategoryById(model.cityId);
                    if(city != null)
                        model.countryId = city.ParentCategoryId;
                }

                

                


                //Countries
                var countries = destinations.Where(d => d.ParentCategoryId == 0).ToList();
                //first empty entry
                model.AvailableCountries.Add(new SelectListItem
                {
                    Value = "0",
                    Text = _localizationService.GetResource("Common.All")
                });
                //all other categories
                foreach (var c in countries)
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name,
                        Selected = model.countryId == c.Id
                    });
                }

                var cities = destinations.Where(d => d.ParentCategoryId == model.countryId).ToList();
                //first empty entry
                model.AvailableCities.Add(new SelectListItem
                {
                    Value = "0",
                    Text = _localizationService.GetResource("Common.All")
                });
                //cities
                foreach (var c in cities)
                {
                    model.AvailableCities.Add(new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name,
                        Selected = model.cityId == c.Id
                    });
                }
            }

            string cacheKeyCollections = string.Format(SEARCH_CATEGORIES_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id, (int)CategoryType.Collection);
            var collections = _cacheManager.Get(cacheKeyCollections, () => GetSearchCategories(CategoryType.Collection));

            model.AvailableCollections = collections.ToList();


            string cacheKeyCategories = string.Format(SEARCH_CATEGORIES_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id, (int)CategoryType.Category);
            var categories = _cacheManager.Get(cacheKeyCategories, () => GetSearchCategories(CategoryType.Category));

            model.AvailableCategories = categories.ToList();

            IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
            // only search if query string search keyword is set (used to avoid searching or displaying search term min length error message on /search page load)
            if (Request.Params["q"] != null)
            {
                if (searchTerms.Length < _catalogSettings.ProductSearchTermMinimumLength)
                {
                    model.Warning = string.Format(_localizationService.GetResource("Search.SearchTermMinimumLengthIsNCharacters"), _catalogSettings.ProductSearchTermMinimumLength);
                }
                else
                {
                    
                }
            }
            var categoryIds = new List<int>();
            int manufacturerId = 0;
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            bool searchInDescriptions = false;
            
            //advanced search
            var categoryId = model.cityId != 0 ? model.cityId : model.countryId;
            if (categoryId > 0)
            {
                categoryIds.Add(categoryId);
                categoryIds.AddRange(GetChildCategoryIds(categoryId));

                var category = _categoryService.GetCategoryById(categoryId);
                if (category != null)
                    model.CityOrCountryName = category.GetLocalized(c => c.Name);
            }


            searchInDescriptions = model.sid;

            //var searchInProductTags = false;
            var searchInProductTags = searchInDescriptions;

            //products
            IList<int> alreadyFilteredSpecOptionIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);
            IList<int> filterableSpecificationAttributeOptionIds;
            var searchproducts = _productService.SearchProducts(out filterableSpecificationAttributeOptionIds,
                true,
                        categoryIds: categoryIds,
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                priceMin: minPriceConverted,
                priceMax: maxPriceConverted,
                keywords: searchTerms,
                searchDescriptions: searchInDescriptions,
                searchSku: searchInDescriptions,
                searchProductTags: searchInProductTags,
                productType: ProductType.GroupedProduct,
                filteredSpecs: alreadyFilteredSpecOptionIds,
                languageId: _workContext.WorkingLanguage.Id,
                orderBy: (ProductSortingEnum)command.OrderBy
                ).AsEnumerable();

            // search collections
            searchproducts = searchproducts.Where(p => {
                var b = true;

                var collectionIds = p.ProductCategories.Where(pc => pc.Category.CategoryType == CategoryType.Collection).Select(pc => pc.CategoryId);

                var cateIds = p.ProductCategories.Where(pc => pc.Category.CategoryType == CategoryType.Category).Select(pc => pc.CategoryId);
                var attractionIds = p.ProductCategories.Where(pc => pc.Category.CategoryType == CategoryType.Attraction).Select(pc => pc.CategoryId);


                if (model.collectionId > 0)
                    if (!collectionIds.Contains(model.collectionId))
                        return false;

                if (model.categoryId > 0)
                    if (!cateIds.Contains(model.categoryId))
                        return false;

                if (model.attractionId > 0)
                    if (!attractionIds.Contains(model.attractionId))
                        return false;
                return b;
            });

            if (model.collectionId > 0)
            {
                var collection = _categoryService.GetCategoryById(model.collectionId);
                if (collection != null)
                    model.CollectionName = collection.GetLocalized(c => c.Name);
            }

            if (model.categoryId > 0)
            {
                var category = _categoryService.GetCategoryById(model.categoryId);
                if (category != null)
                    model.CategoryName = category.GetLocalized(c => c.Name);
            }

            if (model.attractionId > 0)
            {
                var attraction = _categoryService.GetCategoryById(model.attractionId);
                if (attraction != null)
                    model.AttractionName = attraction.GetLocalized(c => c.Name);
            }

            //specs
            model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
                filterableSpecificationAttributeOptionIds != null ? filterableSpecificationAttributeOptionIds.ToArray() : null,
                _specificationAttributeService,
                _webHelper,
                _workContext,
                _cacheManager);

            model.SpecificationAttributes = _specificationAttributeService.GetProductSpecificationAttributes(allowFiltering: true)
                .Select(ps => ps.SpecificationAttributeOption.SpecificationAttribute)
                .Distinct().OrderBy(spec => spec.DisplayOrder).ToList();

            model.AlreadyFilteredSpecOptionIds = alreadyFilteredSpecOptionIds.ToList();

            products = new PagedList<Product>(searchproducts.ToList(), pageIndex: command.PageNumber - 1, pageSize: command.PageSize);


            model.Products = PrepareProductOverviewModels(products).ToList();

            var attractions = new List<Category>();
            foreach (var p in products)
            {
                var listAttraction = _categoryService.GetProductCategoriesByProductIdAndCategoryTypeId(p.Id, (int)CategoryType.Attraction).Select(pc => pc.Category);
                attractions.AddRange(listAttraction);
            }

            model.AvailableAttractions = PrepareCategorySimpleModels(rootCategoryId: 0, allCategories: attractions).ToList();


            model.NoResults = !model.Products.Any();

            //search term statistics
            if (!String.IsNullOrEmpty(searchTerms))
            {
                var searchTerm = _searchTermService.GetSearchTermByKeyword(searchTerms, _storeContext.CurrentStore.Id);
                if (searchTerm != null)
                {
                    searchTerm.Count++;
                    _searchTermService.UpdateSearchTerm(searchTerm);
                }
                else
                {
                    searchTerm = new SearchTerm
                    {
                        Keyword = searchTerms,
                        StoreId = _storeContext.CurrentStore.Id,
                        Count = 1
                    };
                    _searchTermService.InsertSearchTerm(searchTerm);
                }
            }

            //event
            _eventPublisher.Publish(new ProductSearchEvent
            {
                SearchTerm = searchTerms,
                SearchInDescriptions = searchInDescriptions,
                CategoryIds = categoryIds,
                ManufacturerId = manufacturerId,
                WorkingLanguageId = _workContext.WorkingLanguage.Id
            });
            model.PagingFilteringContext.LoadPagedList(products);

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult SearchBox()
        {
            var model = new SearchBoxModel
            {
                AutoCompleteEnabled = _catalogSettings.ProductSearchAutoCompleteEnabled,
                ShowProductImagesInSearchAutoComplete = _catalogSettings.ShowProductImagesInSearchAutoComplete,
                SearchTermMinimumLength = _catalogSettings.ProductSearchTermMinimumLength
            };
            return PartialView(model);
        }

        public ActionResult SearchTermAutoComplete(string term)
        {
            if (String.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return Content("");

            //products
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                keywords: term,
                searchSku: false,
                languageId: _workContext.WorkingLanguage.Id,
                visibleIndividuallyOnly: true,
                pageSize: productNumber);

            var models = PrepareProductOverviewModels(products, false, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize).ToList();
            var result = (from p in models
                          select new
                          {
                              label = p.Name,
                              producturl = Url.RouteUrl("Product", new { SeName = p.SeName }),
                              productpictureurl = p.DefaultPictureModel.ImageUrl
                          })
                          .ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [NonAction]
        protected List<CategorySimpleModel> GetSearchCategories(CategoryType type)
        {
            var categoriesModel = new List<CategorySimpleModel>();
            //all categories
            //var allCategories = _categoryService.GetAllCategories(categoryTypeId: (int)type);

            return PrepareCategorySimpleModels(rootCategoryId: 0, categoryType: (int)type).ToList();
        }
        #endregion
    }
}