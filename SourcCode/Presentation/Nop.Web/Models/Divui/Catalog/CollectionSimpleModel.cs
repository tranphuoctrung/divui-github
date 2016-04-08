using System.Collections.Generic;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Catalog
{
    public class CollectionSimpleModel : BaseNopEntityModel
    {
        public CollectionSimpleModel()
        {
            SubCollections = new List<CollectionSimpleModel>();
        }

        public string Name { get; set; }

        public string SeName { get; set; }

        public int? NumberOfProducts { get; set; }

        public bool IncludeInTopMenu { get; set; }

        public List<CollectionSimpleModel> SubCollections { get; set; }
    }
}