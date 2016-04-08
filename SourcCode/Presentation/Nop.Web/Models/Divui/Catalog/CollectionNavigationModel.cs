using System.Collections.Generic;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Catalog
{
    public partial class CollectionNavigationModel : BaseNopModel
    {
        public CollectionNavigationModel()
        {
            Collections = new List<CollectionSimpleModel>();
        }

        public int CurrentCollectionId { get; set; }
        public List<CollectionSimpleModel> Collections { get; set; }
    }
}