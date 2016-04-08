using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Divui.Catalog
{
    public class SpecificationAttributeOptionModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        public string FilterUrl { get; set; }

        public bool Selected { get; set; }
    }
}