using System.Collections.Generic;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Catalog
{
    public partial class AttractionNavigationModel : BaseNopModel
    {
        public AttractionNavigationModel()
        {
            Attractions = new List<AttractionSimpleModel>();
        }

        public int CurrentAttractionId { get; set; }
        public List<AttractionSimpleModel> Attractions { get; set; }
    }
}