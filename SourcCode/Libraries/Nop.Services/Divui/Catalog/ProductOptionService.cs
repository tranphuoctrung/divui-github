using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Divui.Catalog;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Services.Events;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Divui.Catalog
{
    public partial class ProductOptionService : IProductOptionService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : productOption ID
        /// </remarks>
        private const string PRODUCTOPTIONS_BY_ID_KEY = "Nop.productOption.id-{0}";
       
       
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTOPTIONS_PATTERN_KEY = "Nop.productOption.";
       

        #endregion

        #region Fields

        private readonly IRepository<ProductOption> _productOptionRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="productOptionRepository">ProductOption repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="storeMappingRepository">Store mapping repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public ProductOptionService(ICacheManager cacheManager,
            IRepository<ProductOption> productOptionRepository,
            IRepository<Product> productRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IWorkContext workContext,
            IStoreContext storeContext,
            IEventPublisher eventPublisher,
            CatalogSettings catalogSettings)
        {
            this._cacheManager = cacheManager;
            this._productOptionRepository = productOptionRepository;
            this._productRepository = productRepository;
            this._storeMappingRepository = storeMappingRepository;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._eventPublisher = eventPublisher;
            this._catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete productOption
        /// </summary>
        /// <param name="productOption">ProductOption</param>
        public virtual void DeleteProductOption(ProductOption productOption)
        {
            if (productOption == null)
                throw new ArgumentNullException("productOption");

            productOption.Deleted = true;
            UpdateProductOption(productOption);
        }

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="productOptionName">ProductOption name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>ProductOptions</returns>
        public virtual IPagedList<ProductOption> GetAllProductOptions(string productOptionName = "",
            int pageIndex = 0, int pageSize = int.MaxValue, int productId = 0, bool showHidden = false)
        {
            var query = _productOptionRepository.Table;
            if (!showHidden)
                query = query.Where(c => c.Published);
            if (!String.IsNullOrWhiteSpace(productOptionName))
                query = query.Where(c => c.Name.Contains(productOptionName));
            if (productId > 0)
                query = query.Where(c => c.ProductId == productId);

            query = query.Where(c => !c.Deleted);
            query = query.OrderBy(c => c.DisplayOrder);

            var sortedProductOptions = query.ToList();

            //paging
            return new PagedList<ProductOption>(sortedProductOptions, pageIndex, pageSize);
        }
        
        /// <summary>
        /// Gets a productOption
        /// </summary>
        /// <param name="productOptionId">ProductOption identifier</param>
        /// <returns>ProductOption</returns>
        public virtual ProductOption GetProductOptionById(int productOptionId)
        {
            if (productOptionId == 0)
                return null;

            string key = string.Format(PRODUCTOPTIONS_BY_ID_KEY, productOptionId);
            return _cacheManager.Get(key, () => _productOptionRepository.GetById(productOptionId));
        }

        /// <summary>
        /// Inserts productOption
        /// </summary>
        /// <param name="productOption">ProductOption</param>
        public virtual void InsertProductOption(ProductOption productOption)
        {
            if (productOption == null)
                throw new ArgumentNullException("productOption");

            _productOptionRepository.Insert(productOption);

            //cache
            _cacheManager.RemoveByPattern(PRODUCTOPTIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productOption);
        }

        /// <summary>
        /// Updates the productOption
        /// </summary>
        /// <param name="productOption">ProductOption</param>
        public virtual void UpdateProductOption(ProductOption productOption)
        {
            if (productOption == null)
                throw new ArgumentNullException("productOption");
            
            _productOptionRepository.Update(productOption);

            //cache
            _cacheManager.RemoveByPattern(PRODUCTOPTIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productOption);
        }
        
        #endregion
    }
}
