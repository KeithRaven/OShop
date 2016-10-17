using Orchard;
using Orchard.Data;
using OShop.Models;
using System;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.ContentManagement.Handlers;
using System.Collections.Generic;
using Orchard.Logging;

namespace OShop.Services {
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<PaymentTransactionRecord> _transactionRepository;
        private readonly IOrdersService _orderService;
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;
        public ILogger Logger { get; set; }
        public PaymentService(
            IRepository<PaymentTransactionRecord> transactionRepository,
            IOrdersService orderService,
            Lazy<IEnumerable<IContentHandler>> handlers) {
            _transactionRepository = transactionRepository;
            _orderService = orderService;

            _handlers = handlers;
            Logger = NullLogger.Instance;
        }

        public PaymentTransactionRecord GetTransaction(int Id) {
            return _transactionRepository.Get(Id);
        }

        public void AddTransaction(PaymentPart Part, PaymentTransactionRecord Transaction) {
            if (Part == null) {
                throw new ArgumentNullException("Part", "PaymentPart cannot be null.");
            }
            if (Transaction == null) {
                throw new ArgumentNullException("Transaction", "Transaction cannot be null.");
            }
            Transaction.PaymentPartRecord = Part.Record;
            _transactionRepository.Create(Transaction);
        }

        public void UpdateTransaction(PaymentTransactionRecord Transaction) {
            if (Transaction == null) {
                throw new ArgumentNullException("Transaction", "Transaction cannot be null.");
            }

            _transactionRepository.Update(Transaction);

            var order = _orderService.GetOrderById(Transaction.PaymentPartRecord.ContentItemRecord.Id);
            var paymentStatus = order.As<PaymentPart>().Status;

            if(Transaction.Status == TransactionStatus.Validated && paymentStatus == PaymentStatus.Completed){
                var context = new UpdateContentContext(order.ContentItem);

                _handlers.Value.Invoke(handler => handler.Updating(context), Logger);

                order.OrderStatus = OrderStatus.Processing;

                _handlers.Value.Invoke(handler => handler.Updated(context), Logger);
            }

        }

    }
}