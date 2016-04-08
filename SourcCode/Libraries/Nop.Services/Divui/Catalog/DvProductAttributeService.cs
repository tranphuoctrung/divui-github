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
    public partial class ProductAttributeService
    {
        #region Constants


        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        private const string ORDERITEMATTRIBUTEMAPPINGS_ALL_KEY = "Nop.orderitemattributemapping.all-{0}";

        #endregion
        #region Fields


        private readonly IRepository<OrderItemAttributeMapping> _orderItemAttributeMappingRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="productAttributeRepository">Product attribute repository</param>
        /// <param name="productAttributeMappingRepository">Product attribute mapping repository</param>
        /// <param name="productAttributeCombinationRepository">Product attribute combination repository</param>
        /// <param name="productAttributeValueRepository">Product attribute value repository</param>
        /// <param name="predefinedProductAttributeValueRepository">Predefined product attribute value repository</param>
        /// <param name="eventPublisher">Event published</param>
        public ProductAttributeService(ICacheManager cacheManager,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<ProductAttributeMapping> productAttributeMappingRepository,
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
            IRepository<ProductAttributeValue> productAttributeValueRepository,
            IRepository<PredefinedProductAttributeValue> predefinedProductAttributeValueRepository,
            IEventPublisher eventPublisher,
            IRepository<OrderItemAttributeMapping> orderItemAttributeMappingRepository)
        {
            this._cacheManager = cacheManager;
            this._productAttributeRepository = productAttributeRepository;
            this._productAttributeMappingRepository = productAttributeMappingRepository;
            this._productAttributeCombinationRepository = productAttributeCombinationRepository;
            this._productAttributeValueRepository = productAttributeValueRepository;
            this._predefinedProductAttributeValueRepository = predefinedProductAttributeValueRepository;
            this._eventPublisher = eventPublisher;
            this._orderItemAttributeMappingRepository = orderItemAttributeMappingRepository;
        }

        #endregion

        #region Product attributes mappings

        /// <summary>
        /// Deletes a product attribute mapping
        /// </summary>
        /// <param name="orderItemAttributeMapping">Product attribute mapping</param>
        public virtual void DeleteOrderItemAttributeMapping(OrderItemAttributeMapping orderItemAttributeMapping)
        {
            if (orderItemAttributeMapping == null)
                throw new ArgumentNullException("orderItemAttributeMapping");

            _orderItemAttributeMappingRepository.Delete(orderItemAttributeMapping);

            //cache
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(orderItemAttributeMapping);
        }

        /// <summary>
        /// Gets product attribute mappings by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>Product attribute mapping collection</returns>
        public virtual IList<OrderItemAttributeMapping> GetOrderItemAttributeMappingsByOrderItemId(int orderItemId)
        {

            var query = from pam in _orderItemAttributeMappingRepository.Table
                        orderby pam.DisplayOrder
                        where pam.OrderItemId == orderItemId
                        select pam;
            var orderItemAttributeMappings = query.ToList();
            return orderItemAttributeMappings;
        }

        /// <summary>
        /// Gets a product attribute mapping
        /// </summary>
        /// <param name="orderItemAttributeMappingId">Product attribute mapping identifier</param>
        /// <returns>Product attribute mapping</returns>
        public virtual OrderItemAttributeMapping GetOrderItemAttributeMappingById(int orderItemAttributeMappingId)
        {
            if (orderItemAttributeMappingId == 0)
                return null;

            return _orderItemAttributeMappingRepository.GetById(orderItemAttributeMappingId);
        }

        /// <summary>
        /// Inserts a product attribute mapping
        /// </summary>
        /// <param name="orderItemAttributeMapping">The product attribute mapping</param>
        public virtual void InsertOrderItemAttributeMapping(OrderItemAttributeMapping orderItemAttributeMapping)
        {
            if (orderItemAttributeMapping == null)
                throw new ArgumentNullException("orderItemAttributeMapping");

            _orderItemAttributeMappingRepository.Insert(orderItemAttributeMapping);

          
            //event notification
            _eventPublisher.EntityInserted(orderItemAttributeMapping);
        }

        /// <summary>
        /// Updates the product attribute mapping
        /// </summary>
        /// <param name="orderItemAttributeMapping">The product attribute mapping</param>
        public virtual void UpdateOrderItemAttributeMapping(OrderItemAttributeMapping orderItemAttributeMapping)
        {
            if (orderItemAttributeMapping == null)
                throw new ArgumentNullException("orderItemAttributeMapping");

            _orderItemAttributeMappingRepository.Update(orderItemAttributeMapping);


            //event notification
            _eventPublisher.EntityUpdated(orderItemAttributeMapping);
        }

        #endregion
    }
}
