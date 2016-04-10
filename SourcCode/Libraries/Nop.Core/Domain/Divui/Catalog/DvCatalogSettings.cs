using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    public partial class CatalogSettings
    {
        /// Gets or sets the default value to use for Category product number
        /// </summary>
        public int DefaultCategoryProductNumber { get; set; }

        /// Gets or sets the default value to use for Collection product number
        /// </summary>
        public int DefaultCollectionProductNumber { get; set; }

        /// Gets or sets the default value to use for Attraction product number
        /// </summary>
        public int DefaultAttractionProductNumber { get; set; }

        /// Gets or sets the default value to use for Destination product number
        /// </summary>
        public int DefaultDestinationProductNumber { get; set; }
    }
}
