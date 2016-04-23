using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Orders
{
    public partial interface IOrderProcessingService
    {
        PlaceOrderResult DvPlaceOrder(ProcessPaymentRequest processPaymentRequest);
    }
}
