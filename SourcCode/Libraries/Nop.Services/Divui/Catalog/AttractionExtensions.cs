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
    public static class AttractionExtensions
    {
        /// <summary>
        /// Sort attractions for tree representation
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="parentId">Parent attraction identifier</param>
        /// <param name="ignoreAttractionsWithoutExistingParent">A value indicating whether attractions without parent attraction in provided attraction list (source) should be ignored</param>
        /// <returns>Sorted attractions</returns>
        public static IList<Attraction> SortAttractionsForTree(this IList<Attraction> source, int parentId = 0, bool ignoreAttractionsWithoutExistingParent = false)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var result = new List<Attraction>();

            foreach (var cat in source.Where(c => c.ParentAttractionId == parentId).ToList())
            {
                result.Add(cat);
                result.AddRange(SortAttractionsForTree(source, cat.Id, true));
            }
            if (!ignoreAttractionsWithoutExistingParent && result.Count != source.Count)
            {
                //find attractions without parent in provided attraction source and insert them into result
                foreach (var cat in source)
                    if (result.FirstOrDefault(x => x.Id == cat.Id) == null)
                        result.Add(cat);
            }
            return result;
        }

        /// <summary>
        /// Returns a ProductAttraction that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="attractionId">Attraction identifier</param>
        /// <returns>A ProductAttraction that has the specified values; otherwise null</returns>
        public static ProductAttraction FindProductAttraction(this IList<ProductAttraction> source,
            int productId, int attractionId)
        {
            foreach (var productAttraction in source)
                if (productAttraction.ProductId == productId && productAttraction.AttractionId == attractionId)
                    return productAttraction;

            return null;
        }

        /// <summary>
        /// Get formatted attraction breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="attraction">Attraction</param>
        /// <param name="attractionService">Attraction service</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        public static string GetFormattedBreadCrumb(this Attraction attraction,
            IAttractionService attractionService,
            string separator = ">>", int languageId = 0)
        {
            string result = string.Empty;

            var breadcrumb = GetAttractionBreadCrumb(attraction, attractionService, null, null, true);
            for (int i = 0; i <= breadcrumb.Count - 1; i++)
            {
                var attractionName = breadcrumb[i].GetLocalized(x => x.Name, languageId);
                result = String.IsNullOrEmpty(result)
                    ? attractionName
                    : string.Format("{0} {1} {2}", result, separator, attractionName);
            }

            return result;
        }

        /// <summary>
        /// Get formatted attraction breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="attraction">Attraction</param>
        /// <param name="allAttractions">All attractions</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        public static string GetFormattedBreadCrumb(this Attraction attraction,
            IList<Attraction> allAttractions,
            string separator = ">>", int languageId = 0)
        {
            string result = string.Empty;

            var breadcrumb = GetAttractionBreadCrumb(attraction, allAttractions, null, null, true);
            for (int i = 0; i <= breadcrumb.Count - 1; i++)
            {
                var attractionName = breadcrumb[i].GetLocalized(x => x.Name, languageId);
                result = String.IsNullOrEmpty(result)
                    ? attractionName
                    : string.Format("{0} {1} {2}", result, separator, attractionName);
            }

            return result;
        }

        /// <summary>
        /// Get attraction breadcrumb 
        /// </summary>
        /// <param name="attraction">Attraction</param>
        /// <param name="attractionService">Attraction service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Attraction breadcrumb </returns>
        public static IList<Attraction> GetAttractionBreadCrumb(this Attraction attraction,
            IAttractionService attractionService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            bool showHidden = false)
        {
            if (attraction == null)
                throw new ArgumentNullException("attraction");

            var result = new List<Attraction>();

            //used to prevent circular references
            var alreadyProcessedAttractionIds = new List<int>();

            while (attraction != null && //not null
                !attraction.Deleted && //not deleted
                (showHidden || attraction.Published) && //published
                (showHidden || aclService.Authorize(attraction)) && //ACL
                (showHidden || storeMappingService.Authorize(attraction)) && //Store mapping
                !alreadyProcessedAttractionIds.Contains(attraction.Id)) //prevent circular references
            {
                result.Add(attraction);

                alreadyProcessedAttractionIds.Add(attraction.Id);

                attraction = attractionService.GetAttractionById(attraction.ParentAttractionId);
            }
            result.Reverse();
            return result;
        }

        /// <summary>
        /// Get attraction breadcrumb 
        /// </summary>
        /// <param name="attraction">Attraction</param>
        /// <param name="allAttractions">All attractions</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Attraction breadcrumb </returns>
        public static IList<Attraction> GetAttractionBreadCrumb(this Attraction attraction,
            IList<Attraction> allAttractions,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            bool showHidden = false)
        {
            if (attraction == null)
                throw new ArgumentNullException("attraction");

            var result = new List<Attraction>();

            //used to prevent circular references
            var alreadyProcessedAttractionIds = new List<int>();

            while (attraction != null && //not null
                !attraction.Deleted && //not deleted
                (showHidden || attraction.Published) && //published
                (showHidden || aclService.Authorize(attraction)) && //ACL
                (showHidden || storeMappingService.Authorize(attraction)) && //Store mapping
                !alreadyProcessedAttractionIds.Contains(attraction.Id)) //prevent circular references
            {
                result.Add(attraction);

                alreadyProcessedAttractionIds.Add(attraction.Id);

                attraction = (from c in allAttractions
                            where c.Id == attraction.ParentAttractionId
                            select c).FirstOrDefault();
            }
            result.Reverse();
            return result;
        }
    }
}
