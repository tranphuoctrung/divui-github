using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Divui.Catalog
{
    public class CategorySpecificationModel
    {
        public int SpecificationAttributeId { get; set; }

        public string SpecificationAttributeName { get; set; }

        //this value is already HTML encoded
        public string ValueRaw { get; set; }
        public string SystemName { get; set; }
        public string CssClass { get; set; }

    }
}