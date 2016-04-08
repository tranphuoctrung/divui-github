using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Collection service interface
    /// </summary>
    public partial interface ICollectionService
    {
        /// <summary>
        /// Delete collection
        /// </summary>
        /// <param name="collection">Collection</param>
        void DeleteCollection(Collection collection);

        /// <summary>
        /// Gets all collections
        /// </summary>
        /// <param name="collectionName">Collection name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Collections</returns>
        IPagedList<Collection> GetAllCollections(string collectionName = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// Gets all collections filtered by parent collection identifier
        /// </summary>
        /// <param name="parentCollectionId">Parent collection identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="includeAllLevels">A value indicating whether we should load all child levels</param>
        /// <returns>Collections</returns>
        IList<Collection> GetAllCollectionsByParentCollectionId(int parentCollectionId,
            bool showHidden = false, bool includeAllLevels = false);

        /// <summary>
        /// Gets all collections displayed on the home page
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Collections</returns>
        IList<Collection> GetAllCollectionsDisplayedOnHomePage(bool showHidden = false);
                
        /// <summary>
        /// Gets a collection
        /// </summary>
        /// <param name="collectionId">Collection identifier</param>
        /// <returns>Collection</returns>
        Collection GetCollectionById(int collectionId);

        /// <summary>
        /// Inserts collection
        /// </summary>
        /// <param name="collection">Collection</param>
        void InsertCollection(Collection collection);

        /// <summary>
        /// Updates the collection
        /// </summary>
        /// <param name="collection">Collection</param>
        void UpdateCollection(Collection collection);
        
        /// <summary>
        /// Deletes a product collection mapping
        /// </summary>
        /// <param name="productCollection">Product collection</param>
        void DeleteProductCollection(ProductCollection productCollection);

        /// <summary>
        /// Gets product collection mapping collection
        /// </summary>
        /// <param name="collectionId">Collection identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product a collection mapping collection</returns>
        IPagedList<ProductCollection> GetProductCollectionsByCollectionId(int collectionId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// Gets a product collection mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product collection mapping collection</returns>
        IList<ProductCollection> GetProductCollectionsByProductId(int productId, bool showHidden = false);
        /// <summary>
        /// Gets a product collection mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier (used in multi-store environment). "showHidden" parameter should also be "true"</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns> Product collection mapping collection</returns>
        IList<ProductCollection> GetProductCollectionsByProductId(int productId, int storeId, bool showHidden = false);

        /// <summary>
        /// Gets a product collection mapping 
        /// </summary>
        /// <param name="productCollectionId">Product collection mapping identifier</param>
        /// <returns>Product collection mapping</returns>
        ProductCollection GetProductCollectionById(int productCollectionId);

        /// <summary>
        /// Inserts a product collection mapping
        /// </summary>
        /// <param name="productCollection">>Product collection mapping</param>
        void InsertProductCollection(ProductCollection productCollection);

        /// <summary>
        /// Updates the product collection mapping 
        /// </summary>
        /// <param name="productCollection">>Product collection mapping</param>
        void UpdateProductCollection(ProductCollection productCollection);
    }
}
