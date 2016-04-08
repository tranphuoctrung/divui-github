using Nop.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Admin.Models.Catalog
{
    public partial class ProductAttributeModel
    {
        [NopResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.CssClass")]
        [AllowHtml]
        public string CssClass { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.SystemName")]
        public string SystemName { get; set; }
    }
}