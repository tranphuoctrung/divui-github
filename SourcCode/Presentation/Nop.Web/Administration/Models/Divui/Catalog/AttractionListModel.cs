using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Catalog
{
    public partial class AttractionListModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.Catalog.Attractions.List.SearchAttractionName")]
        [AllowHtml]
        public string SearchAttractionName { get; set; }
    }
}