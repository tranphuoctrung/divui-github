using System.Collections.Generic;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Catalog
{
    public partial class CategorySimpleModel : BaseNopEntityModel
    {
        public CategorySimpleModel()
        {
            SubCategories = new List<CategorySimpleModel>();
        }

        public string Name { get; set; }

        public string SeName { get; set; }

        public int? NumberOfProducts { get; set; }

        public bool IncludeInTopMenu { get; set; }

        public int ParentCategoryId { get; set; }

        public List<CategorySimpleModel> SubCategories { get; set; }
    }
}