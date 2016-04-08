using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Catalog
{
    public partial interface IProductAttributeService
    {
        #region OrderItem attributes mappings

        /// <summary>
        /// Deletes a orderItem attribute mapping
        /// </summary>
        /// <param name="orderItemAttributeMapping">OrderItem attribute mapping</param>
        void DeleteOrderItemAttributeMapping(OrderItemAttributeMapping orderItemAttributeMapping);

        /// <summary>
        /// Gets orderItem attribute mappings by orderItem identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>OrderItem attribute mapping collection</returns>
        IList<OrderItemAttributeMapping> GetOrderItemAttributeMappingsByOrderItemId(int orderItemId);

        /// <summary>
        /// Gets a orderItem attribute mapping
        /// </summary>
        /// <param name="orderItemAttributeMappingId">OrderItem attribute mapping identifier</param>
        /// <returns>OrderItem attribute mapping</returns>
        OrderItemAttributeMapping GetOrderItemAttributeMappingById(int orderItemAttributeMappingId);

        /// <summary>
        /// Inserts a orderItem attribute mapping
        /// </summary>
        /// <param name="orderItemAttributeMapping">The orderItem attribute mapping</param>
        void InsertOrderItemAttributeMapping(OrderItemAttributeMapping orderItemAttributeMapping);

        /// <summary>
        /// Updates the orderItem attribute mapping
        /// </summary>
        /// <param name="orderItemAttributeMapping">The orderItem attribute mapping</param>
        void UpdateOrderItemAttributeMapping(OrderItemAttributeMapping orderItemAttributeMapping);

        #endregion
    }
}
