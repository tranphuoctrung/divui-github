using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Catalog
{
    public partial class SpecificationAttributeService
    {
        private readonly IRepository<CategorySpecificationAttribute> _categorySpecificationAttributeRepository;


        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="specificationAttributeRepository">Specification attribute repository</param>
        /// <param name="specificationAttributeOptionRepository">Specification attribute option repository</param>
        /// <param name="productSpecificationAttributeRepository">Product specification attribute repository</param>
        /// <param name="eventPublisher">Event published</param>
        public SpecificationAttributeService(ICacheManager cacheManager,
            IRepository<SpecificationAttribute> specificationAttributeRepository,
            IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IRepository<CategorySpecificationAttribute> categorySpecificationAttributeRepository,
            IEventPublisher eventPublisher)
        {
            _cacheManager = cacheManager;
            _specificationAttributeRepository = specificationAttributeRepository;
            _specificationAttributeOptionRepository = specificationAttributeOptionRepository;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _categorySpecificationAttributeRepository = categorySpecificationAttributeRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Category specification attribute

        /// <summary>
        /// Deletes a category specification attribute mapping
        /// </summary>
        /// <param name="categorySpecificationAttribute">Category specification attribute</param>
        public virtual void DeleteCategorySpecificationAttribute(CategorySpecificationAttribute categorySpecificationAttribute)
        {
            if (categorySpecificationAttribute == null)
                throw new ArgumentNullException("categorySpecificationAttribute");

            _categorySpecificationAttributeRepository.Delete(categorySpecificationAttribute);

            _cacheManager.RemoveByPattern(PRODUCTSPECIFICATIONATTRIBUTE_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(categorySpecificationAttribute);
        }

        /// <summary>
        /// Gets a category specification attribute mapping collection
        /// </summary>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="specificationAttributeOptionId">Specification attribute option identifier; 0 to load all records</param>
        /// <param name="allowFiltering">0 to load attributes with AllowFiltering set to false, 1 to load attributes with AllowFiltering set to true, null to load all attributes</param>
        /// <param name="showOnCategoryPage">0 to load attributes with ShowOnCategoryPage set to false, 1 to load attributes with ShowOnCategoryPage set to true, null to load all attributes</param>
        /// <returns>Category specification attribute mapping collection</returns>
        public virtual IList<CategorySpecificationAttribute> GetCategorySpecificationAttributes(int categoryId = 0,
            int specificationAttributeOptionId = 0, bool? allowFiltering = null, bool? showOnCategoryPage = null)
        {
            string allowFilteringCacheStr = allowFiltering.HasValue ? allowFiltering.ToString() : "null";
            string showOnCategoryPageCacheStr = showOnCategoryPage.HasValue ? showOnCategoryPage.ToString() : "null";
            string key = string.Format(PRODUCTSPECIFICATIONATTRIBUTE_ALLBYPRODUCTID_KEY, categoryId, allowFilteringCacheStr, showOnCategoryPageCacheStr);

            return _cacheManager.Get(key, () =>
            {
                var query = _categorySpecificationAttributeRepository.Table;
                if (categoryId > 0)
                    query = query.Where(psa => psa.CategoryId == categoryId);
                if (specificationAttributeOptionId > 0)
                    query = query.Where(psa => psa.SpecificationAttributeOptionId == specificationAttributeOptionId);
                if (allowFiltering.HasValue)
                    query = query.Where(psa => psa.AllowFiltering == allowFiltering.Value);
                if (showOnCategoryPage.HasValue)
                    query = query.Where(psa => psa.ShowOnCategoryPage == showOnCategoryPage.Value);
                query = query.OrderBy(psa => psa.DisplayOrder);

                var categorySpecificationAttributes = query.ToList();
                return categorySpecificationAttributes;
            });
        }

        /// <summary>
        /// Gets a category specification attribute mapping 
        /// </summary>
        /// <param name="categorySpecificationAttributeId">Category specification attribute mapping identifier</param>
        /// <returns>Category specification attribute mapping</returns>
        public virtual CategorySpecificationAttribute GetCategorySpecificationAttributeById(int categorySpecificationAttributeId)
        {
            if (categorySpecificationAttributeId == 0)
                return null;

            return _categorySpecificationAttributeRepository.GetById(categorySpecificationAttributeId);
        }

        /// <summary>
        /// Inserts a category specification attribute mapping
        /// </summary>
        /// <param name="categorySpecificationAttribute">Category specification attribute mapping</param>
        public virtual void InsertCategorySpecificationAttribute(CategorySpecificationAttribute categorySpecificationAttribute)
        {
            if (categorySpecificationAttribute == null)
                throw new ArgumentNullException("categorySpecificationAttribute");

            _categorySpecificationAttributeRepository.Insert(categorySpecificationAttribute);

            _cacheManager.RemoveByPattern(PRODUCTSPECIFICATIONATTRIBUTE_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(categorySpecificationAttribute);
        }

        /// <summary>
        /// Updates the category specification attribute mapping
        /// </summary>
        /// <param name="categorySpecificationAttribute">Category specification attribute mapping</param>
        public virtual void UpdateCategorySpecificationAttribute(CategorySpecificationAttribute categorySpecificationAttribute)
        {
            if (categorySpecificationAttribute == null)
                throw new ArgumentNullException("categorySpecificationAttribute");

            _categorySpecificationAttributeRepository.Update(categorySpecificationAttribute);

            _cacheManager.RemoveByPattern(PRODUCTSPECIFICATIONATTRIBUTE_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(categorySpecificationAttribute);
        }

        /// <summary>
        /// Gets a count of category specification attribute mapping records
        /// </summary>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="specificationAttributeOptionId">The specification attribute option identifier; 0 to load all records</param>
        /// <returns>Count</returns>
        public virtual int GetCategorySpecificationAttributeCount(int categoryId = 0, int specificationAttributeOptionId = 0)
        {
            var query = _categorySpecificationAttributeRepository.Table;
            if (categoryId > 0)
                query = query.Where(psa => psa.CategoryId == categoryId);
            if (specificationAttributeOptionId > 0)
                query = query.Where(psa => psa.SpecificationAttributeOptionId == specificationAttributeOptionId);

            return query.Count();
        }

        #endregion
    }
}
