using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Catalog
{
    public partial class CollectionListModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.Catalog.Collections.List.SearchCollectionName")]
        [AllowHtml]
        public string SearchCollectionName { get; set; }
    }
}