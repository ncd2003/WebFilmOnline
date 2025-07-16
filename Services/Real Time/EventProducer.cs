using System.Collections.Concurrent;
using System.Threading;
using WebFilmOnline.Models;

namespace WebFilmOnline.Services.Real_Time
{
    public class EventProducer
    {
        private readonly ConcurrentQueue<TransactionEvent> _eventQueue;
        private int _transactionCounter = 0;

        public EventProducer(ConcurrentQueue<TransactionEvent> eventQueue)
        {
            _eventQueue = eventQueue;
        }

        // Phương thức này sẽ được gọi từ các service khác (ví dụ: PaymentService, PointService)
        // khi một giao dịch thành công.
        public void GenerateTransaction(string userId, string productType, string productName, decimal amount, string paymentMethod, string status, string promotionCode = null)
        {
            var transactionId = $"TXN-{Interlocked.Increment(ref _transactionCounter):D5}";
            var newEvent = new TransactionEvent
            {
                TransactionId = transactionId,
                Timestamp = DateTime.Now,
                UserId = userId,
                ProductType = productType,
                ProductName = productName,
                Amount = amount,
                PaymentMethod = paymentMethod,
                PromotionCode = promotionCode,
                Status = status
            };
            _eventQueue.Enqueue(newEvent);
            Console.WriteLine($"[EventProducer] Đã tạo và đẩy sự kiện: {newEvent.TransactionId} - {newEvent.ProductName} - {newEvent.Amount} {newEvent.Status}");
        }
    }
}
