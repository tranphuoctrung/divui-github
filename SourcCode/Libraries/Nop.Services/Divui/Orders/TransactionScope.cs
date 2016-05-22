using System;

namespace Nop.Services.Orders
{
    internal class TransactionScope : IDisposable
    {
        private object required;
        private TransactionOptions transactionOptions;

        public TransactionScope(object required, TransactionOptions transactionOptions)
        {
            this.required = required;
            this.transactionOptions = transactionOptions;
        }
    }
}