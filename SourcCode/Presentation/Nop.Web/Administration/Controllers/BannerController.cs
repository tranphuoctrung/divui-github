using Nop.Admin.Extensions;
using Nop.Admin.Models.Catalog;
using Nop.Admin.Models.Divui.Catalog;
using Nop.Services.Divui.Catalog;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Admin.Controllers
{
    public class BannerController : BaseAdminController
    {
        #region Fields

        
        private readonly IPermissionService _permissionService;

        private readonly IBannerService _bannerService;

        private readonly ILocalizationService _localizationService;
        #endregion

        #region Constructors

        public BannerController(IBannerService bannerService,
            IPermissionService permissionService,
            ILocalizationService localizationService)
        {
            this._permissionService = permissionService;
            this._bannerService = bannerService;
            this._localizationService = localizationService;
        }

        #endregion
        // GET: Banner
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBanners))
                return AccessDeniedView();

            var model = new BannerListModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, BannerListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBanners))
                return AccessDeniedView();

            var categories = _bannerService.GetAllBanners(model.SearchBannerName,
                command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = categories.Select(x =>
                {
                    var bannerModel = x.ToModel();
                    
                    return bannerModel;
                }),
                Total = categories.TotalCount
            };
            return Json(gridModel);
        }


        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBanners))
                return AccessDeniedView();

           
            var model = new BannerModel();
           
            
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(BannerModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBanners))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var banner = model.ToEntity();
                banner.StartDate = model.StartDate;
                banner.EndDate = model.EndDate;

                _bannerService.InsertBanner(banner);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Banners.Added"));

                return continueEditing ? RedirectToAction("Edit", new { id = banner.Id }) : RedirectToAction("List");
            }

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBanners))
                return AccessDeniedView();

            var banner = _bannerService.GetBannerById(id);
            if (banner == null)
                //No blog post found with the specified id
                return RedirectToAction("List");
            
            var model = banner.ToModel();
            
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(BannerModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBanners))
                return AccessDeniedView();

            var banner = _bannerService.GetBannerById(model.Id);
            if (banner == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                banner = model.ToEntity(banner);
                banner.StartDate = model.StartDate;
                banner.EndDate = model.EndDate;
                _bannerService.UpdateBanner(banner);

                
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Banners.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = banner.Id });
                }
                return RedirectToAction("List");
            }
            
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBanners))
                return AccessDeniedView();

            var banner = _bannerService.GetBannerById(id);
            if (banner == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            _bannerService.DeleteBanner(banner);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Banners.Deleted"));
            return RedirectToAction("List");
        }

    }
}