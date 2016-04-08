using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Attraction service
    /// </summary>
    public partial class AttractionService : IAttractionService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : attraction ID
        /// </remarks>
        private const string ATTRACTIONS_BY_ID_KEY = "Nop.attraction.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : parent attraction ID
        /// {1} : show hidden records?
        /// {2} : current customer ID
        /// {3} : store ID
        /// {3} : include all levels (child)
        /// </remarks>
        private const string ATTRACTIONS_BY_PARENT_COLLECTION_ID_KEY = "Nop.attraction.byparent-{0}-{1}-{2}-{3}-{4}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : attraction ID
        /// {2} : page index
        /// {3} : page size
        /// {4} : current customer ID
        /// {5} : store ID
        /// </remarks>
        private const string PRODUCTATTRACTIONS_ALLBYCOLLECTIONID_KEY = "Nop.productattraction.allbyattractionid-{0}-{1}-{2}-{3}-{4}-{5}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : product ID
        /// {2} : current customer ID
        /// {3} : store ID
        /// </remarks>
        private const string PRODUCTATTRACTIONS_ALLBYPRODUCTID_KEY = "Nop.productattraction.allbyproductid-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string ATTRACTIONS_PATTERN_KEY = "Nop.attraction.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTATTRACTIONS_PATTERN_KEY = "Nop.productattraction.";

        #endregion

        #region Fields

        private readonly IRepository<Attraction> _attractionRepository;
        private readonly IRepository<ProductAttraction> _productAttractionRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly CatalogSettings _catalogSettings;

        #endregion
        
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="attractionRepository">Attraction repository</param>
        /// <param name="productAttractionRepository">ProductAttraction repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="aclRepository">ACL record repository</param>
        /// <param name="storeMappingRepository">Store mapping repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public AttractionService(ICacheManager cacheManager,
            IRepository<Attraction> attractionRepository,
            IRepository<ProductAttraction> productAttractionRepository,
            IRepository<Product> productRepository,
            IRepository<AclRecord> aclRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IWorkContext workContext,
            IStoreContext storeContext,
            IEventPublisher eventPublisher,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            CatalogSettings catalogSettings)
        {
            this._cacheManager = cacheManager;
            this._attractionRepository = attractionRepository;
            this._productAttractionRepository = productAttractionRepository;
            this._productRepository = productRepository;
            this._aclRepository = aclRepository;
            this._storeMappingRepository = storeMappingRepository;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._eventPublisher = eventPublisher;
            this._storeMappingService = storeMappingService;
            this._aclService = aclService;
            this._catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete attraction
        /// </summary>
        /// <param name="attraction">Attraction</param>
        public virtual void DeleteAttraction(Attraction attraction)
        {
            if (attraction == null)
                throw new ArgumentNullException("attraction");

            attraction.Deleted = true;
            UpdateAttraction(attraction);

            //reset a "Parent attraction" property of all child subattractions
            var subattractions = GetAllAttractionsByParentAttractionId(attraction.Id, true);
            foreach (var subattraction in subattractions)
            {
                subattraction.ParentAttractionId = 0;
                UpdateAttraction(subattraction);
            }
        }
        
        /// <summary>
        /// Gets all attractions
        /// </summary>
        /// <param name="attractionName">Attraction name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Attractions</returns>
        public virtual IPagedList<Attraction> GetAllAttractions(string attractionName = "", 
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = _attractionRepository.Table;
            if (!showHidden)
                query = query.Where(c => c.Published);
            if (!String.IsNullOrWhiteSpace(attractionName))
                query = query.Where(c => c.Name.Contains(attractionName));
            query = query.Where(c => !c.Deleted);
            query = query.OrderBy(c => c.ParentAttractionId).ThenBy(c => c.DisplayOrder);
            
            if (!showHidden && (!_catalogSettings.IgnoreAcl || !_catalogSettings.IgnoreStoreLimitations))
            {
                if (!_catalogSettings.IgnoreAcl)
                {
                    //ACL (access control list)
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                    query = from c in query
                            join acl in _aclRepository.Table
                            on new { c1 = c.Id, c2 = "Attraction" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
                            from acl in c_acl.DefaultIfEmpty()
                            where !c.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                            select c;
                }
                if (!_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    var currentStoreId = _storeContext.CurrentStore.Id;
                    query = from c in query
                            join sm in _storeMappingRepository.Table
                            on new { c1 = c.Id, c2 = "Attraction" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
                            from sm in c_sm.DefaultIfEmpty()
                            where !c.LimitedToStores || currentStoreId == sm.StoreId
                            select c;
                }

                //only distinct attractions (group by ID)
                query = from c in query
                        group c by c.Id
                        into cGroup
                        orderby cGroup.Key
                        select cGroup.FirstOrDefault();
                query = query.OrderBy(c => c.ParentAttractionId).ThenBy(c => c.DisplayOrder);
            }
            
            var unsortedAttractions = query.ToList();

            //sort attractions
            var sortedAttractions = unsortedAttractions.SortAttractionsForTree();

            //paging
            return new PagedList<Attraction>(sortedAttractions, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all attractions filtered by parent attraction identifier
        /// </summary>
        /// <param name="parentAttractionId">Parent attraction identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="includeAllLevels">A value indicating whether we should load all child levels</param>
        /// <returns>Attractions</returns>
        public virtual IList<Attraction> GetAllAttractionsByParentAttractionId(int parentAttractionId,
            bool showHidden = false, bool includeAllLevels = false)
        {
            string key = string.Format(ATTRACTIONS_BY_PARENT_COLLECTION_ID_KEY, parentAttractionId, showHidden, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id, includeAllLevels);
            return _cacheManager.Get(key, () =>
            {
                var query = _attractionRepository.Table;
                if (!showHidden)
                    query = query.Where(c => c.Published);
                query = query.Where(c => c.ParentAttractionId == parentAttractionId);
                query = query.Where(c => !c.Deleted);
                query = query.OrderBy(c => c.DisplayOrder);

                if (!showHidden && (!_catalogSettings.IgnoreAcl || !_catalogSettings.IgnoreStoreLimitations))
                {
                    if (!_catalogSettings.IgnoreAcl)
                    {
                        //ACL (access control list)
                        var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                        query = from c in query
                                join acl in _aclRepository.Table
                                on new { c1 = c.Id, c2 = "Attraction" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
                                from acl in c_acl.DefaultIfEmpty()
                                where !c.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                                select c;
                    }
                    if (!_catalogSettings.IgnoreStoreLimitations)
                    {
                        //Store mapping
                        var currentStoreId = _storeContext.CurrentStore.Id;
                        query = from c in query
                                join sm in _storeMappingRepository.Table
                                on new { c1 = c.Id, c2 = "Attraction" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
                                from sm in c_sm.DefaultIfEmpty()
                                where !c.LimitedToStores || currentStoreId == sm.StoreId
                                select c;
                    }
                    //only distinct attractions (group by ID)
                    query = from c in query
                            group c by c.Id
                            into cGroup
                            orderby cGroup.Key
                            select cGroup.FirstOrDefault();
                    query = query.OrderBy(c => c.DisplayOrder);
                }

                var attractions = query.ToList();
                if (includeAllLevels)
                {
                    var childAttractions = new List<Attraction>();
                    //add child levels
                    foreach (var attraction in attractions)
                    {
                        childAttractions.AddRange(GetAllAttractionsByParentAttractionId(attraction.Id, showHidden, includeAllLevels));
                    }
                    attractions.AddRange(childAttractions);
                }
                return attractions;
            });
        }
        
        /// <summary>
        /// Gets all attractions displayed on the home page
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Attractions</returns>
        public virtual IList<Attraction> GetAllAttractionsDisplayedOnHomePage(bool showHidden = false)
        {
            var query = from c in _attractionRepository.Table
                        orderby c.DisplayOrder
                        where c.Published &&
                        !c.Deleted && 
                        c.ShowOnHomePage
                        select c;

            var attractions = query.ToList();
            if (!showHidden)
            {
                attractions = attractions
                    .Where(c => _aclService.Authorize(c) && _storeMappingService.Authorize(c))
                    .ToList();
            }

            return attractions;
        }
                
        /// <summary>
        /// Gets a attraction
        /// </summary>
        /// <param name="attractionId">Attraction identifier</param>
        /// <returns>Attraction</returns>
        public virtual Attraction GetAttractionById(int attractionId)
        {
            if (attractionId == 0)
                return null;
            
            string key = string.Format(ATTRACTIONS_BY_ID_KEY, attractionId);
            return _cacheManager.Get(key, () => _attractionRepository.GetById(attractionId));
        }

        /// <summary>
        /// Inserts attraction
        /// </summary>
        /// <param name="attraction">Attraction</param>
        public virtual void InsertAttraction(Attraction attraction)
        {
            if (attraction == null)
                throw new ArgumentNullException("attraction");

            _attractionRepository.Insert(attraction);

            //cache
            _cacheManager.RemoveByPattern(ATTRACTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRACTIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(attraction);
        }

        /// <summary>
        /// Updates the attraction
        /// </summary>
        /// <param name="attraction">Attraction</param>
        public virtual void UpdateAttraction(Attraction attraction)
        {
            if (attraction == null)
                throw new ArgumentNullException("attraction");

            //validate attraction hierarchy
            var parentAttraction = GetAttractionById(attraction.ParentAttractionId);
            while (parentAttraction != null)
            {
                if (attraction.Id == parentAttraction.Id)
                {
                    attraction.ParentAttractionId = 0;
                    break;
                }
                parentAttraction = GetAttractionById(parentAttraction.ParentAttractionId);
            }

            _attractionRepository.Update(attraction);

            //cache
            _cacheManager.RemoveByPattern(ATTRACTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRACTIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(attraction);
        }
        
        
        /// <summary>
        /// Deletes a product attraction mapping
        /// </summary>
        /// <param name="productAttraction">Product attraction</param>
        public virtual void DeleteProductAttraction(ProductAttraction productAttraction)
        {
            if (productAttraction == null)
                throw new ArgumentNullException("productAttraction");

            _productAttractionRepository.Delete(productAttraction);

            //cache
            _cacheManager.RemoveByPattern(ATTRACTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRACTIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productAttraction);
        }

        /// <summary>
        /// Gets product attraction mapping attraction
        /// </summary>
        /// <param name="attractionId">Attraction identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product a attraction mapping attraction</returns>
        public virtual IPagedList<ProductAttraction> GetProductAttractionsByAttractionId(int attractionId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (attractionId == 0)
                return new PagedList<ProductAttraction>(new List<ProductAttraction>(), pageIndex, pageSize);

            string key = string.Format(PRODUCTATTRACTIONS_ALLBYCOLLECTIONID_KEY, showHidden, attractionId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in _productAttractionRepository.Table
                            join p in _productRepository.Table on pc.ProductId equals p.Id
                            where pc.AttractionId == attractionId &&
                                  !p.Deleted &&
                                  (showHidden || p.Published)
                            orderby pc.DisplayOrder
                            select pc;

                if (!showHidden && (!_catalogSettings.IgnoreAcl || !_catalogSettings.IgnoreStoreLimitations))
                {
                    if (!_catalogSettings.IgnoreAcl)
                    {
                        //ACL (access control list)
                        var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                        query = from pc in query
                                join c in _attractionRepository.Table on pc.AttractionId equals c.Id
                                join acl in _aclRepository.Table
                                on new { c1 = c.Id, c2 = "Attraction" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
                                from acl in c_acl.DefaultIfEmpty()
                                where !c.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                                select pc;
                    }
                    if (!_catalogSettings.IgnoreStoreLimitations)
                    {
                        //Store mapping
                        var currentStoreId = _storeContext.CurrentStore.Id;
                        query = from pc in query
                                join c in _attractionRepository.Table on pc.AttractionId equals c.Id
                                join sm in _storeMappingRepository.Table
                                on new { c1 = c.Id, c2 = "Attraction" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
                                from sm in c_sm.DefaultIfEmpty()
                                where !c.LimitedToStores || currentStoreId == sm.StoreId
                                select pc;
                    }
                    //only distinct attractions (group by ID)
                    query = from c in query
                            group c by c.Id
                            into cGroup
                            orderby cGroup.Key
                            select cGroup.FirstOrDefault();
                    query = query.OrderBy(pc => pc.DisplayOrder);
                }

                var productAttractions = new PagedList<ProductAttraction>(query, pageIndex, pageSize);
                return productAttractions;
            });
        }

        /// <summary>
        /// Gets a product attraction mapping attraction
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns> Product attraction mapping attraction</returns>
        public virtual IList<ProductAttraction> GetProductAttractionsByProductId(int productId, bool showHidden = false)
        {
            return GetProductAttractionsByProductId(productId, _storeContext.CurrentStore.Id, showHidden);
        }
        /// <summary>
        /// Gets a product attraction mapping attraction
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier (used in multi-store environment). "showHidden" parameter should also be "true"</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns> Product attraction mapping attraction</returns>
        public virtual IList<ProductAttraction> GetProductAttractionsByProductId(int productId, int storeId, bool showHidden = false)
        {
            if (productId == 0)
                return new List<ProductAttraction>();

            string key = string.Format(PRODUCTATTRACTIONS_ALLBYPRODUCTID_KEY, showHidden, productId, _workContext.CurrentCustomer.Id, storeId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in _productAttractionRepository.Table
                            join c in _attractionRepository.Table on pc.AttractionId equals c.Id
                            where pc.ProductId == productId &&
                                  !c.Deleted &&
                                  (showHidden || c.Published)
                            orderby pc.DisplayOrder
                            select pc;

                var allProductAttractions = query.ToList();
                var result = new List<ProductAttraction>();
                if (!showHidden)
                {
                    foreach (var pc in allProductAttractions)
                    {
                        //ACL (access control list) and store mapping
                        var attraction = pc.Attraction;
                        if (_aclService.Authorize(attraction) && _storeMappingService.Authorize(attraction, storeId))
                            result.Add(pc);
                    }
                }
                else
                {
                    //no filtering
                    result.AddRange(allProductAttractions);
                }
                return result;
            });
        }

        /// <summary>
        /// Gets a product attraction mapping 
        /// </summary>
        /// <param name="productAttractionId">Product attraction mapping identifier</param>
        /// <returns>Product attraction mapping</returns>
        public virtual ProductAttraction GetProductAttractionById(int productAttractionId)
        {
            if (productAttractionId == 0)
                return null;

            return _productAttractionRepository.GetById(productAttractionId);
        }

        /// <summary>
        /// Inserts a product attraction mapping
        /// </summary>
        /// <param name="productAttraction">>Product attraction mapping</param>
        public virtual void InsertProductAttraction(ProductAttraction productAttraction)
        {
            if (productAttraction == null)
                throw new ArgumentNullException("productAttraction");
            
            _productAttractionRepository.Insert(productAttraction);

            //cache
            _cacheManager.RemoveByPattern(ATTRACTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRACTIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productAttraction);
        }

        /// <summary>
        /// Updates the product attraction mapping 
        /// </summary>
        /// <param name="productAttraction">>Product attraction mapping</param>
        public virtual void UpdateProductAttraction(ProductAttraction productAttraction)
        {
            if (productAttraction == null)
                throw new ArgumentNullException("productAttraction");

            _productAttractionRepository.Update(productAttraction);

            //cache
            _cacheManager.RemoveByPattern(ATTRACTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRACTIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productAttraction);
        }

        #endregion
    }
}
