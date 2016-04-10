using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Settings
{
    public partial class CatalogSettingsModel
    {
        [NopResourceDisplayName("Admin.Configuration.Settings.Catalog.DefaultCategoryProductNumber")]
        public int DefaultCategoryProductNumber { get; set; }
        public bool DefaultCategoryProductNumber_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.Catalog.DefaultCollectionProductNumber")]
        public int DefaultCollectionProductNumber { get; set; }
        public bool DefaultCollectionProductNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Catalog.DefaultAttractionProductNumber")]
        public int DefaultAttractionProductNumber { get; set; }
        public bool DefaultAttractionProductNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Catalog.DefaultDestinationProductNumber")]
        public int DefaultDestinationProductNumber { get; set; }
        public bool DefaultDestinationProductNumber_OverrideForStore { get; set; }
    }
}