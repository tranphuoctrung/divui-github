using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.OnePAY.Models;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.OnePAY.Controllers
{
    public class PaymentOnePAYController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly OnePAYPaymentSettings _onePAYPaymentSettings;
        private readonly PaymentSettings _paymentSettings;

        public PaymentOnePAYController(ISettingService settingService,
            IPaymentService paymentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger, IWebHelper webHelper,
            OnePAYPaymentSettings onePAYPaymentSettings,
            PaymentSettings paymentSettings)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._webHelper = webHelper;
            this._onePAYPaymentSettings = onePAYPaymentSettings;
            this._paymentSettings = paymentSettings;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.Sandbox = _onePAYPaymentSettings.Sandbox;
            model.UseSandbox = _onePAYPaymentSettings.UseSandbox;
            model.MerchantID = _onePAYPaymentSettings.MerchantID;
            model.AccessCode = _onePAYPaymentSettings.AccessCode;
            model.HashCode = _onePAYPaymentSettings.HashCode;
            model.AdditionalFee = _onePAYPaymentSettings.AdditionalFee;
            return View("~/Plugins/Payments.OnePAY/Views/PaymentOnePAY/Configure.cshtml", model);
            //return View("Nop.Plugin.Payments.OnePAY.Views.PaymentOnePAY.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _onePAYPaymentSettings.Sandbox = model.Sandbox;
            _onePAYPaymentSettings.UseSandbox = model.UseSandbox;
            _onePAYPaymentSettings.MerchantID = model.MerchantID;
            _onePAYPaymentSettings.AccessCode = model.AccessCode;
            _onePAYPaymentSettings.HashCode = model.HashCode;
            _onePAYPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_onePAYPaymentSettings);

            return View("~/Plugins/Payments.OnePAY/Views/PaymentOnePAY/Configure.cshtml", model);

            //return View("Nop.Plugin.Payments.OnePAY.Views.PaymentOnePAY.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.OnePAY/Views/PaymentOnePAY/PaymentInfo.cshtml");
            //return View("Nop.Plugin.Payments.OnePAY.Views.PaymentOnePAY.PaymentInfo");
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        [ValidateInput(false)]
        public ActionResult PDTHandler(FormCollection form)
        {
            var model = _onePAYPaymentSettings;
            string hashValidateResult = "";
            VPCRequest conn = new VPCRequest("http://onepay.vn");
            conn.SetSecureSecret(model.HashCode);
            hashValidateResult = conn.Process3PartyResponse(Request.QueryString);

            // Lay gia tri tham so tra ve tu cong thanh toan
            string txnResponseCode = conn.GetResultField("vpc_TxnResponseCode", "Unknown");
            string message = conn.GetResultField("vpc_Message", "Unknown");
            string command = conn.GetResultField("vpc_Command", "Unknown");
            string localed = conn.GetResultField("vpc_Locale", "Unknown");
            string merchTxnRef = conn.GetResultField("vpc_MerchTxnRef", "Unknown");
            string merchantID = conn.GetResultField("vpc_Merchant", "Unknown"); // MerchantID provided by OnePAY
            string orderInfo = conn.GetResultField("vpc_OrderInfo", "Unknown"); // Order Id
            string amount = conn.GetResultField("vpc_Amount", "Unknown");
            string transactionNo = conn.GetResultField("vpc_TransactionNo", "Unknown"); // ID Transaction created by OnePAY
            string acqResponseCode = conn.GetResultField("vpc_AcqResponseCode", "Unknown");
            string receiptNo = conn.GetResultField("vpc_ReceiptNo", "Unknown");
            string batchNo = conn.GetResultField("vpc_BatchNo", "Unknown");
            string authorizeld = conn.GetResultField("vpc_Authorizeld", "Unknown");
            string card = conn.GetResultField("vpc_Card", "Unknown");
            string secureHash = conn.GetResultField("vpc_SecureHash", "Unknown");
            string verSecurityLevel = conn.GetResultField("vpc_VerSecurityLevel", "Unknown");
            string verToken = conn.GetResultField("Vpc_VerToken", "Unknown");
            string vpc_3DSXID = conn.GetResultField("vpc_3DSXID", "Unknown");
            string vpc_3DSECI = conn.GetResultField("vpc_3DSECI", "Unknown");
            string vpc_3Dsenrolled = conn.GetResultField("vpc_3Dsenrolled", "Unknown");
            string vpc_3Dsstatus = conn.GetResultField("vpc_3Dsstatus", "Unknown");

            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.OnePAY") as OnePAYPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("OnePay Standard module cannot be loaded");

            Order order = _orderService.GetOrderById(Convert.ToInt32(orderInfo)); // GetOrderById(IdOrder);
            if (order != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine("OnePAY result:");
                sb.AppendLine("vpc_TxnResponseCode: " + txnResponseCode);
                sb.AppendLine("vpc_Message: " + message);
                sb.AppendLine("vpc_Command: " + command);
                sb.AppendLine("vpc_Locale: " + localed);
                sb.AppendLine("vpc_MerchTxnRef: " + merchTxnRef);
                sb.AppendLine("vpc_Merchant: " + merchantID);
                sb.AppendLine("vpc_OrderInfo: " + orderInfo);
                sb.AppendLine("vpc_Amount: " + String.Format("{0:#,###}" + ".00", Convert.ToDecimal(amount) / 100) + " vnd");
                sb.AppendLine("vpc_TransactionNo: " + transactionNo);
                sb.AppendLine("vpc_AcqResponseCode: " + acqResponseCode);
                sb.AppendLine("vpc_ReceiptNo: " + receiptNo);
                sb.AppendLine("vpc_BatchNo: " + batchNo);
                sb.AppendLine("vpc_AuthorizeId: " + authorizeld);
                sb.AppendLine("vpc_Card: " + card);
                sb.AppendLine("vpc_SecureHash: " + secureHash);
                sb.AppendLine("vpc_VerSecurityLevel: " + verSecurityLevel);
                sb.AppendLine("vpc_VerToken: " + verToken);
                sb.AppendLine("vpc_3DSXID: " + vpc_3DSXID);
                sb.AppendLine("vpc_3DSECI: " + vpc_3DSECI);
                sb.AppendLine("vpc_3Dsenrolled: " + vpc_3Dsenrolled);
                sb.AppendLine("vpc_3DsStatus: " + vpc_3Dsstatus);

                //order note
                UpdateNoteOrder(order.Id, sb.ToString());

                // total order send OnePAY
                string totalSend = (Math.Round(order.OrderTotal, 0) * 100).ToString();
                int lastIdx = totalSend.LastIndexOfAny(new char[] { '.', ',' });
                if (lastIdx != -1)
                    totalSend = totalSend.Substring(0, lastIdx);

                //validate onepay result 
                if (ValidateCheckout(order.Id, hashValidateResult, txnResponseCode.Trim()))
                {
                    //mark order as paid
                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        order.AuthorizationTransactionId = "visa";
                        order.AuthorizationTransactionCode = "0";
                        order.AuthorizationTransactionResult = "Giao dịch thành công";
                        _orderService.UpdateOrder(order);
                        _orderProcessingService.MarkOrderAsPaid(order);

                        return RedirectToRoute("CheckoutOnePAY");
                    }
                }
                else
                {
                    order.AuthorizationTransactionId = "visa";
                    order.AuthorizationTransactionCode = txnResponseCode.Trim();
                    order.AuthorizationTransactionResult = CheckReponseCodeWriteNodes(txnResponseCode.Trim());
                    _orderService.UpdateOrder(order);
                    return RedirectToRoute("CheckoutOnePAY");
                }

            }

            return RedirectToRoute("CheckoutOnePAY");
        }

        private bool ValidateCheckout(int idOrder, string hashValidateResult, string resCode)
        {
            bool check;
            if (hashValidateResult == "CORRECTED" && resCode == "0")
            {
                UpdateNoteOrder(idOrder, "Giao dịch thành công");
                check = true;
            }
            else if (hashValidateResult == "INVALIDATED" && resCode == "0")
            {
                UpdateNoteOrder(idOrder, "Giao dịch đang xử lý");
                check = false;
            }
            else
            {
                string mes = CheckReponseCodeWriteNodes(resCode);
                UpdateNoteOrder(idOrder, mes);
                check = false;
            }
            return check;
        }

        private void UpdateNoteOrder(int id, string note)
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

        private string CheckReponseCodeWriteNodes(string responseCode)
        {
            string message;
            if (responseCode == "?")
                message = "Lỗi cổng OnePAY không xác định.";
            else if (responseCode == "1")
                message = "Ngân hàng từ chối giao dịch.";
            else if (responseCode == "2")
                message = "Ngân hàng từ chối giao dịch";
            else if (responseCode == "3")
                message = "Giao dịch bị từ chối - Ngân hàng không trả về kết quả.";
            else if (responseCode == "4")
                message = "Giao dịch bị từ chối - Thẻ hết hạn/ Thẻ bị khóa.";
            else if (responseCode == "5")
                message = "Giao dịch bị từ chối - Số tiền trong thẻ không đủ để thực hiện giao dịch.";
            else if (responseCode == "6")
                message = "Giao dịch bị từ chối - Lỗi giao tiếp với ngân hàng.";
            else if (responseCode == "7")
                message = "Lỗi cổng OnePAY không xác định.";
            else if (responseCode == "8")
                message = "Giao dịch bị từ chối - Loại giao dịch không được hỗ trợ.";
            else if (responseCode == "9")
                message = "Lỗi cổng OnePAY không xác định.";
            else if (responseCode == "A")
                message = "Giao dịch bị hủy bỏ";
            else if (responseCode == "C")
                message = "Khách hàng hủy giao dịch.";
            else if (responseCode == "D")
                message = "Tạm dừng giao dịch.";
            else if (responseCode == "E")
                message = "Issuer Returned a Referral Response.";
            else if (responseCode == "F")
                message = "3D Secure Authentication Failed ";
            else if (responseCode == "I")
                message = "Mã an toàn không hợp lệ";
            else if (responseCode == "L")
                message = "Giao dịch đã bị khóa.";
            else if (responseCode == "N")
                message = "Cardholder is not enrolled in 3D Secure.";
            else if (responseCode == "P")
                message = "Giao dịch đang ở trạng thái chờ.";
            else if (responseCode == "R")
                message = "Thời gian thử lại vượt quá giới hạn, giao dịch không được xử lý.";
            else if (responseCode == "S")
                message = "OrderInfo bị trùng lặp.";
            else if (responseCode == "U")
                message = "Mã bảo vệ không chính xác.";
            else
                message = "Quá trình thanh toán xảy ra lỗi!";
            return message;
        }
    }
}