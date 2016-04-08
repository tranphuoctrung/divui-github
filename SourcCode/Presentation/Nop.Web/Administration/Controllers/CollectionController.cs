using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Models.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Controllers
{
    public partial class CollectionController : BaseAdminController
    {
        #region Fields

        private readonly ICollectionService _collectionService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IDiscountService _discountService;
        private readonly IPermissionService _permissionService;
        private readonly IAclService _aclService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IExportManager _exportManager;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IVendorService _vendorService;
        private readonly CatalogSettings _catalogSettings;

        #endregion
        
        #region Constructors

        public CollectionController(ICollectionService collectionService, ICategoryTemplateService categoryTemplateService,
            IManufacturerService manufacturerService, IProductService productService, 
            ICustomerService customerService,
            IUrlRecordService urlRecordService, 
            IPictureService pictureService, 
            ILanguageService languageService,
            ILocalizationService localizationService, 
            ILocalizedEntityService localizedEntityService,
            IDiscountService discountService,
            IPermissionService permissionService,
            IAclService aclService, 
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IExportManager exportManager, 
            IVendorService vendorService, 
            ICustomerActivityService customerActivityService,
            CatalogSettings catalogSettings)
        {
            this._collectionService = collectionService;
            this._categoryTemplateService = categoryTemplateService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._customerService = customerService;
            this._urlRecordService = urlRecordService;
            this._pictureService = pictureService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._discountService = discountService;
            this._permissionService = permissionService;
            this._vendorService = vendorService;
            this._aclService = aclService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._exportManager = exportManager;
            this._customerActivityService = customerActivityService;
            this._catalogSettings = catalogSettings;
        }

        #endregion
        
        #region Utilities

        [NonAction]
        protected virtual void UpdateLocales(Collection collection, CollectionModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(collection,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(collection,
                                                           x => x.Description,
                                                           localized.Description,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(collection,
                                                           x => x.MetaKeywords,
                                                           localized.MetaKeywords,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(collection,
                                                           x => x.MetaDescription,
                                                           localized.MetaDescription,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(collection,
                                                           x => x.MetaTitle,
                                                           localized.MetaTitle,
                                                           localized.LanguageId);

                //search engine name
                var seName = collection.ValidateSeName(localized.SeName, localized.Name, false);
                _urlRecordService.SaveSlug(collection, seName, localized.LanguageId);
            }
        }

        [NonAction]
        protected virtual void UpdatePictureSeoNames(Collection collection)
        {
            var picture = _pictureService.GetPictureById(collection.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(collection.Name));
        }

        [NonAction]
        protected virtual void PrepareAllCollectionsModel(CollectionModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCollections.Add(new SelectListItem
            {
                Text = "[None]",
                Value = "0"
            });
            var collections = _collectionService.GetAllCollections(showHidden: true);
            foreach (var c in collections)
            {
                model.AvailableCollections.Add(new SelectListItem
                {
                    Text = c.GetFormattedBreadCrumb(collections),
                    Value = c.Id.ToString()
                });
            }
        }

        [NonAction]
        protected virtual void PrepareTemplatesModel(CollectionModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var templates = _categoryTemplateService.GetAllCategoryTemplates();
            foreach (var template in templates)
            {
                model.AvailableCollectionTemplates.Add(new SelectListItem
                {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }
        }

        [NonAction]
        protected virtual void PrepareDiscountModel(CollectionModel model, Collection collection, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableDiscounts = _discountService
                .GetAllDiscounts(DiscountType.AssignedToCollections, showHidden: true)
                .Select(d => d.ToModel())
                .ToList();

            if (!excludeProperties && collection != null)
            {
                model.SelectedDiscountIds = collection.AppliedDiscounts.Select(d => d.Id).ToArray();
            }
        }

        [NonAction]
        protected virtual void PrepareAclModel(CollectionModel model, Collection collection, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (collection != null)
                {
                    model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccess(collection);
                }
            }
        }

        [NonAction]
        protected virtual void SaveCollectionAcl(Collection collection, CollectionModel model)
        {
            var existingAclRecords = _aclService.GetAclRecords(collection);
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        _aclService.InsertAclRecord(collection, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        _aclService.DeleteAclRecord(aclRecordToDelete);
                }
            }
        }

        [NonAction]
        protected virtual void PrepareStoresMappingModel(CollectionModel model, Collection collection, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (collection != null)
                {
                    model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(collection);
                }
            }
        }

        [NonAction]
        protected virtual void SaveStoreMappings(Collection collection, CollectionModel model)
        {
            var existingStoreMappings = _storeMappingService.GetStoreMappings(collection);
            var allStores = _storeService.GetAllStores();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds != null && model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        _storeMappingService.InsertStoreMapping(collection, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        _storeMappingService.DeleteStoreMapping(storeMappingToDelete);
                }
            }
        }

        #endregion
        
        #region List / tree

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();

            var model = new CollectionListModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, CollectionListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();

            var collections = _collectionService.GetAllCollections(model.SearchCollectionName, 
                command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = collections.Select(x =>
                {
                    var collectionModel = x.ToModel();
                    collectionModel.Breadcrumb = x.GetFormattedBreadCrumb(_collectionService);
                    return collectionModel;
                }),
                Total = collections.TotalCount
            };
            return Json(gridModel);
        }
        
        public ActionResult Tree()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();

            return View();
        }

        [HttpPost,]
        public ActionResult TreeLoadChildren(int id = 0)
        {
            var collections = _collectionService.GetAllCollectionsByParentCollectionId(id, true)
                .Select(x => new
                             {
                                 id = x.Id,
                                 Name = x.Name,
                                 hasChildren = _collectionService.GetAllCollectionsByParentCollectionId(x.Id, true).Count > 0,
                                 imageUrl = Url.Content("~/Administration/Content/images/ico-content.png")
                             });

            return Json(collections);
        }

        #endregion

        #region Create / Edit / Delete

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();

            var model = new CollectionModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //templates
            PrepareTemplatesModel(model);
            //collections
            PrepareAllCollectionsModel(model);
            //discounts
            PrepareDiscountModel(model, null, true);
            //ACL
            PrepareAclModel(model, null, false);
            //Stores
            PrepareStoresMappingModel(model, null, false);
            //default values
            model.PageSize = _catalogSettings.DefaultCategoryPageSize;
            model.PageSizeOptions = _catalogSettings.DefaultCategoryPageSizeOptions;
            model.Published = true;
            model.IncludeInTopMenu = true;
            model.AllowCustomersToSelectPageSize = true;            

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(CollectionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var collection = model.ToEntity();
                collection.CreatedOnUtc = DateTime.UtcNow;
                collection.UpdatedOnUtc = DateTime.UtcNow;
                _collectionService.InsertCollection(collection);
                //search engine name
                model.SeName = collection.ValidateSeName(model.SeName, collection.Name, true);
                _urlRecordService.SaveSlug(collection, model.SeName, 0);
                //locales
                UpdateLocales(collection, model);
                //discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCollections, showHidden: true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                        collection.AppliedDiscounts.Add(discount);
                }
                _collectionService.UpdateCollection(collection);
                //update picture seo file name
                UpdatePictureSeoNames(collection);
                //ACL (customer roles)
                SaveCollectionAcl(collection, model);
                //Stores
                SaveStoreMappings(collection, model);

                //activity log
                _customerActivityService.InsertActivity("AddNewCollection", _localizationService.GetResource("ActivityLog.AddNewCollection"), collection.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Collections.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = collection.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //templates
            PrepareTemplatesModel(model);
            //collections
            PrepareAllCollectionsModel(model);
            //discounts
            PrepareDiscountModel(model, null, true);
            //ACL
            PrepareAclModel(model, null, true);
            //Stores
            PrepareStoresMappingModel(model, null, true);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();

            var collection = _collectionService.GetCollectionById(id);
            if (collection == null || collection.Deleted) 
                //No collection found with the specified id
                return RedirectToAction("List");

            var model = collection.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = collection.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = collection.GetLocalized(x => x.Description, languageId, false, false);
                locale.MetaKeywords = collection.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = collection.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = collection.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = collection.GetSeName(languageId, false, false);
            });
            //templates
            PrepareTemplatesModel(model);
            //collections
            PrepareAllCollectionsModel(model);
            //discounts
            PrepareDiscountModel(model, collection, false);
            //ACL
            PrepareAclModel(model, collection, false);
            //Stores
            PrepareStoresMappingModel(model, collection, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(CollectionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();

            var collection = _collectionService.GetCollectionById(model.Id);
            if (collection == null || collection.Deleted)
                //No collection found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                int prevPictureId = collection.PictureId;
                collection = model.ToEntity(collection);
                collection.UpdatedOnUtc = DateTime.UtcNow;
                _collectionService.UpdateCollection(collection);
                //search engine name
                model.SeName = collection.ValidateSeName(model.SeName, collection.Name, true);
                _urlRecordService.SaveSlug(collection, model.SeName, 0);
                //locales
                UpdateLocales(collection, model);
                //discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCollections, showHidden: true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    {
                        //new discount
                        if (collection.AppliedDiscounts.Count(d => d.Id == discount.Id) == 0)
                            collection.AppliedDiscounts.Add(discount);
                    }
                    else
                    {
                        //remove discount
                        if (collection.AppliedDiscounts.Count(d => d.Id == discount.Id) > 0)
                            collection.AppliedDiscounts.Remove(discount);
                    }
                }
                _collectionService.UpdateCollection(collection);
                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != collection.PictureId)
                {
                    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }
                //update picture seo file name
                UpdatePictureSeoNames(collection);
                //ACL
                SaveCollectionAcl(collection, model);
                //Stores
                SaveStoreMappings(collection, model);

                //activity log
                _customerActivityService.InsertActivity("EditCollection", _localizationService.GetResource("ActivityLog.EditCollection"), collection.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Collections.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = collection.Id});
                }
                return RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            //templates
            PrepareTemplatesModel(model);
            //collections
            PrepareAllCollectionsModel(model);
            //discounts
            PrepareDiscountModel(model, collection, true);
            //ACL
            PrepareAclModel(model, collection, true);
            //Stores
            PrepareStoresMappingModel(model, collection, true);

            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();

            var collection = _collectionService.GetCollectionById(id);
            if (collection == null)
                //No collection found with the specified id
                return RedirectToAction("List");

            _collectionService.DeleteCollection(collection);

            //activity log
            _customerActivityService.InsertActivity("DeleteCollection", _localizationService.GetResource("ActivityLog.DeleteCollection"), collection.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Collections.Deleted"));
            return RedirectToAction("List");
        }
        

        #endregion

        #region Export / Import

        public ActionResult ExportXml()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();

            try
            {
                var xml = _exportManager.ExportCollectionsToXml();
                return new XmlDownloadResult(xml, "collections.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        #endregion

        #region Products

        [HttpPost]
        public ActionResult ProductList(DataSourceRequest command, int collectionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();

            var productCollections = _collectionService.GetProductCollectionsByCollectionId(collectionId,
                command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = productCollections.Select(x => new CollectionModel.CollectionProductModel
                {
                    Id = x.Id,
                    CollectionId = x.CollectionId,
                    ProductId = x.ProductId,
                    ProductName = _productService.GetProductById(x.ProductId).Name,
                    IsFeaturedProduct = x.IsFeaturedProduct,
                    DisplayOrder = x.DisplayOrder
                }),
                Total = productCollections.TotalCount
            };

            return Json(gridModel);
        }

        public ActionResult ProductUpdate(CollectionModel.CollectionProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();

            var productCollection = _collectionService.GetProductCollectionById(model.Id);
            if (productCollection == null)
                throw new ArgumentException("No product collection mapping found with the specified id");

            productCollection.IsFeaturedProduct = model.IsFeaturedProduct;
            productCollection.DisplayOrder = model.DisplayOrder;
            _collectionService.UpdateProductCollection(productCollection);

            return new NullJsonResult();
        }

        public ActionResult ProductDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();

            var productCollection = _collectionService.GetProductCollectionById(id);
            if (productCollection == null)
                throw new ArgumentException("No product collection mapping found with the specified id");

            //var collectionId = productCollection.CollectionId;
            _collectionService.DeleteProductCollection(productCollection);

            return new NullJsonResult();
        }

        public ActionResult ProductAddPopup(int collectionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();
            
            var model = new CollectionModel.AddCollectionProductModel();
            //collections
            model.AvailableCollections.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var collections = _collectionService.GetAllCollections(showHidden: true);
            foreach (var c in collections)
                model.AvailableCollections.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(collections), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult ProductAddPopupList(DataSourceRequest command, CollectionModel.AddCollectionProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();

            var gridModel = new DataSourceResult();
            var products = _productService.SearchProducts(
                //collectionIds: new List<int> { model.SearchCollectionId },
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
                );
            gridModel.Data = products.Select(x => x.ToModel());
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }
        
        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult ProductAddPopup(string btnId, string formId, CollectionModel.AddCollectionProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCollections))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (int id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        var existingProductCollections = _collectionService.GetProductCollectionsByCollectionId(model.CollectionId, showHidden: true);
                        if (existingProductCollections.FindProductCollection(id, model.CollectionId) == null)
                        {
                            _collectionService.InsertProductCollection(
                                new ProductCollection
                                {
                                    CollectionId = model.CollectionId,
                                    ProductId = id,
                                    IsFeaturedProduct = false,
                                    DisplayOrder = 1
                                });
                        }
                    }
                }
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View(model);
        }

        #endregion
    }
}
