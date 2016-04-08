using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Catalog
{
    public partial class CategorySimpleModel
    {
        public bool DisplayedOnHomePage { get; set; }

        public bool IsNewCategory { get; set; }
    }
}