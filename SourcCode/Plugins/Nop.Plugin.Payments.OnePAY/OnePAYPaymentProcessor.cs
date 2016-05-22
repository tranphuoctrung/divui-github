using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.OnePAY.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;

namespace Nop.Plugin.Payments.OnePAY
{
    /// <summary>
    /// OnePay Standard payment processor
    /// </summary>
    public class OnePAYPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly OnePAYPaymentSettings _onePAYPaymentSettings;
        private readonly IOrderService _orderService;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ITaxService _taxService;
        private readonly HttpContextBase _httpContext;
        #endregion

        #region Ctor

        public OnePAYPaymentProcessor(OnePAYPaymentSettings onePAYPaymentSettings,
            IOrderService orderService,
            ISettingService settingService, ICurrencyService currencyService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            ICheckoutAttributeParser checkoutAttributeParser, ITaxService taxService,
            HttpContextBase httpContext)
        {
            this._onePAYPaymentSettings = onePAYPaymentSettings;
            this._orderService = orderService;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._taxService = taxService;
            this._httpContext = httpContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets OnePAY Payment URL
        /// </summary>
        /// <returns></returns>
        private string GetOnePAYUrl()
        {
            return _onePAYPaymentSettings.UseSandbox ? (string.IsNullOrEmpty(_onePAYPaymentSettings.Sandbox) ? "https://mtf.onepay.vn/vpcpay/vpcpay.op" : _onePAYPaymentSettings.Sandbox) :
                "https://onepay.vn/vpcpay/vpcpay.op";
        }

        /// <summary>
        /// Gets OnePAY version
        /// </summary>
        private string GetOnePayVersion()
        {
            return "2";
        }
        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //already paid or order.OrderTotal == decimal.Zero
            if (postProcessPaymentRequest.Order.PaymentStatus == PaymentStatus.Paid)
                return;

            try
            {
                StringBuilder sb = new StringBuilder();
                var model = _onePAYPaymentSettings;
                string merchantID = model.MerchantID;
                string accessCode = model.AccessCode;
                string hashCode = model.HashCode;

                // ID of transaction
                string merchTxnRef = postProcessPaymentRequest.Order.OrderGuid.ToString();

                string totalSend = (Math.Round(postProcessPaymentRequest.Order.OrderTotal, 0) * 100).ToString();

                // Check null or empty total order
                if (string.IsNullOrEmpty(totalSend))
                {
                    sb.AppendLine("Error: TotalOrder in invalid");
                    sb.AppendLine("Total Order: " + totalSend);
                    return;
                }
                else
                {
                    int lastIdx = totalSend.LastIndexOfAny(new char[] { '.', ',' });
                    if (lastIdx != -1)
                        totalSend = totalSend.Substring(0, lastIdx);

                    var infoOrder = postProcessPaymentRequest.Order;

                    string orderId = infoOrder.Id.ToString(); // Order name will show on payment gateway
                    string idCustumer = infoOrder.CustomerId.ToString();
                    string ipCustumer = infoOrder.CustomerIp.ToString();
                    string address = HttpUtility.UrlEncode(infoOrder.BillingAddress.Address1 != null ? infoOrder.BillingAddress.Address1 : "");
                    string province = HttpUtility.UrlEncode(infoOrder.BillingAddress.StateProvince != null ? infoOrder.BillingAddress.StateProvince.Name : "");
                    string city = HttpUtility.UrlEncode(infoOrder.BillingAddress.City != null ? infoOrder.BillingAddress.City : "");
                    string countryShip = HttpUtility.UrlEncode(infoOrder.BillingAddress.Country != null ? infoOrder.BillingAddress.Country.Name : "");                    
                    string phoneCustomer = HttpUtility.UrlEncode(infoOrder.BillingAddress.PhoneNumber != null ?  infoOrder.BillingAddress.PhoneNumber : "");
                    string emailCustomer = infoOrder.BillingAddress.Email.Trim();
                    
                    //string returnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentOnePAY/PDTHandler"; // URL for receiving payment result from gateway
                    string returnUrl = _webHelper.GetStoreHost(false) + "Plugins/PaymentOnePAY/PDTHandler"; // URL for receiving payment result from gateway

                    VPCRequest conn = new VPCRequest(GetOnePAYUrl());
                    conn.SetSecureSecret(hashCode);

                    //Thông tin về các trường trong giao dịch
                    conn.AddDigitalOrderField("AgainLink", _webHelper.GetStoreHost(false));
                    conn.AddDigitalOrderField("Title", "onepay paygate");
                    conn.AddDigitalOrderField("vpc_Locale", "vn"); //Chọn ngôn ngữ hiển thị khi thanh toán(vn/en)
                    conn.AddDigitalOrderField("vpc_Version", GetOnePayVersion());//GetOnePayVersion()); //version Onepay
                    conn.AddDigitalOrderField("vpc_Command", "pay"); // Gán giá trị Type_Comment mặc định là pay
                    conn.AddDigitalOrderField("vpc_Merchant", merchantID); // Mở file info.txt để lấy thông tin MerchantID. ONEPAY chỉ là ví dụ
                    conn.AddDigitalOrderField("vpc_AccessCode", accessCode);
                    conn.AddDigitalOrderField("vpc_MerchTxnRef", merchTxnRef);
                    conn.AddDigitalOrderField("vpc_OrderInfo", orderId);
                    conn.AddDigitalOrderField("vpc_Amount", totalSend); // Tổng giá tiền giao dịch gửi đi sau khi * 100                              
                    conn.AddDigitalOrderField("vpc_ReturnURL", returnUrl); // Link trả về sau khi thanh toán
                    
                    // Thông tin về khách hàng
                    conn.AddDigitalOrderField("vpc_SHIP_Street01", address);
                    conn.AddDigitalOrderField("vpc_SHIP_Provice", province);
                    conn.AddDigitalOrderField("vpc_SHIP_City", city);
                    conn.AddDigitalOrderField("vpc_SHIP_Country", countryShip);
                    conn.AddDigitalOrderField("vpc_Customer_Phone", phoneCustomer);
                    conn.AddDigitalOrderField("vpc_Customer_Email", emailCustomer);
                    conn.AddDigitalOrderField("vpc_Customer_Id", idCustumer); // Mã khách hàng
                    conn.AddDigitalOrderField("vpc_TicketNo", ipCustumer); // LTV viết hàm lấy IP Client

                    // Lưu lại thông tin trước gửi sang cổng thanh toán OnePAY vào Node Order Detail
                    sb.AppendLine("Send OnePAY: ");
                    sb.AppendLine("dv_CustomerId: " + idCustumer);
                    sb.AppendLine("dv_OrderId: " + orderId);
                    sb.AppendLine("dv_MerchTxnRef: " + merchTxnRef);
                    sb.AppendLine("dv_Amount: " + String.Format("{0:#,###}" + ".00", Convert.ToDecimal(totalSend) / 100) + " vnđ");
                    sb.AppendLine("dv_AddressShip: " + address + ", " + province + ", " + city + ", " + countryShip);
                    sb.AppendLine("dv_PhoneCustomer: " + phoneCustomer);
                    sb.AppendLine("dv_EmailCustomer: " + emailCustomer);

                    //Update order node
                    UpdateNoteOrder(postProcessPaymentRequest.Order.Id, sb.ToString());

                    //chuyển hướng sang cổng thanh toán
                    String url = conn.Create3PartyQueryString();
                    _httpContext.Response.Redirect(url);
                }
            }
            catch(Exception ex)
            {
                UpdateNoteOrder(postProcessPaymentRequest.Order.Id, ex.Message);
            }
        }

        public void UpdateNoteOrder(int id, string note)
        {
            Order order = _orderService.GetOrderById(id); // GetOrderById(IdOrder);
            order.OrderNotes.Add(new OrderNote()
            {
                Note = note,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee()
        {
            return _onePAYPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //PayPal Standard is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice

            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentOnePAY";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.OnePAY.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentOnePAY";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.OnePAY.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentOnePAYController);
        }


        /// <summary>
        /// Install Plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new OnePAYPaymentSettings()
            {
                Sandbox = "https://mtf.onepay.vn/vpcpay/vpcpay.op",
                UseSandbox = true,
                MerchantID = "TESTONEPAY",
                AccessCode = "6BEB2546",
                HashCode = "6D0870CDE5F24F34F3915FB0045120DB"
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.OnePAY.Fields.RedirectionTip", "Hệ thống sẽ tự động chuyển sang cổng thanh toán OnePAY sau khi bạn xác nhận đơn hàng.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.OnePAY.Fields.UseSandbox", "Sử dụng cổng test");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.OnePAY.Fields.UseSandbox.Hint", "Chọn Sandbox nếu bạn muốn giao dịch trên cổng thử nghiệm.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.OnePAY.Fields.MerchantID", "MerchantID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.OnePAY.Fields.MerchantID.Hint", "Nhập mã MerchantID do OnePAY cung cấp.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.OnePAY.Fields.AccessCode", "AccessCode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.OnePAY.Fields.AccessCode.Hint", "Nhập mã AccessCode do OnePAY cung cấp.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.OnePAY.Fields.HashCode", "HashCode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.OnePAY.Fields.HashCode.Hint", "Nhập mã HashCode do OnePAY cung cấp.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.OnePAY.Fields.AdditionalFee", "Phí dịch vụ");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.OnePAY.Fields.AdditionalFee.Hint", "Chọn mức phí khi sử dụng dịch vụ.");
            
            base.Install();
        }


        /// <summary>
        /// Uninstall Plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<OnePAYPaymentSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.OnePAY.Fields.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.OnePAY.Fields.UseSandbox");
            this.DeletePluginLocaleResource("Plugins.Payments.OnePAY.Fields.UseSandbox.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.OnePAY.Fields.MerchantID");
            this.DeletePluginLocaleResource("Plugins.Payments.OnePAY.Fields.MerchantID.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.OnePAY.Fields.AccessCode");
            this.DeletePluginLocaleResource("Plugins.Payments.OnePAY.Fields.AccessCode.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.OnePAY.Fields.HashCode");
            this.DeletePluginLocaleResource("Plugins.Payments.OnePAY.Fields.HashCode.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.OnePAY.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.OnePAY.Fields.AdditionalFee.Hint");

            base.Uninstall();
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return 0;
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }

        #endregion
    }
}
