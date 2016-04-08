using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Caching;
using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;

namespace Nop.Services.Catalog
{
    public partial class AvailabilitySetupService : IAvailabilitySetupService
    {
        

        #region Fields

        private readonly IRepository<AvailabilitySetup> _availabilitySetupRepository;
        private readonly IRepository<Product> _productRepository;
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
        /// <param name="availabilitySetupRepository">ProductOption repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="storeMappingRepository">Store mapping repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public AvailabilitySetupService(ICacheManager cacheManager,
            IRepository<AvailabilitySetup> availabilitySetupRepository,
            IRepository<Product> productRepository,
            IWorkContext workContext,
            IStoreContext storeContext,
            IEventPublisher eventPublisher,
            CatalogSettings catalogSettings)
        {
            this._cacheManager = cacheManager;
            this._availabilitySetupRepository = availabilitySetupRepository;
            this._productRepository = productRepository;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._eventPublisher = eventPublisher;
            this._catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods
        public IList<AvailabilitySetup> GetAllAvailabilitySetups(int productId = 0, List<int> customerRoleIds = null, DateTime? fromDate = default(DateTime?), DateTime? toDate = default(DateTime?))
        {
            var query = _availabilitySetupRepository.Table;

            if (productId > 0)
                query = query.Where(ps => ps.ProductId == productId);

            if (customerRoleIds != null && customerRoleIds.Count > 0)
                query = query.Where(ps => customerRoleIds.Contains(ps.CustomerRoleId));

            if (fromDate.HasValue)
                query = query.Where(ps => ps.FromDate >= fromDate);

            if (toDate.HasValue)
                query = query.Where(ps => ps.ToDate <= toDate);

            return query.ToList();
        }
        #endregion
    }
}
