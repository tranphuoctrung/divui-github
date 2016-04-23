using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Services.Orders
{
    public partial class ShoppingCartService
    {
        #region Fields


        private readonly IRepository<ShoppingCartItemAttributeMapping> _shoppingCartItemAttributeMappingRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sciRepository">Shopping cart repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="currencyService">Currency service</param>
        /// <param name="productService">Product settings</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="productAttributeParser">Product attribute parser</param>
        /// <param name="checkoutAttributeService">Checkout attribute service</param>
        /// <param name="checkoutAttributeParser">Checkout attribute parser</param>
        /// <param name="priceFormatter">Price formatter</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="shoppingCartSettings">Shopping cart settings</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="permissionService">Permission service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="genericAttributeService">Generic attribute service</param>
        /// <param name="productAttributeService">Product attribute service</param>
        /// <param name="dateTimeHelper">Datetime helper</param>
        public ShoppingCartService(IRepository<ShoppingCartItem> sciRepository,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICurrencyService currencyService,
            IProductService productService,
            ILocalizationService localizationService,
            IProductAttributeParser productAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            ICheckoutAttributeParser checkoutAttributeParser,
            IPriceFormatter priceFormatter,
            ICustomerService customerService,
            ShoppingCartSettings shoppingCartSettings,
            IEventPublisher eventPublisher,
            IPermissionService permissionService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IGenericAttributeService genericAttributeService,
            IProductAttributeService productAttributeService,
            IDateTimeHelper dateTimeHelper,
            IRepository<ShoppingCartItemAttributeMapping> shoppingCartItemAttributeMappingRepository)
        {
            this._sciRepository = sciRepository;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._currencyService = currencyService;
            this._productService = productService;
            this._localizationService = localizationService;
            this._productAttributeParser = productAttributeParser;
            this._checkoutAttributeService = checkoutAttributeService;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._priceFormatter = priceFormatter;
            this._customerService = customerService;
            this._shoppingCartSettings = shoppingCartSettings;
            this._eventPublisher = eventPublisher;
            this._permissionService = permissionService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._genericAttributeService = genericAttributeService;
            this._productAttributeService = productAttributeService;
            this._dateTimeHelper = dateTimeHelper;
            this._shoppingCartItemAttributeMappingRepository = shoppingCartItemAttributeMappingRepository;
        }

        #endregion

        #region Shopping cart attributes mappings

        /// <summary>
        /// Deletes a shopping cart attribute mapping
        /// </summary>
        /// <param name="shoppingCartItemAttributeMapping">Shopping cart attribute mapping</param>
        public virtual void DeleteShoppingCartItemAttributeMapping(ShoppingCartItemAttributeMapping shoppingCartItemAttributeMapping)
        {
            if (shoppingCartItemAttributeMapping == null)
                throw new ArgumentNullException("shoppingCartItemAttributeMapping");

            _shoppingCartItemAttributeMappingRepository.Delete(shoppingCartItemAttributeMapping);

            //event notification
            _eventPublisher.EntityDeleted(shoppingCartItemAttributeMapping);
        }

        /// <summary>
        /// Gets shopping cart attribute mappings by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>Shopping cart attribute mapping collection</returns>
        public virtual IList<ShoppingCartItemAttributeMapping> GetShoppingCartItemAttributeMappingsByShoppingCartItemId(int shoppingCartItemId)
        {

            var query = from pam in _shoppingCartItemAttributeMappingRepository.Table
                        orderby pam.DisplayOrder
                        where pam.ShoppingCartItemId == shoppingCartItemId
                        select pam;
            var shoppingCartItemAttributeMappings = query.ToList();
            return shoppingCartItemAttributeMappings;
        }

        /// <summary>
        /// Gets a shopping cart attribute mapping
        /// </summary>
        /// <param name="shoppingCartItemAttributeMappingId">Shopping cart attribute mapping identifier</param>
        /// <returns>Shopping cart attribute mapping</returns>
        public virtual ShoppingCartItemAttributeMapping GetShoppingCartItemAttributeMappingById(int shoppingCartItemAttributeMappingId)
        {
            if (shoppingCartItemAttributeMappingId == 0)
                return null;

            return _shoppingCartItemAttributeMappingRepository.GetById(shoppingCartItemAttributeMappingId);
        }

        /// <summary>
        /// Inserts a shopping cart attribute mapping
        /// </summary>
        /// <param name="shoppingCartItemAttributeMapping">The shopping cart attribute mapping</param>
        public virtual void InsertShoppingCartItemAttributeMapping(ShoppingCartItemAttributeMapping shoppingCartItemAttributeMapping)
        {
            if (shoppingCartItemAttributeMapping == null)
                throw new ArgumentNullException("shoppingCartItemAttributeMapping");

            _shoppingCartItemAttributeMappingRepository.Insert(shoppingCartItemAttributeMapping);


            //event notification
            _eventPublisher.EntityInserted(shoppingCartItemAttributeMapping);
        }

        /// <summary>
        /// Updates the shopping cart attribute mapping
        /// </summary>
        /// <param name="shoppingCartItemAttributeMapping">The shopping cart attribute mapping</param>
        public virtual void UpdateShoppingCartItemAttributeMapping(ShoppingCartItemAttributeMapping shoppingCartItemAttributeMapping)
        {
            if (shoppingCartItemAttributeMapping == null)
                throw new ArgumentNullException("shoppingCartItemAttributeMapping");

            _shoppingCartItemAttributeMappingRepository.Update(shoppingCartItemAttributeMapping);


            //event notification
            _eventPublisher.EntityUpdated(shoppingCartItemAttributeMapping);
        }

        /// <summary>
        /// Gets a shopping cart attribute mapping
        /// </summary>
        /// <param name="cartId">Shopping cart item identifier</param>
        /// <param name="attributeId">attribute identifier</param>
        /// <returns>Shopping cart attribute mapping</returns>
        public virtual ShoppingCartItemAttributeMapping GetShoppingCartItemAttributeMappingByCartIdAndAttributeId(int cartId, int attributeId)
        {
            if (cartId == 0 || attributeId == 0)
                return null;

            return _shoppingCartItemAttributeMappingRepository.Table.SingleOrDefault(s => s.ProductAttributeId == attributeId && s.ShoppingCartItemId == cartId);

        }
        #endregion


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
        public virtual IList<string> AddToCart(List<ShoppingCartItemAttributeMapping> shoppingCartItemAttributeMappings, Customer customer, Product product,
            ShoppingCartType shoppingCartType, int storeId, string attributesXml = null,
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool automaticallyAddRequiredProductsIfEnabled = true)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (product == null)
                throw new ArgumentNullException("product");

            var warnings = new List<string>();
            if (shoppingCartType == ShoppingCartType.ShoppingCart && !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, customer))
            {
                warnings.Add("Shopping cart is disabled");
                return warnings;
            }
            if (shoppingCartType == ShoppingCartType.Wishlist && !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist, customer))
            {
                warnings.Add("Wishlist is disabled");
                return warnings;
            }
            if (customer.IsSearchEngineAccount())
            {
                warnings.Add("Search engine can't add to cart");
                return warnings;
            }

            if (quantity <= 0)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));
                return warnings;
            }

            //reset checkout info
            _customerService.ResetCheckoutData(customer, storeId);

            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == shoppingCartType)
                .LimitPerStore(storeId)
                .ToList();

            var shoppingCartItem = FindShoppingCartItemInTheCart(cart,
                shoppingCartType, product, attributesXml, customerEnteredPrice,
                rentalStartDate, rentalEndDate);

            if (shoppingCartItem != null)
            {
                //update existing shopping cart item
                int newQuantity = shoppingCartItem.Quantity + quantity;
                
                switch (shoppingCartType)
                {
                    case ShoppingCartType.ShoppingCart:
                        {
                            warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, product,
                                storeId, attributesXml,
                                customerEnteredPrice, rentalStartDate, rentalEndDate,
                                newQuantity, automaticallyAddRequiredProductsIfEnabled));
                        }
                        break;
                    case ShoppingCartType.Wishlist:
                        {
                            warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, product,
                                storeId, attributesXml,
                                customerEnteredPrice, rentalStartDate, rentalEndDate,
                                newQuantity, automaticallyAddRequiredProductsIfEnabled, false, false, false, false, false));
                        }
                        break;
                    default:
                        break;
                }

                if (warnings.Count == 0)
                {
                    shoppingCartItem.AttributesXml = attributesXml;
                    shoppingCartItem.Quantity = newQuantity;
                    shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                    _customerService.UpdateCustomer(customer);

                    //event notification
                    _eventPublisher.EntityUpdated(shoppingCartItem);
                }
            }
            else
            {
                
                //maximum items validation
                switch (shoppingCartType)
                {
                    case ShoppingCartType.ShoppingCart:
                        {
                            //new shopping cart item
                            warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, product,
                                storeId, attributesXml, customerEnteredPrice,
                                rentalStartDate, rentalEndDate,
                                quantity, automaticallyAddRequiredProductsIfEnabled, getAttributesWarnings: false));


                            if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                            {
                                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumShoppingCartItems"), _shoppingCartSettings.MaximumShoppingCartItems));
                                return warnings;
                            }
                        }
                        break;
                    case ShoppingCartType.Wishlist:
                        {
                            //new shopping cart item
                            warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, product,
                                storeId, attributesXml, customerEnteredPrice,
                                rentalStartDate, rentalEndDate,
                                quantity, automaticallyAddRequiredProductsIfEnabled,false, false, false, false, false));


                            if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                            {
                                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumWishlistItems"), _shoppingCartSettings.MaximumWishlistItems));
                                return warnings;
                            }
                        }
                        break;
                    default:
                        break;
                }

                if (warnings.Count == 0)
                {
                   
                    DateTime now = DateTime.UtcNow;
                    shoppingCartItem = new ShoppingCartItem
                    {
                        ShoppingCartType = shoppingCartType,
                        StoreId = storeId,
                        Product = product,
                        AttributesXml = attributesXml,
                        CustomerEnteredPrice = customerEnteredPrice,
                        Quantity = quantity,
                        RentalStartDateUtc = rentalStartDate,
                        RentalEndDateUtc = rentalEndDate,
                        CreatedOnUtc = now,
                        UpdatedOnUtc = now
                    };

                    customer.ShoppingCartItems.Add(shoppingCartItem);

                    _customerService.UpdateCustomer(customer);

                    //maximum items validation
                    switch (shoppingCartType)
                    {
                        case ShoppingCartType.ShoppingCart:
                            {
                                foreach (var attribute in shoppingCartItemAttributeMappings)
                                {
                                    attribute.ShoppingCartItemId = shoppingCartItem.Id;
                                    InsertShoppingCartItemAttributeMapping(attribute);
                                }
                            }
                            break;
                        case ShoppingCartType.Wishlist:
                            {
                                
                            }
                            break;
                        default:
                            break;
                    }
                    


                    //updated "HasShoppingCartItems" property used for performance optimization
                    customer.HasShoppingCartItems = customer.ShoppingCartItems.Count > 0;
                    _customerService.UpdateCustomer(customer);

                    //event notification
                    _eventPublisher.EntityInserted(shoppingCartItem);
                }
            }

            return warnings;
        }


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
        public virtual IList<string> UpdateShoppingCartItem(List<ShoppingCartItemAttributeMapping> shoppingCartItemAttributeMappings, Customer customer,
            int shoppingCartItemId, string attributesXml,
            decimal customerEnteredPrice,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool resetCheckoutData = true)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var warnings = new List<string>();

            var shoppingCartItem = customer.ShoppingCartItems.FirstOrDefault(sci => sci.Id == shoppingCartItemId);
            if (shoppingCartItem != null)
            {
                if (resetCheckoutData)
                {
                    //reset checkout data
                    _customerService.ResetCheckoutData(customer, shoppingCartItem.StoreId);
                }
                if (quantity > 0)
                {
                    //check warnings
                    warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartItem.ShoppingCartType,
                        shoppingCartItem.Product, shoppingCartItem.StoreId,
                        attributesXml, customerEnteredPrice,
                        rentalStartDate, rentalEndDate, quantity, false));
                    if (warnings.Count == 0)
                    {
                        //if everything is OK, then update a shopping cart item
                        shoppingCartItem.Quantity = quantity;
                        shoppingCartItem.AttributesXml = attributesXml;
                        shoppingCartItem.CustomerEnteredPrice = customerEnteredPrice;
                        shoppingCartItem.RentalStartDateUtc = rentalStartDate;
                        shoppingCartItem.RentalEndDateUtc = rentalEndDate;
                        shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;

                        foreach (var attribute in shoppingCartItemAttributeMappings)
                        {
                            var attributeEntity = GetShoppingCartItemAttributeMappingByCartIdAndAttributeId(shoppingCartItem.Id, attribute.ProductAttributeId);

                            if(attributeEntity != null)
                            {
                                attributeEntity.Value = attribute.Value;
                                UpdateShoppingCartItemAttributeMapping(attributeEntity);
                            }

                        }
                        _customerService.UpdateCustomer(customer);

                        //event notification
                        _eventPublisher.EntityUpdated(shoppingCartItem);
                    }
                }
                else
                {
                    //delete a shopping cart item
                    DeleteShoppingCartItem(shoppingCartItem, resetCheckoutData, true);
                }
            }

            return warnings;
        }
    }


}
