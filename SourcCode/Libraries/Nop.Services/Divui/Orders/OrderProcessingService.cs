﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;

namespace Nop.Services.Orders
{
    public partial class OrderProcessingService
    {
        /// <summary>
        /// Places an order
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Place order result</returns>
        public virtual PlaceOrderResult DvPlaceOrder(ProcessPaymentRequest processPaymentRequest)
        {
            //think about moving functionality of processing recurring orders (after the initial order was placed) to ProcessNextRecurringPayment() method
            if (processPaymentRequest == null)
                throw new ArgumentNullException("processPaymentRequest");

            var result = new PlaceOrderResult();
            try
            {
                if (processPaymentRequest.OrderGuid == Guid.Empty)
                    processPaymentRequest.OrderGuid = Guid.NewGuid();

                //prepare order details
                var details = DvPreparePlaceOrderDetails(processPaymentRequest);

                #region Payment workflow


                //process payment
                ProcessPaymentResult processPaymentResult = null;
                //skip payment workflow if order total equals zero
                var skipPaymentWorkflow = details.OrderTotal == decimal.Zero;
                if (!skipPaymentWorkflow)
                {
                    var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
                    if (paymentMethod == null)
                        throw new NopException("Payment method couldn't be loaded");

                    //ensure that payment method is active
                    if (!paymentMethod.IsPaymentMethodActive(_paymentSettings))
                        throw new NopException("Payment method is not active");

                    if (!processPaymentRequest.IsRecurringPayment)
                    {
                        if (details.IsRecurringShoppingCart)
                        {
                            //recurring cart
                            var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                            switch (recurringPaymentType)
                            {
                                case RecurringPaymentType.NotSupported:
                                    throw new NopException("Recurring payments are not supported by selected payment method");
                                case RecurringPaymentType.Manual:
                                case RecurringPaymentType.Automatic:
                                    processPaymentResult = _paymentService.ProcessRecurringPayment(processPaymentRequest);
                                    break;
                                default:
                                    throw new NopException("Not supported recurring payment type");
                            }
                        }
                        else
                        {
                            //standard cart
                            processPaymentResult = _paymentService.ProcessPayment(processPaymentRequest);
                        }
                    }
                    else
                    {
                        if (details.IsRecurringShoppingCart)
                        {
                            //Old credit card info
                            processPaymentRequest.CreditCardType = details.InitialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(details.InitialOrder.CardType) : "";
                            processPaymentRequest.CreditCardName = details.InitialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(details.InitialOrder.CardName) : "";
                            processPaymentRequest.CreditCardNumber = details.InitialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(details.InitialOrder.CardNumber) : "";
                            processPaymentRequest.CreditCardCvv2 = details.InitialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(details.InitialOrder.CardCvv2) : "";
                            try
                            {
                                processPaymentRequest.CreditCardExpireMonth = details.InitialOrder.AllowStoringCreditCardNumber ? Convert.ToInt32(_encryptionService.DecryptText(details.InitialOrder.CardExpirationMonth)) : 0;
                                processPaymentRequest.CreditCardExpireYear = details.InitialOrder.AllowStoringCreditCardNumber ? Convert.ToInt32(_encryptionService.DecryptText(details.InitialOrder.CardExpirationYear)) : 0;
                            }
                            catch { }

                            var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                            switch (recurringPaymentType)
                            {
                                case RecurringPaymentType.NotSupported:
                                    throw new NopException("Recurring payments are not supported by selected payment method");
                                case RecurringPaymentType.Manual:
                                    processPaymentResult = _paymentService.ProcessRecurringPayment(processPaymentRequest);
                                    break;
                                case RecurringPaymentType.Automatic:
                                    //payment is processed on payment gateway site
                                    processPaymentResult = new ProcessPaymentResult();
                                    break;
                                default:
                                    throw new NopException("Not supported recurring payment type");
                            }
                        }
                        else
                        {
                            throw new NopException("No recurring products");
                        }
                    }
                }
                else
                {
                    //payment is not required
                    if (processPaymentResult == null)
                        processPaymentResult = new ProcessPaymentResult();
                    processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;
                }

                if (processPaymentResult == null)
                    throw new NopException("processPaymentResult is not available");

                #endregion

                if (processPaymentResult.Success)
                {
                    #region Save order details

                    var order = new Order
                    {
                        StoreId = processPaymentRequest.StoreId,
                        OrderGuid = processPaymentRequest.OrderGuid,
                        CustomerId = details.Customer.Id,
                        CustomerLanguageId = details.CustomerLanguage.Id,
                        CustomerTaxDisplayType = details.CustomerTaxDisplayType,
                        CustomerIp = _webHelper.GetCurrentIpAddress(),
                        OrderSubtotalInclTax = details.OrderSubTotalInclTax,
                        OrderSubtotalExclTax = details.OrderSubTotalExclTax,
                        OrderSubTotalDiscountInclTax = details.OrderSubTotalDiscountInclTax,
                        OrderSubTotalDiscountExclTax = details.OrderSubTotalDiscountExclTax,
                        OrderShippingInclTax = details.OrderShippingTotalInclTax,
                        OrderShippingExclTax = details.OrderShippingTotalExclTax,
                        PaymentMethodAdditionalFeeInclTax = details.PaymentAdditionalFeeInclTax,
                        PaymentMethodAdditionalFeeExclTax = details.PaymentAdditionalFeeExclTax,
                        TaxRates = details.TaxRates,
                        OrderTax = details.OrderTaxTotal,
                        OrderTotal = details.OrderTotal,
                        RefundedAmount = decimal.Zero,
                        OrderDiscount = details.OrderDiscountAmount,
                        CheckoutAttributeDescription = details.CheckoutAttributeDescription,
                        CheckoutAttributesXml = details.CheckoutAttributesXml,
                        CustomerCurrencyCode = details.CustomerCurrencyCode,
                        CurrencyRate = details.CustomerCurrencyRate,
                        AffiliateId = details.AffiliateId,
                        OrderStatus = OrderStatus.Pending,
                        AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber,
                        CardType = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardType) : string.Empty,
                        CardName = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardName) : string.Empty,
                        CardNumber = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardNumber) : string.Empty,
                        MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(processPaymentRequest.CreditCardNumber)),
                        CardCvv2 = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardCvv2) : string.Empty,
                        CardExpirationMonth = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireMonth.ToString()) : string.Empty,
                        CardExpirationYear = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireYear.ToString()) : string.Empty,
                        PaymentMethodSystemName = processPaymentRequest.PaymentMethodSystemName,
                        AuthorizationTransactionId = processPaymentResult.AuthorizationTransactionId,
                        AuthorizationTransactionCode = processPaymentResult.AuthorizationTransactionCode,
                        AuthorizationTransactionResult = processPaymentResult.AuthorizationTransactionResult,
                        CaptureTransactionId = processPaymentResult.CaptureTransactionId,
                        CaptureTransactionResult = processPaymentResult.CaptureTransactionResult,
                        SubscriptionTransactionId = processPaymentResult.SubscriptionTransactionId,
                        PaymentStatus = processPaymentResult.NewPaymentStatus,
                        PaidDateUtc = null,
                        BillingAddress = details.BillingAddress,
                        ShippingAddress = details.ShippingAddress,
                        ShippingStatus = details.ShippingStatus,
                        ShippingMethod = details.ShippingMethodName,
                        PickUpInStore = details.PickUpInStore,
                        ShippingRateComputationMethodSystemName = details.ShippingRateComputationMethodSystemName,
                        CustomValuesXml = processPaymentRequest.SerializeCustomValues(),
                        VatNumber = details.VatNumber,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    _orderService.InsertOrder(order);

                    result.PlacedOrder = order;

                    if (!processPaymentRequest.IsRecurringPayment)
                    {
                        //move shopping cart items to order items
                        foreach (var sc in details.Cart)
                        {
                            //prices
                            decimal taxRate;
                            Discount scDiscount;
                            decimal discountAmount;
                            decimal scUnitPrice = _priceCalculationService.GetUnitPrice(sc);
                            decimal scSubTotal = _priceCalculationService.GetSubTotal(sc, true, out discountAmount, out scDiscount);
                            decimal scUnitPriceInclTax = _taxService.GetProductPrice(sc.Product, scUnitPrice, true, details.Customer, out taxRate);
                            decimal scUnitPriceExclTax = _taxService.GetProductPrice(sc.Product, scUnitPrice, false, details.Customer, out taxRate);
                            decimal scSubTotalInclTax = _taxService.GetProductPrice(sc.Product, scSubTotal, true, details.Customer, out taxRate);
                            decimal scSubTotalExclTax = _taxService.GetProductPrice(sc.Product, scSubTotal, false, details.Customer, out taxRate);

                            decimal discountAmountInclTax = _taxService.GetProductPrice(sc.Product, discountAmount, true, details.Customer, out taxRate);
                            decimal discountAmountExclTax = _taxService.GetProductPrice(sc.Product, discountAmount, false, details.Customer, out taxRate);
                            if (scDiscount != null && !details.AppliedDiscounts.ContainsDiscount(scDiscount))
                                details.AppliedDiscounts.Add(scDiscount);

                            //attributes
                            string attributeDescription = _productAttributeFormatter.FormatAttributes(sc.Product, sc.AttributesXml, details.Customer);

                            var itemWeight = _shippingService.GetShoppingCartItemWeight(sc);

                            //save order item
                            var orderItem = new OrderItem
                            {
                                OrderItemGuid = Guid.NewGuid(),
                                Order = order,
                                ProductId = sc.ProductId,
                                UnitPriceInclTax = scUnitPriceInclTax,
                                UnitPriceExclTax = scUnitPriceExclTax,
                                PriceInclTax = scSubTotalInclTax,
                                PriceExclTax = scSubTotalExclTax,
                                OriginalProductCost = _priceCalculationService.GetProductCost(sc.Product, sc.AttributesXml),
                                AttributeDescription = attributeDescription,
                                AttributesXml = sc.AttributesXml,
                                Quantity = sc.Quantity,
                                DiscountAmountInclTax = discountAmountInclTax,
                                DiscountAmountExclTax = discountAmountExclTax,
                                DownloadCount = 0,
                                IsDownloadActivated = false,
                                LicenseDownloadId = 0,
                                ItemWeight = itemWeight,
                                RentalStartDateUtc = sc.RentalStartDateUtc,
                                RentalEndDateUtc = sc.RentalEndDateUtc
                            };
                            order.OrderItems.Add(orderItem);
                            foreach (var sca in sc.ShoppingCartItemAttributeMappings)
                            {
                                orderItem.OrderItemAttributeMappings.Add(new OrderItemAttributeMapping()
                                {
                                    OrderItemId = orderItem.Id,
                                    ProductAttributeId = sca.ProductAttributeId,
                                    Value = sca.Value,
                                    AttributeControlTypeId = sca.AttributeControlTypeId,
                                    DisplayOrder = sca.DisplayOrder,
                                    IsRequired = true
                                });
                            
                            }

                            _orderService.UpdateOrder(order);

                            //gift cards
                            if (sc.Product.IsGiftCard)
                            {
                                string giftCardRecipientName, giftCardRecipientEmail,
                                    giftCardSenderName, giftCardSenderEmail, giftCardMessage;
                                _productAttributeParser.GetGiftCardAttribute(sc.AttributesXml,
                                    out giftCardRecipientName, out giftCardRecipientEmail,
                                    out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                                for (int i = 0; i < sc.Quantity; i++)
                                {
                                    var gc = new GiftCard
                                    {
                                        GiftCardType = sc.Product.GiftCardType,
                                        PurchasedWithOrderItem = orderItem,
                                        Amount = sc.Product.OverriddenGiftCardAmount.HasValue ? sc.Product.OverriddenGiftCardAmount.Value : scUnitPriceExclTax,
                                        IsGiftCardActivated = false,
                                        GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                                        RecipientName = giftCardRecipientName,
                                        RecipientEmail = giftCardRecipientEmail,
                                        SenderName = giftCardSenderName,
                                        SenderEmail = giftCardSenderEmail,
                                        Message = giftCardMessage,
                                        IsRecipientNotified = false,
                                        CreatedOnUtc = DateTime.UtcNow
                                    };
                                    _giftCardService.InsertGiftCard(gc);
                                }
                            }

                            //inventory
                            _productService.AdjustInventory(sc.Product, -sc.Quantity, sc.AttributesXml);
                        }

                        //clear shopping cart
                        details.Cart.ToList().ForEach(sci => _shoppingCartService.DeleteShoppingCartItem(sci, false));
                    }
                    else
                    {
                        //recurring payment
                        var initialOrderItems = details.InitialOrder.OrderItems;
                        foreach (var orderItem in initialOrderItems)
                        {
                            //save item
                            var newOrderItem = new OrderItem
                            {
                                OrderItemGuid = Guid.NewGuid(),
                                Order = order,
                                ProductId = orderItem.ProductId,
                                UnitPriceInclTax = orderItem.UnitPriceInclTax,
                                UnitPriceExclTax = orderItem.UnitPriceExclTax,
                                PriceInclTax = orderItem.PriceInclTax,
                                PriceExclTax = orderItem.PriceExclTax,
                                OriginalProductCost = orderItem.OriginalProductCost,
                                AttributeDescription = orderItem.AttributeDescription,
                                AttributesXml = orderItem.AttributesXml,
                                Quantity = orderItem.Quantity,
                                DiscountAmountInclTax = orderItem.DiscountAmountInclTax,
                                DiscountAmountExclTax = orderItem.DiscountAmountExclTax,
                                DownloadCount = 0,
                                IsDownloadActivated = false,
                                LicenseDownloadId = 0,
                                ItemWeight = orderItem.ItemWeight,
                                RentalStartDateUtc = orderItem.RentalStartDateUtc,
                                RentalEndDateUtc = orderItem.RentalEndDateUtc
                            };
                            order.OrderItems.Add(newOrderItem);
                            _orderService.UpdateOrder(order);

                            //gift cards
                            if (orderItem.Product.IsGiftCard)
                            {
                                string giftCardRecipientName, giftCardRecipientEmail,
                                    giftCardSenderName, giftCardSenderEmail, giftCardMessage;
                                _productAttributeParser.GetGiftCardAttribute(orderItem.AttributesXml,
                                    out giftCardRecipientName, out giftCardRecipientEmail,
                                    out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                                for (int i = 0; i < orderItem.Quantity; i++)
                                {
                                    var gc = new GiftCard
                                    {
                                        GiftCardType = orderItem.Product.GiftCardType,
                                        PurchasedWithOrderItem = newOrderItem,
                                        Amount = orderItem.UnitPriceExclTax,
                                        IsGiftCardActivated = false,
                                        GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                                        RecipientName = giftCardRecipientName,
                                        RecipientEmail = giftCardRecipientEmail,
                                        SenderName = giftCardSenderName,
                                        SenderEmail = giftCardSenderEmail,
                                        Message = giftCardMessage,
                                        IsRecipientNotified = false,
                                        CreatedOnUtc = DateTime.UtcNow
                                    };
                                    _giftCardService.InsertGiftCard(gc);
                                }
                            }

                            //inventory
                            _productService.AdjustInventory(orderItem.Product, -orderItem.Quantity, orderItem.AttributesXml);
                        }
                    }

                    //discount usage history
                    if (!processPaymentRequest.IsRecurringPayment)
                        foreach (var discount in details.AppliedDiscounts)
                        {
                            var duh = new DiscountUsageHistory
                            {
                                Discount = discount,
                                Order = order,
                                CreatedOnUtc = DateTime.UtcNow
                            };
                            _discountService.InsertDiscountUsageHistory(duh);
                        }

                    //gift card usage history
                    if (!processPaymentRequest.IsRecurringPayment)
                        if (details.AppliedGiftCards != null)
                            foreach (var agc in details.AppliedGiftCards)
                            {
                                decimal amountUsed = agc.AmountCanBeUsed;
                                var gcuh = new GiftCardUsageHistory
                                {
                                    GiftCard = agc.GiftCard,
                                    UsedWithOrder = order,
                                    UsedValue = amountUsed,
                                    CreatedOnUtc = DateTime.UtcNow
                                };
                                agc.GiftCard.GiftCardUsageHistory.Add(gcuh);
                                _giftCardService.UpdateGiftCard(agc.GiftCard);
                            }

                    //reward points history
                    if (details.RedeemedRewardPointsAmount > decimal.Zero)
                    {
                        _rewardPointService.AddRewardPointsHistoryEntry(details.Customer,
                            -details.RedeemedRewardPoints, order.StoreId,
                            string.Format(_localizationService.GetResource("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.Id),
                            order, details.RedeemedRewardPointsAmount);
                        _customerService.UpdateCustomer(details.Customer);
                    }

                    //recurring orders
                    if (!processPaymentRequest.IsRecurringPayment && details.IsRecurringShoppingCart)
                    {
                        //create recurring payment (the first payment)
                        var rp = new RecurringPayment
                        {
                            CycleLength = processPaymentRequest.RecurringCycleLength,
                            CyclePeriod = processPaymentRequest.RecurringCyclePeriod,
                            TotalCycles = processPaymentRequest.RecurringTotalCycles,
                            StartDateUtc = DateTime.UtcNow,
                            IsActive = true,
                            CreatedOnUtc = DateTime.UtcNow,
                            InitialOrder = order,
                        };
                        _orderService.InsertRecurringPayment(rp);


                        var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                        switch (recurringPaymentType)
                        {
                            case RecurringPaymentType.NotSupported:
                                {
                                    //not supported
                                }
                                break;
                            case RecurringPaymentType.Manual:
                                {
                                    //first payment
                                    var rph = new RecurringPaymentHistory
                                    {
                                        RecurringPayment = rp,
                                        CreatedOnUtc = DateTime.UtcNow,
                                        OrderId = order.Id,
                                    };
                                    rp.RecurringPaymentHistory.Add(rph);
                                    _orderService.UpdateRecurringPayment(rp);
                                }
                                break;
                            case RecurringPaymentType.Automatic:
                                {
                                    //will be created later (process is automated)
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    #endregion

                    #region Notifications & notes

                    //notes, messages
                    if (_workContext.OriginalCustomerIfImpersonated != null)
                    {
                        //this order is placed by a store administrator impersonating a customer
                        order.OrderNotes.Add(new OrderNote
                        {
                            Note = string.Format("Order placed by a store owner ('{0}'. ID = {1}) impersonating the customer.",
                                _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        _orderService.UpdateOrder(order);
                    }
                    else
                    {
                        order.OrderNotes.Add(new OrderNote
                        {
                            Note = "Order placed",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        _orderService.UpdateOrder(order);
                    }


                    //send email notifications
                    int orderPlacedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
                    if (orderPlacedStoreOwnerNotificationQueuedEmailId > 0)
                    {
                        order.OrderNotes.Add(new OrderNote
                        {
                            Note = string.Format("\"Order placed\" email (to store owner) has been queued. Queued email identifier: {0}.", orderPlacedStoreOwnerNotificationQueuedEmailId),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        _orderService.UpdateOrder(order);
                    }

                    var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                        _pdfService.PrintOrderToPdf(order, order.CustomerLanguageId) : null;
                    var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                        "order.pdf" : null;
                    int orderPlacedCustomerNotificationQueuedEmailId = _workflowMessageService
                        .SendOrderPlacedCustomerNotification(order, order.CustomerLanguageId, orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName);
                    if (orderPlacedCustomerNotificationQueuedEmailId > 0)
                    {
                        order.OrderNotes.Add(new OrderNote
                        {
                            Note = string.Format("\"Order placed\" email (to customer) has been queued. Queued email identifier: {0}.", orderPlacedCustomerNotificationQueuedEmailId),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        _orderService.UpdateOrder(order);
                    }

                    var vendors = GetVendorsInOrder(order);
                    foreach (var vendor in vendors)
                    {
                        int orderPlacedVendorNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedVendorNotification(order, vendor, order.CustomerLanguageId);
                        if (orderPlacedVendorNotificationQueuedEmailId > 0)
                        {
                            order.OrderNotes.Add(new OrderNote
                            {
                                Note = string.Format("\"Order placed\" email (to vendor) has been queued. Queued email identifier: {0}.", orderPlacedVendorNotificationQueuedEmailId),
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            _orderService.UpdateOrder(order);
                        }
                    }

                    //check order status
                    CheckOrderStatus(order);

                    //reset checkout data
                    if (!processPaymentRequest.IsRecurringPayment)
                        _customerService.ResetCheckoutData(details.Customer, processPaymentRequest.StoreId, clearCouponCodes: true, clearCheckoutAttributes: true);

                    if (!processPaymentRequest.IsRecurringPayment)
                    {
                        _customerActivityService.InsertActivity(
                            "PublicStore.PlaceOrder",
                            _localizationService.GetResource("ActivityLog.PublicStore.PlaceOrder"),
                            order.Id);
                    }

                    //raise event       
                    _eventPublisher.Publish(new OrderPlacedEvent(order));

                    if (order.PaymentStatus == PaymentStatus.Paid)
                    {
                        ProcessOrderPaid(order);
                    }
                    #endregion
                }
                else
                {
                    //payment errors
                    foreach (var paymentError in processPaymentResult.Errors)
                        result.AddError(string.Format(_localizationService.GetResource("Checkout.PaymentError"), paymentError));
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc.Message, exc);
                result.AddError(exc.Message);
            }

            #region Process errors

            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i + 1, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //log it
                string logError = string.Format("Error while placing order. {0}", error);
                var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
                _logger.Error(logError, customer: customer);
            }

            #endregion

            return result;
        }

        /// <summary>
        /// Prepare details to place an order. It also sets some properties to "processPaymentRequest"
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Details</returns>
        protected virtual PlaceOrderContainter DvPreparePlaceOrderDetails(ProcessPaymentRequest processPaymentRequest)
        {
            var details = new PlaceOrderContainter();

            //Recurring orders. Load initial order
            if (processPaymentRequest.IsRecurringPayment)
            {
                details.InitialOrder = _orderService.GetOrderById(processPaymentRequest.InitialOrderId);
                if (details.InitialOrder == null)
                    throw new ArgumentException("Initial order is not set for recurring payment");

                processPaymentRequest.PaymentMethodSystemName = details.InitialOrder.PaymentMethodSystemName;
            }

            //customer
            details.Customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            if (details.Customer == null)
                throw new ArgumentException("Customer is not set");

            //affiliate
            var affiliate = _affiliateService.GetAffiliateById(details.Customer.AffiliateId);
            if (affiliate != null && affiliate.Active && !affiliate.Deleted)
                details.AffiliateId = affiliate.Id;

            //customer currency
            if (!processPaymentRequest.IsRecurringPayment)
            {
                var currencyTmp = _currencyService.GetCurrencyById(details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.CurrencyId, processPaymentRequest.StoreId));
                var customerCurrency = (currencyTmp != null && currencyTmp.Published) ? currencyTmp : _workContext.WorkingCurrency;
                details.CustomerCurrencyCode = customerCurrency.CurrencyCode;
                var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                details.CustomerCurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate;
            }
            else
            {
                details.CustomerCurrencyCode = details.InitialOrder.CustomerCurrencyCode;
                details.CustomerCurrencyRate = details.InitialOrder.CurrencyRate;
            }

            //customer language
            if (!processPaymentRequest.IsRecurringPayment)
            {
                details.CustomerLanguage = _languageService.GetLanguageById(details.Customer.GetAttribute<int>(
                    SystemCustomerAttributeNames.LanguageId, processPaymentRequest.StoreId));
            }
            else
            {
                details.CustomerLanguage = _languageService.GetLanguageById(details.InitialOrder.CustomerLanguageId);
            }
            if (details.CustomerLanguage == null || !details.CustomerLanguage.Published)
                details.CustomerLanguage = _workContext.WorkingLanguage;

            //check whether customer is guest
            if (details.Customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                throw new NopException("Anonymous checkout is not allowed");

            //billing address
            if (!processPaymentRequest.IsRecurringPayment)
            {
                if (details.Customer.BillingAddress == null)
                    throw new NopException("Billing address is not provided");

                if (!CommonHelper.IsValidEmail(details.Customer.BillingAddress.Email))
                    throw new NopException("Email is not valid");

                //clone billing address
                details.BillingAddress = (Address)details.Customer.BillingAddress.Clone();
                if (details.BillingAddress.Country != null && !details.BillingAddress.Country.AllowsBilling)
                    throw new NopException(string.Format("Country '{0}' is not allowed for billing", details.BillingAddress.Country.Name));
            }
            else
            {
                if (details.InitialOrder.BillingAddress == null)
                    throw new NopException("Billing address is not available");

                //clone billing address
                details.BillingAddress = (Address)details.InitialOrder.BillingAddress.Clone();
                if (details.BillingAddress.Country != null && !details.BillingAddress.Country.AllowsBilling)
                    throw new NopException(string.Format("Country '{0}' is not allowed for billing", details.BillingAddress.Country.Name));
            }

            //checkout attributes
            if (!processPaymentRequest.IsRecurringPayment)
            {
                details.CheckoutAttributesXml = details.Customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, processPaymentRequest.StoreId);
                details.CheckoutAttributeDescription = _checkoutAttributeFormatter.FormatAttributes(details.CheckoutAttributesXml, details.Customer);
            }
            else
            {
                details.CheckoutAttributesXml = details.InitialOrder.CheckoutAttributesXml;
                details.CheckoutAttributeDescription = details.InitialOrder.CheckoutAttributeDescription;
            }

            //load and validate customer shopping cart
            if (!processPaymentRequest.IsRecurringPayment)
            {
                //load shopping cart
                details.Cart = details.Customer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(processPaymentRequest.StoreId)
                    .ToList();

                if (details.Cart.Count == 0)
                    throw new NopException("Cart is empty");

                //validate the entire shopping cart
                var warnings = _shoppingCartService.GetShoppingCartWarnings(details.Cart,
                    details.CheckoutAttributesXml,
                    true);
                if (warnings.Count > 0)
                {
                    var warningsSb = new StringBuilder();
                    foreach (string warning in warnings)
                    {
                        warningsSb.Append(warning);
                        warningsSb.Append(";");
                    }
                    throw new NopException(warningsSb.ToString());
                }

                //validate individual cart items
                foreach (var sci in details.Cart)
                {
                    var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(details.Customer, sci.ShoppingCartType,
                        sci.Product, processPaymentRequest.StoreId, sci.AttributesXml,
                        sci.CustomerEnteredPrice, sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                        sci.Quantity, false, getAttributesWarnings: false);
                    if (sciWarnings.Count > 0)
                    {
                        var warningsSb = new StringBuilder();
                        foreach (string warning in sciWarnings)
                        {
                            warningsSb.Append(warning);
                            warningsSb.Append(";");
                        }
                        throw new NopException(warningsSb.ToString());
                    }
                }
            }

            //min totals validation
            if (!processPaymentRequest.IsRecurringPayment)
            {
                bool minOrderSubtotalAmountOk = ValidateMinOrderSubtotalAmount(details.Cart);
                if (!minOrderSubtotalAmountOk)
                {
                    decimal minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
                    throw new NopException(string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false)));
                }
                bool minOrderTotalAmountOk = ValidateMinOrderTotalAmount(details.Cart);
                if (!minOrderTotalAmountOk)
                {
                    decimal minOrderTotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
                    throw new NopException(string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"), _priceFormatter.FormatPrice(minOrderTotalAmount, true, false)));
                }
            }

            //tax display type
            if (!processPaymentRequest.IsRecurringPayment)
            {
                if (_taxSettings.AllowCustomersToSelectTaxDisplayType)
                    details.CustomerTaxDisplayType = (TaxDisplayType)details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.TaxDisplayTypeId, processPaymentRequest.StoreId);
                else
                    details.CustomerTaxDisplayType = _taxSettings.TaxDisplayType;
            }
            else
            {
                details.CustomerTaxDisplayType = details.InitialOrder.CustomerTaxDisplayType;
            }

            //sub total
            if (!processPaymentRequest.IsRecurringPayment)
            {
                //sub total (incl tax)
                decimal orderSubTotalDiscountAmount1;
                Discount orderSubTotalAppliedDiscount1;
                decimal subTotalWithoutDiscountBase1;
                decimal subTotalWithDiscountBase1;
                _orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart,
                    true, out orderSubTotalDiscountAmount1, out orderSubTotalAppliedDiscount1,
                    out subTotalWithoutDiscountBase1, out subTotalWithDiscountBase1);
                details.OrderSubTotalInclTax = subTotalWithoutDiscountBase1;
                details.OrderSubTotalDiscountInclTax = orderSubTotalDiscountAmount1;

                //discount history
                if (orderSubTotalAppliedDiscount1 != null && !details.AppliedDiscounts.ContainsDiscount(orderSubTotalAppliedDiscount1))
                    details.AppliedDiscounts.Add(orderSubTotalAppliedDiscount1);

                //sub total (excl tax)
                decimal orderSubTotalDiscountAmount2;
                Discount orderSubTotalAppliedDiscount2;
                decimal subTotalWithoutDiscountBase2;
                decimal subTotalWithDiscountBase2;
                _orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart,
                    false, out orderSubTotalDiscountAmount2, out orderSubTotalAppliedDiscount2,
                    out subTotalWithoutDiscountBase2, out subTotalWithDiscountBase2);
                details.OrderSubTotalExclTax = subTotalWithoutDiscountBase2;
                details.OrderSubTotalDiscountExclTax = orderSubTotalDiscountAmount2;
            }
            else
            {
                details.OrderSubTotalInclTax = details.InitialOrder.OrderSubtotalInclTax;
                details.OrderSubTotalExclTax = details.InitialOrder.OrderSubtotalExclTax;
            }


            //shipping info
            bool shoppingCartRequiresShipping;
            if (!processPaymentRequest.IsRecurringPayment)
            {
                shoppingCartRequiresShipping = details.Cart.RequiresShipping();
            }
            else
            {
                shoppingCartRequiresShipping = details.InitialOrder.ShippingStatus != ShippingStatus.ShippingNotRequired;
            }
            if (shoppingCartRequiresShipping)
            {
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    details.PickUpInStore = _shippingSettings.AllowPickUpInStore &&
                        details.Customer.GetAttribute<bool>(SystemCustomerAttributeNames.SelectedPickUpInStore, processPaymentRequest.StoreId);

                    if (!details.PickUpInStore)
                    {
                        if (details.Customer.ShippingAddress == null)
                            throw new NopException("Shipping address is not provided");

                        if (!CommonHelper.IsValidEmail(details.Customer.ShippingAddress.Email))
                            throw new NopException("Email is not valid");

                        //clone shipping address
                        details.ShippingAddress = (Address)details.Customer.ShippingAddress.Clone();
                        if (details.ShippingAddress.Country != null && !details.ShippingAddress.Country.AllowsShipping)
                        {
                            throw new NopException(string.Format("Country '{0}' is not allowed for shipping", details.ShippingAddress.Country.Name));
                        }
                    }

                    var shippingOption = details.Customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, processPaymentRequest.StoreId);
                    if (shippingOption != null)
                    {
                        details.ShippingMethodName = shippingOption.Name;
                        details.ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName;
                    }
                }
                else
                {
                    details.PickUpInStore = details.InitialOrder.PickUpInStore;
                    if (!details.PickUpInStore)
                    {
                        if (details.InitialOrder.ShippingAddress == null)
                            throw new NopException("Shipping address is not available");

                        //clone shipping address
                        details.ShippingAddress = (Address)details.InitialOrder.ShippingAddress.Clone();
                        if (details.ShippingAddress.Country != null && !details.ShippingAddress.Country.AllowsShipping)
                        {
                            throw new NopException(string.Format("Country '{0}' is not allowed for shipping", details.ShippingAddress.Country.Name));
                        }
                    }

                    details.ShippingMethodName = details.InitialOrder.ShippingMethod;
                    details.ShippingRateComputationMethodSystemName = details.InitialOrder.ShippingRateComputationMethodSystemName;
                }
            }
            details.ShippingStatus = shoppingCartRequiresShipping
                ? ShippingStatus.NotYetShipped
                : ShippingStatus.ShippingNotRequired;

            //shipping total
            if (!processPaymentRequest.IsRecurringPayment)
            {
                decimal taxRate;
                Discount shippingTotalDiscount;
                decimal? orderShippingTotalInclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, true, out taxRate, out shippingTotalDiscount);
                decimal? orderShippingTotalExclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, false);
                if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
                    throw new NopException("Shipping total couldn't be calculated");
                details.OrderShippingTotalInclTax = orderShippingTotalInclTax.Value;
                details.OrderShippingTotalExclTax = orderShippingTotalExclTax.Value;

                if (shippingTotalDiscount != null && !details.AppliedDiscounts.ContainsDiscount(shippingTotalDiscount))
                    details.AppliedDiscounts.Add(shippingTotalDiscount);
            }
            else
            {
                details.OrderShippingTotalInclTax = details.InitialOrder.OrderShippingInclTax;
                details.OrderShippingTotalExclTax = details.InitialOrder.OrderShippingExclTax;
            }


            //payment total
            if (!processPaymentRequest.IsRecurringPayment)
            {
                decimal paymentAdditionalFee = _paymentService.GetAdditionalHandlingFee(details.Cart, processPaymentRequest.PaymentMethodSystemName);
                details.PaymentAdditionalFeeInclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, true, details.Customer);
                details.PaymentAdditionalFeeExclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, false, details.Customer);
            }
            else
            {
                details.PaymentAdditionalFeeInclTax = details.InitialOrder.PaymentMethodAdditionalFeeInclTax;
                details.PaymentAdditionalFeeExclTax = details.InitialOrder.PaymentMethodAdditionalFeeExclTax;
            }


            //tax total
            if (!processPaymentRequest.IsRecurringPayment)
            {
                //tax amount
                SortedDictionary<decimal, decimal> taxRatesDictionary;
                details.OrderTaxTotal = _orderTotalCalculationService.GetTaxTotal(details.Cart, out taxRatesDictionary);

                //VAT number
                var customerVatStatus = (VatNumberStatus)details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId);
                if (_taxSettings.EuVatEnabled && customerVatStatus == VatNumberStatus.Valid)
                    details.VatNumber = details.Customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);

                //tax rates
                foreach (var kvp in taxRatesDictionary)
                {
                    var taxRate = kvp.Key;
                    var taxValue = kvp.Value;
                    details.TaxRates += string.Format("{0}:{1};   ", taxRate.ToString(CultureInfo.InvariantCulture), taxValue.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                details.OrderTaxTotal = details.InitialOrder.OrderTax;
                //VAT number
                details.VatNumber = details.InitialOrder.VatNumber;
            }


            //order total (and applied discounts, gift cards, reward points)
            if (!processPaymentRequest.IsRecurringPayment)
            {
                List<AppliedGiftCard> appliedGiftCards;
                Discount orderAppliedDiscount;
                decimal orderDiscountAmount;
                int redeemedRewardPoints;
                decimal redeemedRewardPointsAmount;

                var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(details.Cart,
                    out orderDiscountAmount, out orderAppliedDiscount, out appliedGiftCards,
                    out redeemedRewardPoints, out redeemedRewardPointsAmount);
                if (!orderTotal.HasValue)
                    throw new NopException("Order total couldn't be calculated");

                details.OrderDiscountAmount = orderDiscountAmount;
                details.RedeemedRewardPoints = redeemedRewardPoints;
                details.RedeemedRewardPointsAmount = redeemedRewardPointsAmount;
                details.AppliedGiftCards = appliedGiftCards;
                details.OrderTotal = orderTotal.Value;

                //discount history
                if (orderAppliedDiscount != null && !details.AppliedDiscounts.ContainsDiscount(orderAppliedDiscount))
                    details.AppliedDiscounts.Add(orderAppliedDiscount);
            }
            else
            {
                details.OrderDiscountAmount = details.InitialOrder.OrderDiscount;
                details.OrderTotal = details.InitialOrder.OrderTotal;
            }
            processPaymentRequest.OrderTotal = details.OrderTotal;

            //recurring or standard shopping cart?
            if (!processPaymentRequest.IsRecurringPayment)
            {
                details.IsRecurringShoppingCart = details.Cart.IsRecurring();
                if (details.IsRecurringShoppingCart)
                {
                    int recurringCycleLength;
                    RecurringProductCyclePeriod recurringCyclePeriod;
                    int recurringTotalCycles;
                    string recurringCyclesError = details.Cart.GetRecurringCycleInfo(_localizationService,
                        out recurringCycleLength, out recurringCyclePeriod, out recurringTotalCycles);
                    if (!string.IsNullOrEmpty(recurringCyclesError))
                        throw new NopException(recurringCyclesError);

                    processPaymentRequest.RecurringCycleLength = recurringCycleLength;
                    processPaymentRequest.RecurringCyclePeriod = recurringCyclePeriod;
                    processPaymentRequest.RecurringTotalCycles = recurringTotalCycles;
                }
            }
            else
            {
                details.IsRecurringShoppingCart = true;
            }

            return details;
        }

    }
}
