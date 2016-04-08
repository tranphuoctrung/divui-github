using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Attraction service interface
    /// </summary>
    public partial interface IAttractionService
    {
        /// <summary>
        /// Delete attraction
        /// </summary>
        /// <param name="attraction">Attraction</param>
        void DeleteAttraction(Attraction attraction);

        /// <summary>
        /// Gets all attractions
        /// </summary>
        /// <param name="attractionName">Attraction name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Attractions</returns>
        IPagedList<Attraction> GetAllAttractions(string attractionName = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// Gets all attractions filtered by parent attraction identifier
        /// </summary>
        /// <param name="parentAttractionId">Parent attraction identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="includeAllLevels">A value indicating whether we should load all child levels</param>
        /// <returns>Attractions</returns>
        IList<Attraction> GetAllAttractionsByParentAttractionId(int parentAttractionId,
            bool showHidden = false, bool includeAllLevels = false);

        /// <summary>
        /// Gets all attractions displayed on the home page
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Attractions</returns>
        IList<Attraction> GetAllAttractionsDisplayedOnHomePage(bool showHidden = false);
                
        /// <summary>
        /// Gets a attraction
        /// </summary>
        /// <param name="attractionId">Attraction identifier</param>
        /// <returns>Attraction</returns>
        Attraction GetAttractionById(int attractionId);

        /// <summary>
        /// Inserts attraction
        /// </summary>
        /// <param name="attraction">Attraction</param>
        void InsertAttraction(Attraction attraction);

        /// <summary>
        /// Updates the attraction
        /// </summary>
        /// <param name="attraction">Attraction</param>
        void UpdateAttraction(Attraction attraction);
        
        /// <summary>
        /// Deletes a product attraction mapping
        /// </summary>
        /// <param name="productAttraction">Product attraction</param>
        void DeleteProductAttraction(ProductAttraction productAttraction);

        /// <summary>
        /// Gets product attraction mapping attraction
        /// </summary>
        /// <param name="attractionId">Attraction identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product a attraction mapping attraction</returns>
        IPagedList<ProductAttraction> GetProductAttractionsByAttractionId(int attractionId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// Gets a product attraction mapping attraction
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product attraction mapping attraction</returns>
        IList<ProductAttraction> GetProductAttractionsByProductId(int productId, bool showHidden = false);
        /// <summary>
        /// Gets a product attraction mapping attraction
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier (used in multi-store environment). "showHidden" parameter should also be "true"</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns> Product attraction mapping attraction</returns>
        IList<ProductAttraction> GetProductAttractionsByProductId(int productId, int storeId, bool showHidden = false);

        /// <summary>
        /// Gets a product attraction mapping 
        /// </summary>
        /// <param name="productAttractionId">Product attraction mapping identifier</param>
        /// <returns>Product attraction mapping</returns>
        ProductAttraction GetProductAttractionById(int productAttractionId);

        /// <summary>
        /// Inserts a product attraction mapping
        /// </summary>
        /// <param name="productAttraction">>Product attraction mapping</param>
        void InsertProductAttraction(ProductAttraction productAttraction);

        /// <summary>
        /// Updates the product attraction mapping 
        /// </summary>
        /// <param name="productAttraction">>Product attraction mapping</param>
        void UpdateProductAttraction(ProductAttraction productAttraction);
    }
}
