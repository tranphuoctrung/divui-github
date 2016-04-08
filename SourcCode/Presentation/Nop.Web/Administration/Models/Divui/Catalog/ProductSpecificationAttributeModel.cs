using System.Web.Mvc;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Catalog
{
    public partial class CategorySpecificationAttributeModel : BaseNopEntityModel
    {
        [AllowHtml]
        public string AttributeTypeName { get; set; }

        [AllowHtml]
        public string AttributeName { get; set; }

        [AllowHtml]
        public string ValueRaw { get; set; }

        public bool AllowFiltering { get; set; }

        public bool ShowOnCategoryPage { get; set; }

        public int DisplayOrder { get; set; }
    }
}