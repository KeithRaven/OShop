using Orchard.Data;
using OShop.Models;
using System;
using Orchard.ContentManagement;

namespace OShop.Services {
    public class PaymentService : IPaymentService {
        private readonly IRepository<PaymentTransactionRecord> _transactionRepository;
        private readonly IOrdersService _orderService;
        public PaymentService(
            IRepository<PaymentTransactionRecord> transactionRepository,
            IOrdersService orderService) {
            _transactionRepository = transactionRepository;
            _orderService = orderService;
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
                order.OrderStatus = OrderStatus.Processing;
            }

         

        }
    }
}