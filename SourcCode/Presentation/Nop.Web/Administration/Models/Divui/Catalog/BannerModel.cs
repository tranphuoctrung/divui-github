using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Admin.Models.Divui.Catalog
{
    public class BannerModel : BaseNopEntityModel
    {
        public BannerModel()
        {
            Published = true;
            Deleted = false;
        }

        [NopResourceDisplayName("Admin.Catalog.Banners.Fields.Name")]
        public virtual string Name { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Banners.Fields.Url")]
        public virtual string Url { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Banners.Fields.Url")]
        [UIHint("Picture")]
        public virtual int PictureId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Banners.Fields.DisplayOrder")]
        [UIHint("Int32")]
        public virtual int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Banners.Fields.Published")]
        public virtual bool Published { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Banners.Fields.Deleted")]
        public virtual bool Deleted { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Banners.Fields.Target")]
        public virtual string Target { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Banners.Fields.StartDate")]
        [UIHint("DateTimeNullable")]
        public virtual DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Banners.Fields.EndDate")]
        [UIHint("DateTimeNullable")]
        public virtual DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Banners.Fields.Description")]
        [AllowHtml]
        public virtual string Description { get; set; }

    }
}