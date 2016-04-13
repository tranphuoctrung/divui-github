using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Catalog
{
    public partial class CategoryNavigationModel
    {
        public string LocalizationResourceName { get; set; }
        public string CssClass { get; set; }
        public string SearchInputName { get; set; }
    }
}