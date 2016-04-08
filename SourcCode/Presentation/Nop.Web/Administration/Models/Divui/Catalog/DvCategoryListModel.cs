using Nop.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Admin.Models.Catalog
{
    public partial class CategoryListModel
    {
        public CategoryListModel() {
            AvaliableCategoryTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Catalog.Categories.List.CatgoryType")]
        public int CatgoryTypeId { get; set; }
        public List<SelectListItem> AvaliableCategoryTypes { get; set; }
    }
}