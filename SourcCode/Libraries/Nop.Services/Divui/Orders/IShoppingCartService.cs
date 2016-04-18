using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Orders
{
    public partial interface IShoppingCartService
    {
        /// <summary>
        /// Deletes a shopping cart attribute mapping
        /// </summary>
        /// <param name="shoppingCartItemAttributeMapping">Shopping cart attribute mapping</param>
       void DeleteShoppingCartItemAttributeMapping(ShoppingCartItemAttributeMapping shoppingCartItemAttributeMapping);
        /// <summary>
        /// Gets shopping cart attribute mappings by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>Shopping cart attribute mapping collection</returns>
        IList<ShoppingCartItemAttributeMapping> GetShoppingCartItemAttributeMappingsByShoppingCartItemId(int shoppingCartItemId);

        /// <summary>
        /// Gets a shopping cart attribute mapping
        /// </summary>
        /// <param name="shoppingCartItemAttributeMappingId">Shopping cart attribute mapping identifier</param>
        /// <returns>Shopping cart attribute mapping</returns>
        ShoppingCartItemAttributeMapping GetShoppingCartItemAttributeMappingById(int shoppingCartItemAttributeMappingId);

        /// <summary>
        /// Inserts a shopping cart attribute mapping
        /// </summary>
        /// <param name="shoppingCartItemAttributeMapping">The shopping cart attribute mapping</param>
        void InsertShoppingCartItemAttributeMapping(ShoppingCartItemAttributeMapping shoppingCartItemAttributeMapping);

        /// <summary>
        /// Updates the shopping cart attribute mapping
        /// </summary>
        /// <param name="shoppingCartItemAttributeMapping">The shopping cart attribute mapping</param>
        void UpdateShoppingCartItemAttributeMapping(ShoppingCartItemAttributeMapping shoppingCartItemAttributeMapping);


        /// <summary>
        /// Gets a shopping cart attribute mapping
        /// </summary>
        /// <param name="cartId">Shopping cart item identifier</param>
        /// <param name="attributeId">attribute identifier</param>
        /// <returns>Shopping cart attribute mapping</returns>
        ShoppingCartItemAttributeMapping GetShoppingCartItemAttributeMappingByCartIdAndAttributeId(int cartId, int attributeId);


        /// <summary>
        /// Add a product to shopping cart
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="product">Product</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">The price enter by a customer</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="automaticallyAddRequiredProductsIfEnabled">Automatically add required products if enabled</param>
        /// <returns>Warnings</returns>
        IList<string> AddToCart(List<ShoppingCartItemAttributeMapping> shoppingCartItemAttributeMappings, Customer customer, Product product,
            ShoppingCartType shoppingCartType, int storeId, string attributesXml = null,
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool automaticallyAddRequiredProductsIfEnabled = true);

        /// <summary>
        /// Updates the shopping cart item
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartItemId">Shopping cart item identifier</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">New customer entered price</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        /// <param name="quantity">New shopping cart item quantity</param>
        /// <param name="resetCheckoutData">A value indicating whether to reset checkout data</param>
        /// <returns>Warnings</returns>
        IList<string> UpdateShoppingCartItem(List<ShoppingCartItemAttributeMapping> shoppingCartItemAttributeMappings, Customer customer,
            int shoppingCartItemId, string attributesXml,
            decimal customerEnteredPrice,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool resetCheckoutData = true);
    }
}
