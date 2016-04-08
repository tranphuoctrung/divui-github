using System.Collections.Generic;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Catalog
{
    public class AttractionSimpleModel : BaseNopEntityModel
    {
        public AttractionSimpleModel()
        {
            SubAttractions = new List<AttractionSimpleModel>();
        }

        public string Name { get; set; }

        public string SeName { get; set; }

        public int? NumberOfProducts { get; set; }

        public bool IncludeInTopMenu { get; set; }

        public List<AttractionSimpleModel> SubAttractions { get; set; }
    }
}