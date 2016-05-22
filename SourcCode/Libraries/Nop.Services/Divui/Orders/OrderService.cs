using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Nop.Services.Orders
{
    public partial class OrderService
    {
        /// <summary>
        /// Gets all orders by customer identifier
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Order collection</returns>
        public virtual IList<Order> GetOrdersByCustomerId(int customerId)
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                var query = from o in _orderRepository.Table
                            orderby o.CreatedOnUtc descending
                            where !o.Deleted && o.CustomerId == customerId
                            select o;
                var orders = query.ToList();
                return orders;
            }
        }

    }
}
