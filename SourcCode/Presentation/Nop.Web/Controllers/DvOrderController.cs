using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Models.Order;
using Nop.Core.Domain.Divui.Catalog;
using Nop.Web.Infrastructure.Cache;
using Nop.Core.Caching;
using Nop.Core.Domain.Media;
using Nop.Web.Models.Media;
using Nop.Web.Models.Catalog;
using Nop.Services.Customers;

namespace Nop.Web.Controllers
{
    public partial class OrderController 
    {
        #region fields
        private readonly MediaSettings _mediaSettings;
        private readonly ICacheManager _cacheManager;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly CustomerSettings _customerSettings;
        #endregion

        #region Constructors

        public OrderController(IOrderService orderService,
            IShipmentService shipmentService,
            IWorkContext workContext,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IOrderProcessingService orderProcessingService,
            IDateTimeHelper dateTimeHelper,
            IPaymentService paymentService,
            ILocalizationService localizationService,
            IPdfService pdfService,
            IShippingService shippingService,
            ICountryService countryService,
            IProductAttributeParser productAttributeParser,
            IWebHelper webHelper,
            IDownloadService downloadService,
            IAddressAttributeFormatter addressAttributeFormatter,
            IStoreContext storeContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            IRewardPointService rewardPointService,
            CatalogSettings catalogSettings,
            OrderSettings orderSettings,
            TaxSettings taxSettings,
            ShippingSettings shippingSettings,
            AddressSettings addressSettings,
            RewardPointsSettings rewardPointsSettings,
            PdfSettings pdfSettings,
             MediaSettings mediaSettings,
            ICacheManager cacheManager,
            IPictureService pictureService,
            IProductService productService,
            CustomerSettings customerSettings)
        {
            this._orderService = orderService;
            this._shipmentService = shipmentService;
            this._workContext = workContext;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._orderProcessingService = orderProcessingService;
            this._dateTimeHelper = dateTimeHelper;
            this._paymentService = paymentService;
            this._localizationService = localizationService;
            this._pdfService = pdfService;
            this._shippingService = shippingService;
            this._countryService = countryService;
            this._productAttributeParser = productAttributeParser;
            this._webHelper = webHelper;
            this._downloadService = downloadService;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._storeContext = storeContext;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._rewardPointService = rewardPointService;

            this._catalogSettings = catalogSettings;
            this._orderSettings = orderSettings;
            this._taxSettings = taxSettings;
            this._shippingSettings = shippingSettings;
            this._addressSettings = addressSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._pdfSettings = pdfSettings;
            this._mediaSettings = mediaSettings;
            this._cacheManager = cacheManager;
            this._pictureService = pictureService;
            this._productService = productService;
            this._customerSettings = customerSettings;

        }

        #endregion

        #region Utilities
        [NonAction]
        protected virtual OrderDetailsModel PrepareOrderDetailsModel(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");
            var model = new OrderDetailsModel();

            model.Id = order.Id;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.IsReOrderAllowed = _orderSettings.IsReOrderAllowed;
            model.IsReturnRequestAllowed = _orderProcessingService.IsReturnRequestAllowed(order);
            model.PdfInvoiceDisabled = _pdfSettings.DisablePdfInvoicesForPendingOrders && order.OrderStatus == OrderStatus.Pending;

            //shipping info
            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;
                model.PickUpInStore = order.PickUpInStore;
                if (!order.PickUpInStore)
                {
                    model.ShippingAddress.PrepareModel(
                        address: order.ShippingAddress,
                        excludeProperties: false,
                        addressSettings: _addressSettings,
                        addressAttributeFormatter: _addressAttributeFormatter);
                }
                model.ShippingMethod = order.ShippingMethod;


                //shipments (only already shipped)
                var shipments = order.Shipments.Where(x => x.ShippedDateUtc.HasValue).OrderBy(x => x.CreatedOnUtc).ToList();
                foreach (var shipment in shipments)
                {
                    var shipmentModel = new OrderDetailsModel.ShipmentBriefModel
                    {
                        Id = shipment.Id,
                        TrackingNumber = shipment.TrackingNumber,
                    };
                    if (shipment.ShippedDateUtc.HasValue)
                        shipmentModel.ShippedDate = _dateTimeHelper.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
                    if (shipment.DeliveryDateUtc.HasValue)
                        shipmentModel.DeliveryDate = _dateTimeHelper.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);
                    model.Shipments.Add(shipmentModel);
                }
            }


            //billing info
            model.BillingAddress.PrepareModel(
                address: order.BillingAddress,
                excludeProperties: false,
                addressSettings: _addressSettings,
                addressAttributeFormatter: _addressAttributeFormatter);

            //VAT number
            model.VatNumber = order.VatNumber;

            //payment method
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            model.PaymentMethod = paymentMethod != null ? paymentMethod.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id) : order.PaymentMethodSystemName;
            model.PaymentMethodStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.CanRePostProcessPayment = _paymentService.CanRePostProcessPayment(order);
            //custom values
            model.CustomValues = order.DeserializeCustomValues();

            //order subtotal
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                //order subtotal
                var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                model.OrderSubtotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                //discount (applied to order subtotal)
                var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                    model.OrderSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
            }
            else
            {
                //excluding tax

                //order subtotal
                var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                model.OrderSubtotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                //discount (applied to order subtotal)
                var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
                    model.OrderSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
            }

            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax

                //order shipping
                var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                model.OrderShipping = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                //payment method additional fee
                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                if (paymentMethodAdditionalFeeInclTaxInCustomerCurrency > decimal.Zero)
                    model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
            }
            else
            {
                //excluding tax

                //order shipping
                var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                model.OrderShipping = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                //payment method additional fee
                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                if (paymentMethodAdditionalFeeExclTaxInCustomerCurrency > decimal.Zero)
                    model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
            }

            //tax
            bool displayTax = true;
            bool displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    displayTaxRates = _taxSettings.DisplayTaxRates && order.TaxRatesDictionary.Count > 0;
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    //TODO pass languageId to _priceFormatter.FormatPrice
                    model.Tax = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);

                    foreach (var tr in order.TaxRatesDictionary)
                    {
                        model.TaxRates.Add(new OrderDetailsModel.TaxRate
                        {
                            Rate = _priceFormatter.FormatTaxRate(tr.Key),
                            //TODO pass languageId to _priceFormatter.FormatPrice
                            Value = _priceFormatter.FormatPrice(_currencyService.ConvertCurrency(tr.Value, order.CurrencyRate), true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage),
                        });
                    }
                }
            }
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoOrderDetailsPage;
            model.PricesIncludeTax = order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax;

            //discount (applied to order total)
            var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
            if (orderDiscountInCustomerCurrency > decimal.Zero)
                model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);


            //gift cards
            foreach (var gcuh in order.GiftCardUsageHistory)
            {
                model.GiftCards.Add(new OrderDetailsModel.GiftCard
                {
                    CouponCode = gcuh.GiftCard.GiftCardCouponCode,
                    Amount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage),
                });
            }

            //reward points           
            if (order.RedeemedRewardPointsEntry != null)
            {
                model.RedeemedRewardPoints = -order.RedeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            model.OrderTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);

            //checkout attributes
            model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;

            //order notes
            foreach (var orderNote in order.OrderNotes
                .Where(on => on.DisplayToCustomer)
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList())
            {
                model.OrderNotes.Add(new OrderDetailsModel.OrderNote
                {
                    Id = orderNote.Id,
                    HasDownload = orderNote.DownloadId > 0,
                    Note = orderNote.FormatOrderNoteText(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }


            //purchased products
            model.ShowSku = _catalogSettings.ShowProductSku;
            var orderItems = _orderService.GetAllOrderItems(order.Id, null, null, null, null, null, null);
            var orderOptions = orderItems.Select(p => new { productoption = p.Product.ProductOptions.FirstOrDefault(), attributesXml = p.AttributesXml }).Distinct().ToList();
            foreach (var orderoption in orderOptions)
            {
                var option = orderoption.productoption;
                if (option == null)
                    continue;
                var optionModel = new OrderDetailsModel.OrderItemOptionModel()
                {
                    ProductOptionId = option.Id,
                    ProductOptionName = option.GetLocalized(po => po.Name),
                    SeName = option.Product.GetSeName()
                };

                var items = orderItems.Where(p => p.Product.ProductOptions.Contains(option)).ToList();


                #region Pictures


                //default picture
                var defaultPictureSize = _mediaSettings.ProductDetailsPictureSize;
                //prepare picture models
                var productPicturesCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DETAILS_PICTURES_MODEL_KEY, option.Product.Id, defaultPictureSize, false, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                var cachedPictures = _cacheManager.Get(productPicturesCacheKey, () =>
                {
                    var pictures = _pictureService.GetPicturesByProductId(option.Product.Id);
                    var defaultPicture = pictures.FirstOrDefault();
                    var defaultPictureModel = new PictureModel
                    {
                        ImageUrl = _pictureService.GetPictureUrl(defaultPicture, defaultPictureSize, true),
                        FullSizeImageUrl = _pictureService.GetPictureUrl(defaultPicture, 0, true),
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), option.Product.GetLocalized(x => x.Name)),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), option.Product.GetLocalized(x => x.Name)),
                    };
                    //"title" attribute
                    defaultPictureModel.Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                        defaultPicture.TitleAttribute :
                        string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), option.Product.GetLocalized(x => x.Name));
                    //"alt" attribute
                    defaultPictureModel.AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                        defaultPicture.AltAttribute :
                        string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), option.Product.GetLocalized(x => x.Name));

                    //all pictures
                    var pictureModels = new List<PictureModel>();
                    foreach (var picture in pictures)
                    {
                        var pictureModel = new PictureModel
                        {
                            ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), option.Product.GetLocalized(x => x.Name)),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), option.Product.GetLocalized(x => x.Name)),
                        };
                        //"title" attribute
                        pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                            picture.TitleAttribute :
                            string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), option.Product.GetLocalized(x => x.Name));
                        //"alt" attribute
                        pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                            picture.AltAttribute :
                            string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), option.Product.GetLocalized(x => x.Name));

                        pictureModels.Add(pictureModel);
                    }

                    return new { DefaultPictureModel = defaultPictureModel, PictureModels = pictureModels };
                });
                optionModel.Picture = cachedPictures.DefaultPictureModel;


                #endregion

                foreach (var orderItem in items)
                {
                    var orderItemModel = new OrderDetailsModel.OrderItemModel
                    {
                        Id = orderItem.Id,
                        OrderItemGuid = orderItem.OrderItemGuid,
                        Sku = orderItem.Product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                        ProductId = orderItem.Product.Id,
                        ProductName = orderItem.Product.GetLocalized(x => x.Name),
                        ProductSeName = orderItem.Product.GetSeName(),
                        Quantity = orderItem.Quantity,
                        AttributeInfo = orderItem.AttributeDescription,
                        AgeRangeTypeId = orderItem.Product.AgeRange,
                        AgeRangeCondition = orderItem.Product.AgeRangeCondition,
                    };

                    if (orderItemModel.AgeRangeTypeId == (int)DvAgeRangeType.Adult)
                    {
                        var pickupAttribute = orderItem.OrderItemAttributeMappings.FirstOrDefault(p => p.ProductAttribute.SystemName == ProductAttributeSystemNames.Pickup);
                        if (pickupAttribute != null)
                        {
                            var pickupAttributeModel = new OrderDetailsModel.OrderItemAttributeModel
                            {
                                ProductId = orderItem.Product.Id,
                                ProductAttributeId = pickupAttribute.ProductAttributeId,
                                Name = pickupAttribute.ProductAttribute.GetLocalized(x => x.Name),
                                AttributeControlType = pickupAttribute.AttributeControlType,
                                Value = pickupAttribute.Value,

                            };

                            optionModel.PickupAttribute = pickupAttributeModel;
                        }

                        var startDateAttribute = orderItem.OrderItemAttributeMappings.FirstOrDefault(p => p.ProductAttribute.SystemName == ProductAttributeSystemNames.StartDate);
                        if (startDateAttribute != null)
                        {
                            var startDateAttributeModel = new OrderDetailsModel.OrderItemAttributeModel
                            {
                                ProductId = orderItem.Product.Id,
                                ProductAttributeId = startDateAttribute.ProductAttributeId,
                                Name = startDateAttribute.ProductAttribute.GetLocalized(x => x.Name),
                                AttributeControlType = startDateAttribute.AttributeControlType,
                                Value = startDateAttribute.Value,
                            };

                            optionModel.SelectedDateAttribute = startDateAttributeModel;
                        }
                    }

                    //rental info
                    if (orderItem.Product.IsRental)
                    {
                        var rentalStartDate = orderItem.RentalStartDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalStartDateUtc.Value) : "";
                        var rentalEndDate = orderItem.RentalEndDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalEndDateUtc.Value) : "";
                        orderItemModel.RentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
                            rentalStartDate, rentalEndDate);
                    }

                    //unit price, subtotal
                    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        //including tax
                        var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                        orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);

                        var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                        orderItemModel.SubTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                    }
                    else
                    {
                        //excluding tax
                        var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                        orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);

                        var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                        orderItemModel.SubTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                    }

                    //downloadable products
                    if (_downloadService.IsDownloadAllowed(orderItem))
                        orderItemModel.DownloadId = orderItem.Product.DownloadId;
                    if (_downloadService.IsLicenseDownloadAllowed(orderItem))
                        orderItemModel.LicenseId = orderItem.LicenseDownloadId.HasValue ? orderItem.LicenseDownloadId.Value : 0;

                    optionModel.Items.Add(orderItemModel);


                }

                optionModel.Total = items.Sum(i => i.PriceInclTax);
                optionModel.strTotal = _priceFormatter.FormatPrice(optionModel.Total);

                optionModel.AdultNumber = items.Count(o => o.Product.AgeRangeType == DvAgeRangeType.Adult);
                optionModel.ChildNumber = items.Count(o => o.Product.AgeRangeType == DvAgeRangeType.Child);
                optionModel.KidNumber = items.Count(o => o.Product.AgeRangeType == DvAgeRangeType.Kid);
                optionModel.SeniorNumber = items.Count(o => o.Product.AgeRangeType == DvAgeRangeType.Senior);

                model.OptionItems.Add(optionModel);
            }



            return model;
        }

        [NonAction]
        protected virtual CustomerOrderListModel PrepareCustomerOrderListModel()
        {
            var model = new CustomerOrderListModel();
            var orders = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id);
            foreach (var order in orders)
            {
                var orderModel = PrepareOrderDetailsModel(order);
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                orderModel.OrderTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);

                model.Orders.Add(orderModel);
            }

            var recurringPayments = _orderService.SearchRecurringPayments(_storeContext.CurrentStore.Id,
                _workContext.CurrentCustomer.Id);
            foreach (var recurringPayment in recurringPayments)
            {
                var recurringPaymentModel = new CustomerOrderListModel.RecurringOrderModel
                {
                    Id = recurringPayment.Id,
                    StartDate = _dateTimeHelper.ConvertToUserTime(recurringPayment.StartDateUtc, DateTimeKind.Utc).ToString(),
                    CycleInfo = string.Format("{0} {1}", recurringPayment.CycleLength, recurringPayment.CyclePeriod.GetLocalizedEnum(_localizationService, _workContext)),
                    NextPayment = recurringPayment.NextPaymentDate.HasValue ? _dateTimeHelper.ConvertToUserTime(recurringPayment.NextPaymentDate.Value, DateTimeKind.Utc).ToString() : "",
                    TotalCycles = recurringPayment.TotalCycles,
                    CyclesRemaining = recurringPayment.CyclesRemaining,
                    InitialOrderId = recurringPayment.InitialOrder.Id,
                    CanCancel = _orderProcessingService.CanCancelRecurringPayment(_workContext.CurrentCustomer, recurringPayment),
                };

                model.RecurringOrders.Add(recurringPaymentModel);
            }

            return model;
        }
        #endregion

        
    }
}