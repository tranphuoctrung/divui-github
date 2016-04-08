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
    /// Collection service
    /// </summary>
    public partial class CollectionService : ICollectionService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : collection ID
        /// </remarks>
        private const string COLLECTIONS_BY_ID_KEY = "Nop.collection.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : parent collection ID
        /// {1} : show hidden records?
        /// {2} : current customer ID
        /// {3} : store ID
        /// {3} : include all levels (child)
        /// </remarks>
        private const string COLLECTIONS_BY_PARENT_COLLECTION_ID_KEY = "Nop.collection.byparent-{0}-{1}-{2}-{3}-{4}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : collection ID
        /// {2} : page index
        /// {3} : page size
        /// {4} : current customer ID
        /// {5} : store ID
        /// </remarks>
        private const string PRODUCTCOLLECTIONS_ALLBYCOLLECTIONID_KEY = "Nop.productcollection.allbycollectionid-{0}-{1}-{2}-{3}-{4}-{5}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : product ID
        /// {2} : current customer ID
        /// {3} : store ID
        /// </remarks>
        private const string PRODUCTCOLLECTIONS_ALLBYPRODUCTID_KEY = "Nop.productcollection.allbyproductid-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string COLLECTIONS_PATTERN_KEY = "Nop.collection.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTCOLLECTIONS_PATTERN_KEY = "Nop.productcollection.";

        #endregion

        #region Fields

        private readonly IRepository<Collection> _collectionRepository;
        private readonly IRepository<ProductCollection> _productCollectionRepository;
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
        /// <param name="collectionRepository">Collection repository</param>
        /// <param name="productCollectionRepository">ProductCollection repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="aclRepository">ACL record repository</param>
        /// <param name="storeMappingRepository">Store mapping repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public CollectionService(ICacheManager cacheManager,
            IRepository<Collection> collectionRepository,
            IRepository<ProductCollection> productCollectionRepository,
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
            this._collectionRepository = collectionRepository;
            this._productCollectionRepository = productCollectionRepository;
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
        /// Delete collection
        /// </summary>
        /// <param name="collection">Collection</param>
        public virtual void DeleteCollection(Collection collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            collection.Deleted = true;
            UpdateCollection(collection);

            //reset a "Parent collection" property of all child subcollections
            var subcollections = GetAllCollectionsByParentCollectionId(collection.Id, true);
            foreach (var subcollection in subcollections)
            {
                subcollection.ParentCollectionId = 0;
                UpdateCollection(subcollection);
            }
        }
        
        /// <summary>
        /// Gets all collections
        /// </summary>
        /// <param name="collectionName">Collection name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Collections</returns>
        public virtual IPagedList<Collection> GetAllCollections(string collectionName = "", 
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = _collectionRepository.Table;
            if (!showHidden)
                query = query.Where(c => c.Published);
            if (!String.IsNullOrWhiteSpace(collectionName))
                query = query.Where(c => c.Name.Contains(collectionName));
            query = query.Where(c => !c.Deleted);
            query = query.OrderBy(c => c.ParentCollectionId).ThenBy(c => c.DisplayOrder);
            
            if (!showHidden && (!_catalogSettings.IgnoreAcl || !_catalogSettings.IgnoreStoreLimitations))
            {
                if (!_catalogSettings.IgnoreAcl)
                {
                    //ACL (access control list)
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                    query = from c in query
                            join acl in _aclRepository.Table
                            on new { c1 = c.Id, c2 = "Collection" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
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
                            on new { c1 = c.Id, c2 = "Collection" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
                            from sm in c_sm.DefaultIfEmpty()
                            where !c.LimitedToStores || currentStoreId == sm.StoreId
                            select c;
                }

                //only distinct collections (group by ID)
                query = from c in query
                        group c by c.Id
                        into cGroup
                        orderby cGroup.Key
                        select cGroup.FirstOrDefault();
                query = query.OrderBy(c => c.ParentCollectionId).ThenBy(c => c.DisplayOrder);
            }
            
            var unsortedCollections = query.ToList();

            //sort collections
            var sortedCollections = unsortedCollections.SortCollectionsForTree();

            //paging
            return new PagedList<Collection>(sortedCollections, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all collections filtered by parent collection identifier
        /// </summary>
        /// <param name="parentCollectionId">Parent collection identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="includeAllLevels">A value indicating whether we should load all child levels</param>
        /// <returns>Collections</returns>
        public virtual IList<Collection> GetAllCollectionsByParentCollectionId(int parentCollectionId,
            bool showHidden = false, bool includeAllLevels = false)
        {
            string key = string.Format(COLLECTIONS_BY_PARENT_COLLECTION_ID_KEY, parentCollectionId, showHidden, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id, includeAllLevels);
            return _cacheManager.Get(key, () =>
            {
                var query = _collectionRepository.Table;
                if (!showHidden)
                    query = query.Where(c => c.Published);
                query = query.Where(c => c.ParentCollectionId == parentCollectionId);
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
                                on new { c1 = c.Id, c2 = "Collection" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
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
                                on new { c1 = c.Id, c2 = "Collection" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
                                from sm in c_sm.DefaultIfEmpty()
                                where !c.LimitedToStores || currentStoreId == sm.StoreId
                                select c;
                    }
                    //only distinct collections (group by ID)
                    query = from c in query
                            group c by c.Id
                            into cGroup
                            orderby cGroup.Key
                            select cGroup.FirstOrDefault();
                    query = query.OrderBy(c => c.DisplayOrder);
                }

                var collections = query.ToList();
                if (includeAllLevels)
                {
                    var childCollections = new List<Collection>();
                    //add child levels
                    foreach (var collection in collections)
                    {
                        childCollections.AddRange(GetAllCollectionsByParentCollectionId(collection.Id, showHidden, includeAllLevels));
                    }
                    collections.AddRange(childCollections);
                }
                return collections;
            });
        }
        
        /// <summary>
        /// Gets all collections displayed on the home page
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Collections</returns>
        public virtual IList<Collection> GetAllCollectionsDisplayedOnHomePage(bool showHidden = false)
        {
            var query = from c in _collectionRepository.Table
                        orderby c.DisplayOrder
                        where c.Published &&
                        !c.Deleted && 
                        c.ShowOnHomePage
                        select c;

            var collections = query.ToList();
            if (!showHidden)
            {
                collections = collections
                    .Where(c => _aclService.Authorize(c) && _storeMappingService.Authorize(c))
                    .ToList();
            }

            return collections;
        }
                
        /// <summary>
        /// Gets a collection
        /// </summary>
        /// <param name="collectionId">Collection identifier</param>
        /// <returns>Collection</returns>
        public virtual Collection GetCollectionById(int collectionId)
        {
            if (collectionId == 0)
                return null;
            
            string key = string.Format(COLLECTIONS_BY_ID_KEY, collectionId);
            return _cacheManager.Get(key, () => _collectionRepository.GetById(collectionId));
        }

        /// <summary>
        /// Inserts collection
        /// </summary>
        /// <param name="collection">Collection</param>
        public virtual void InsertCollection(Collection collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            _collectionRepository.Insert(collection);

            //cache
            _cacheManager.RemoveByPattern(COLLECTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTCOLLECTIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(collection);
        }

        /// <summary>
        /// Updates the collection
        /// </summary>
        /// <param name="collection">Collection</param>
        public virtual void UpdateCollection(Collection collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            //validate collection hierarchy
            var parentCollection = GetCollectionById(collection.ParentCollectionId);
            while (parentCollection != null)
            {
                if (collection.Id == parentCollection.Id)
                {
                    collection.ParentCollectionId = 0;
                    break;
                }
                parentCollection = GetCollectionById(parentCollection.ParentCollectionId);
            }

            _collectionRepository.Update(collection);

            //cache
            _cacheManager.RemoveByPattern(COLLECTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTCOLLECTIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(collection);
        }
        
        
        /// <summary>
        /// Deletes a product collection mapping
        /// </summary>
        /// <param name="productCollection">Product collection</param>
        public virtual void DeleteProductCollection(ProductCollection productCollection)
        {
            if (productCollection == null)
                throw new ArgumentNullException("productCollection");

            _productCollectionRepository.Delete(productCollection);

            //cache
            _cacheManager.RemoveByPattern(COLLECTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTCOLLECTIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productCollection);
        }

        /// <summary>
        /// Gets product collection mapping collection
        /// </summary>
        /// <param name="collectionId">Collection identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product a collection mapping collection</returns>
        public virtual IPagedList<ProductCollection> GetProductCollectionsByCollectionId(int collectionId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (collectionId == 0)
                return new PagedList<ProductCollection>(new List<ProductCollection>(), pageIndex, pageSize);

            string key = string.Format(PRODUCTCOLLECTIONS_ALLBYCOLLECTIONID_KEY, showHidden, collectionId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in _productCollectionRepository.Table
                            join p in _productRepository.Table on pc.ProductId equals p.Id
                            where pc.CollectionId == collectionId &&
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
                                join c in _collectionRepository.Table on pc.CollectionId equals c.Id
                                join acl in _aclRepository.Table
                                on new { c1 = c.Id, c2 = "Collection" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
                                from acl in c_acl.DefaultIfEmpty()
                                where !c.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                                select pc;
                    }
                    if (!_catalogSettings.IgnoreStoreLimitations)
                    {
                        //Store mapping
                        var currentStoreId = _storeContext.CurrentStore.Id;
                        query = from pc in query
                                join c in _collectionRepository.Table on pc.CollectionId equals c.Id
                                join sm in _storeMappingRepository.Table
                                on new { c1 = c.Id, c2 = "Collection" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
                                from sm in c_sm.DefaultIfEmpty()
                                where !c.LimitedToStores || currentStoreId == sm.StoreId
                                select pc;
                    }
                    //only distinct collections (group by ID)
                    query = from c in query
                            group c by c.Id
                            into cGroup
                            orderby cGroup.Key
                            select cGroup.FirstOrDefault();
                    query = query.OrderBy(pc => pc.DisplayOrder);
                }

                var productCollections = new PagedList<ProductCollection>(query, pageIndex, pageSize);
                return productCollections;
            });
        }

        /// <summary>
        /// Gets a product collection mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns> Product collection mapping collection</returns>
        public virtual IList<ProductCollection> GetProductCollectionsByProductId(int productId, bool showHidden = false)
        {
            return GetProductCollectionsByProductId(productId, _storeContext.CurrentStore.Id, showHidden);
        }
        /// <summary>
        /// Gets a product collection mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier (used in multi-store environment). "showHidden" parameter should also be "true"</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns> Product collection mapping collection</returns>
        public virtual IList<ProductCollection> GetProductCollectionsByProductId(int productId, int storeId, bool showHidden = false)
        {
            if (productId == 0)
                return new List<ProductCollection>();

            string key = string.Format(PRODUCTCOLLECTIONS_ALLBYPRODUCTID_KEY, showHidden, productId, _workContext.CurrentCustomer.Id, storeId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in _productCollectionRepository.Table
                            join c in _collectionRepository.Table on pc.CollectionId equals c.Id
                            where pc.ProductId == productId &&
                                  !c.Deleted &&
                                  (showHidden || c.Published)
                            orderby pc.DisplayOrder
                            select pc;

                var allProductCollections = query.ToList();
                var result = new List<ProductCollection>();
                if (!showHidden)
                {
                    foreach (var pc in allProductCollections)
                    {
                        //ACL (access control list) and store mapping
                        var collection = pc.Collection;
                        if (_aclService.Authorize(collection) && _storeMappingService.Authorize(collection, storeId))
                            result.Add(pc);
                    }
                }
                else
                {
                    //no filtering
                    result.AddRange(allProductCollections);
                }
                return result;
            });
        }

        /// <summary>
        /// Gets a product collection mapping 
        /// </summary>
        /// <param name="productCollectionId">Product collection mapping identifier</param>
        /// <returns>Product collection mapping</returns>
        public virtual ProductCollection GetProductCollectionById(int productCollectionId)
        {
            if (productCollectionId == 0)
                return null;

            return _productCollectionRepository.GetById(productCollectionId);
        }

        /// <summary>
        /// Inserts a product collection mapping
        /// </summary>
        /// <param name="productCollection">>Product collection mapping</param>
        public virtual void InsertProductCollection(ProductCollection productCollection)
        {
            if (productCollection == null)
                throw new ArgumentNullException("productCollection");
            
            _productCollectionRepository.Insert(productCollection);

            //cache
            _cacheManager.RemoveByPattern(COLLECTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTCOLLECTIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productCollection);
        }

        /// <summary>
        /// Updates the product collection mapping 
        /// </summary>
        /// <param name="productCollection">>Product collection mapping</param>
        public virtual void UpdateProductCollection(ProductCollection productCollection)
        {
            if (productCollection == null)
                throw new ArgumentNullException("productCollection");

            _productCollectionRepository.Update(productCollection);

            //cache
            _cacheManager.RemoveByPattern(COLLECTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTCOLLECTIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productCollection);
        }

        #endregion
    }
}
