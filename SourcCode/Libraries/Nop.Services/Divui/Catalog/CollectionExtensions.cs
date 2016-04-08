using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Core.Domain.Divui.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Sort collections for tree representation
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="parentId">Parent collection identifier</param>
        /// <param name="ignoreCollectionsWithoutExistingParent">A value indicating whether collections without parent collection in provided collection list (source) should be ignored</param>
        /// <returns>Sorted collections</returns>
        public static IList<Collection> SortCollectionsForTree(this IList<Collection> source, int parentId = 0, bool ignoreCollectionsWithoutExistingParent = false)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var result = new List<Collection>();

            foreach (var cat in source.Where(c => c.ParentCollectionId == parentId).ToList())
            {
                result.Add(cat);
                result.AddRange(SortCollectionsForTree(source, cat.Id, true));
            }
            if (!ignoreCollectionsWithoutExistingParent && result.Count != source.Count)
            {
                //find collections without parent in provided collection source and insert them into result
                foreach (var cat in source)
                    if (result.FirstOrDefault(x => x.Id == cat.Id) == null)
                        result.Add(cat);
            }
            return result;
        }

        /// <summary>
        /// Returns a ProductCollection that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="collectionId">Collection identifier</param>
        /// <returns>A ProductCollection that has the specified values; otherwise null</returns>
        public static ProductCollection FindProductCollection(this IList<ProductCollection> source,
            int productId, int collectionId)
        {
            foreach (var productCollection in source)
                if (productCollection.ProductId == productId && productCollection.CollectionId == collectionId)
                    return productCollection;

            return null;
        }

        /// <summary>
        /// Get formatted collection breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="collection">Collection</param>
        /// <param name="collectionService">Collection service</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        public static string GetFormattedBreadCrumb(this Collection collection,
            ICollectionService collectionService,
            string separator = ">>", int languageId = 0)
        {
            string result = string.Empty;

            var breadcrumb = GetCollectionBreadCrumb(collection, collectionService, null, null, true);
            for (int i = 0; i <= breadcrumb.Count - 1; i++)
            {
                var collectionName = breadcrumb[i].GetLocalized(x => x.Name, languageId);
                result = String.IsNullOrEmpty(result)
                    ? collectionName
                    : string.Format("{0} {1} {2}", result, separator, collectionName);
            }

            return result;
        }

        /// <summary>
        /// Get formatted collection breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="collection">Collection</param>
        /// <param name="allCollections">All collections</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        public static string GetFormattedBreadCrumb(this Collection collection,
            IList<Collection> allCollections,
            string separator = ">>", int languageId = 0)
        {
            string result = string.Empty;

            var breadcrumb = GetCollectionBreadCrumb(collection, allCollections, null, null, true);
            for (int i = 0; i <= breadcrumb.Count - 1; i++)
            {
                var collectionName = breadcrumb[i].GetLocalized(x => x.Name, languageId);
                result = String.IsNullOrEmpty(result)
                    ? collectionName
                    : string.Format("{0} {1} {2}", result, separator, collectionName);
            }

            return result;
        }

        /// <summary>
        /// Get collection breadcrumb 
        /// </summary>
        /// <param name="collection">Collection</param>
        /// <param name="collectionService">Collection service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Collection breadcrumb </returns>
        public static IList<Collection> GetCollectionBreadCrumb(this Collection collection,
            ICollectionService collectionService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            bool showHidden = false)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            var result = new List<Collection>();

            //used to prevent circular references
            var alreadyProcessedCollectionIds = new List<int>();

            while (collection != null && //not null
                !collection.Deleted && //not deleted
                (showHidden || collection.Published) && //published
                (showHidden || aclService.Authorize(collection)) && //ACL
                (showHidden || storeMappingService.Authorize(collection)) && //Store mapping
                !alreadyProcessedCollectionIds.Contains(collection.Id)) //prevent circular references
            {
                result.Add(collection);

                alreadyProcessedCollectionIds.Add(collection.Id);

                collection = collectionService.GetCollectionById(collection.ParentCollectionId);
            }
            result.Reverse();
            return result;
        }

        /// <summary>
        /// Get collection breadcrumb 
        /// </summary>
        /// <param name="collection">Collection</param>
        /// <param name="allCollections">All collections</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Collection breadcrumb </returns>
        public static IList<Collection> GetCollectionBreadCrumb(this Collection collection,
            IList<Collection> allCollections,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            bool showHidden = false)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            var result = new List<Collection>();

            //used to prevent circular references
            var alreadyProcessedCollectionIds = new List<int>();

            while (collection != null && //not null
                !collection.Deleted && //not deleted
                (showHidden || collection.Published) && //published
                (showHidden || aclService.Authorize(collection)) && //ACL
                (showHidden || storeMappingService.Authorize(collection)) && //Store mapping
                !alreadyProcessedCollectionIds.Contains(collection.Id)) //prevent circular references
            {
                result.Add(collection);

                alreadyProcessedCollectionIds.Add(collection.Id);

                collection = (from c in allCollections
                            where c.Id == collection.ParentCollectionId
                            select c).FirstOrDefault();
            }
            result.Reverse();
            return result;
        }
    }
}
