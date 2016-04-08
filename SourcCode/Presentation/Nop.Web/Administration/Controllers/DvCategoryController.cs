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
using Nop.Core.Domain.Divui.Catalog;
using Nop.Core;
using System.Web;
using Nop.Admin.Infrastructure.Cache;
using Nop.Core.Caching;

namespace Nop.Admin.Controllers
{
    public partial class CategoryController
    {
        #region Fields
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Constructors

        public CategoryController(ICategoryService categoryService, ICategoryTemplateService categoryTemplateService,
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
            CatalogSettings catalogSettings,
            ISpecificationAttributeService specificationAttributeService,
            IWorkContext workContext,
            ICacheManager cacheManager)
        {
            this._categoryService = categoryService;
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
            _specificationAttributeService = specificationAttributeService;
            _workContext = workContext;
            _cacheManager = cacheManager;

        }

        #endregion

        #region Utilities
        [NonAction]
        protected virtual void PrepareAllCategoriesModel(CategoryModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCategories.Add(new SelectListItem
            {
                Text = "[None]",
                Value = "0"
            });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
            {
                model.AvailableCategories.Add(new SelectListItem
                {
                    Text = c.GetFormattedBreadCrumb(categories),
                    Value = c.Id.ToString()
                });
            }

            model.AvailableCategoryTypes = CategoryType.Destination.ToSelectList().ToList();

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
            model.AddSpecificationAttributeModel.ShowOnCategoryPage = true;
        }
        #endregion
        #region List / tree
        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var model = new CategoryListModel();

            model.AvaliableCategoryTypes = CategoryType.Destination.ToSelectList().ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, CategoryListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var categories = _categoryService.GetAllCategories(model.SearchCategoryName,
                command.Page - 1, command.PageSize, true, model.CatgoryTypeId);
            var gridModel = new DataSourceResult
            {
                Data = categories.Select(x =>
                {
                    var categoryModel = x.ToModel();
                    categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                    return categoryModel;
                }),
                Total = categories.TotalCount
            };
            return Json(gridModel);
        }

        public ActionResult GetCategoriesByCategoryTypeId(int CategoryTypeId)
        {
            var categories = _categoryService.GetAllCategories(showHidden: true, categoryTypeId: CategoryTypeId);
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem
            {
                Text = "[None]",
                Value = "0"
            });
            foreach (var c in categories)
            {
                result.Add(new SelectListItem
                {
                    Text = c.GetFormattedBreadCrumb(categories),
                    Value = c.Id.ToString()
                });
            }
            var data = new { success = true, data = result };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Category specification attributes

        [ValidateInput(false)]
        public ActionResult CategorySpecificationAttributeAdd(int attributeTypeId, int specificationAttributeOptionId,
            string customValue, bool allowFiltering, bool showOnCategoryPage,
            int displayOrder, int categoryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //a vendor should have access only to his categorys
            if (_workContext.CurrentVendor != null)
            {
                var category = _categoryService.GetCategoryById(categoryId);
                if (category != null)
                {
                    return RedirectToAction("List");
                }
            }

            //we allow filtering only for "Option" attribute type
            if (attributeTypeId != (int)SpecificationAttributeType.Option)
            {
                allowFiltering = false;
            }

            var psa = new CategorySpecificationAttribute
            {
                AttributeTypeId = attributeTypeId,
                SpecificationAttributeOptionId = specificationAttributeOptionId,
                CategoryId = categoryId,
                CustomValue = customValue,
                AllowFiltering = allowFiltering,
                ShowOnCategoryPage = showOnCategoryPage,
                DisplayOrder = displayOrder,
            };
            _specificationAttributeService.InsertCategorySpecificationAttribute(psa);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CategorySpecAttrList(DataSourceRequest command, int categoryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //a vendor should have access only to his categorys
            if (_workContext.CurrentVendor != null)
            {
                var category = _categoryService.GetCategoryById(categoryId);
                if (category != null)
                {
                    return Content("This is not your category");
                }
            }

            var categoryrSpecs = _specificationAttributeService.GetCategorySpecificationAttributes(categoryId);

            var categoryrSpecsModel = categoryrSpecs
                .Select(x =>
                {
                    var psaModel = new CategorySpecificationAttributeModel
                    {
                        Id = x.Id,
                        AttributeTypeName = x.AttributeType.GetLocalizedEnum(_localizationService, _workContext),
                        AttributeName = x.SpecificationAttributeOption.SpecificationAttribute.Name,
                        AllowFiltering = x.AllowFiltering,
                        ShowOnCategoryPage = x.ShowOnCategoryPage,
                        DisplayOrder = x.DisplayOrder
                    };
                    switch (x.AttributeType)
                    {
                        case SpecificationAttributeType.Option:
                            psaModel.ValueRaw = HttpUtility.HtmlEncode(x.SpecificationAttributeOption.Name);
                            break;
                        case SpecificationAttributeType.CustomText:
                            psaModel.ValueRaw = HttpUtility.HtmlEncode(x.CustomValue);
                            break;
                        case SpecificationAttributeType.CustomHtmlText:
                            //do not encode?
                            //psaModel.ValueRaw = x.CustomValue;
                            psaModel.ValueRaw = HttpUtility.HtmlEncode(x.CustomValue);
                            break;
                        case SpecificationAttributeType.Hyperlink:
                            psaModel.ValueRaw = x.CustomValue;
                            break;
                        default:
                            break;
                    }
                    return psaModel;
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = categoryrSpecsModel,
                Total = categoryrSpecsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult CategorySpecAttrUpdate(CategorySpecificationAttributeModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var psa = _specificationAttributeService.GetCategorySpecificationAttributeById(model.Id);
            if (psa == null)
                return Content("No category specification attribute found with the specified id");

            var categoryId = psa.Category.Id;

            //a vendor should have access only to his categorys
            if (_workContext.CurrentVendor != null)
            {
                var category = _categoryService.GetCategoryById(categoryId);
                if (category != null)
                {
                    return Content("This is not your category");
                }
            }

            //we do not allow editing these fields anymore (when we have distinct attribute types)
            //psa.CustomValue = model.CustomValue;
            //psa.AllowFiltering = model.AllowFiltering;
            psa.ShowOnCategoryPage = model.ShowOnCategoryPage;
            psa.DisplayOrder = model.DisplayOrder;
            _specificationAttributeService.UpdateCategorySpecificationAttribute(psa);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult CategorySpecAttrDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var psa = _specificationAttributeService.GetCategorySpecificationAttributeById(id);
            if (psa == null)
                throw new ArgumentException("No specification attribute found with the specified id");

            var categoryId = psa.CategoryId;

            //a vendor should have access only to his categorys
            if (_workContext.CurrentVendor != null)
            {
                var category = _categoryService.GetCategoryById(categoryId);
                if (category != null)
                {
                    return Content("This is not your category");
                }
            }

            _specificationAttributeService.DeleteCategorySpecificationAttribute(psa);

            return new NullJsonResult();
        }

        #endregion

    }
}