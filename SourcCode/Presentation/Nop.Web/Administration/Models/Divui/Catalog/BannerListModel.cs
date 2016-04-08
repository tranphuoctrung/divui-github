using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Admin.Models.Divui.Catalog
{
    public class BannerListModel: BaseNopModel
    {
        [NopResourceDisplayName("Admin.Catalog.Banners.List.SearchBannerName")]
        [AllowHtml]
        public string SearchBannerName { get; set; }
    }
}