using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Collections.Generic;
using WebFilmOnline.Models;

namespace WebFilmOnline.Services.Real_Time
{
    public class StreamProcessor : IHostedService, IDisposable
    {
        private readonly ConcurrentQueue<TransactionEvent> _eventQueue;
        private readonly ConcurrentDictionary<DateTime, decimal> _revenueByMinute;
        private decimal _currentTotalRevenue = 0;
        private readonly ConcurrentDictionary<string, decimal> _revenueByProductType;
        private long _successfulTransactions = 0;

        private Task _processingTask;
        private CancellationTokenSource _cts;

        public StreamProcessor(ConcurrentQueue<TransactionEvent> eventQueue)
        {
            _eventQueue = eventQueue;
            _revenueByMinute = new ConcurrentDictionary<DateTime, decimal>();
            _revenueByProductType = new ConcurrentDictionary<string, decimal>();
        }

        // Bắt đầu dịch vụ nền
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _processingTask = Task.Run(() => ProcessEvents(_cts.Token), _cts.Token);
            Console.WriteLine("[StreamProcessor] Bắt đầu xử lý luồng sự kiện nền...");
            return Task.CompletedTask;
        }

        // Dừng dịch vụ nền
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_processingTask == null) return;

            _cts.Cancel();
            try
            {
                await _processingTask;
            }
            catch (OperationCanceledException)
            {
                // Task đã bị hủy, đây là hành vi mong muốn
            }
            finally
            {
                _cts.Dispose();
            }
            Console.WriteLine("[StreamProcessor] Dừng xử lý luồng sự kiện nền.");
        }

        // Logic xử lý các sự kiện từ hàng đợi
        private async Task ProcessEvents(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_eventQueue.TryDequeue(out TransactionEvent transactionEvent))
                {
                    Console.WriteLine($"[StreamProcessor] Đang xử lý sự kiện: {transactionEvent.TransactionId}");

                    if (transactionEvent.Status == "Thành công")
                    {
                        // Cập nhật tổng doanh thu hiện tại
                        Interlocked.Add(ref _currentTotalRevenue, transactionEvent.Amount);

                        // Cập nhật doanh thu theo phút (làm tròn xuống phút gần nhất)
                        var minuteKey = transactionEvent.Timestamp.Date.AddHours(transactionEvent.Timestamp.Hour).AddMinutes(transactionEvent.Timestamp.Minute);
                        _revenueByMinute.AddOrUpdate(minuteKey, transactionEvent.Amount, (key, oldValue) => oldValue + transactionEvent.Amount);

                        // Cập nhật doanh thu theo loại sản phẩm
                        _revenueByProductType.AddOrUpdate(transactionEvent.ProductType, transactionEvent.Amount, (key, oldValue) => oldValue + transactionEvent.Amount);

                        // Cập nhật số lượng giao dịch thành công
                        Interlocked.Increment(ref _successfulTransactions);
                    }
                }
                else
                {
                    await Task.Delay(100, token); // Đợi một chút nếu không có sự kiện
                }
            }
        }

        // Phương thức để Controller truy vấn dữ liệu tổng hợp
        public RevenueSummaryDto GetCurrentRevenueSummary()
        {
            // Lấy 5 phút gần nhất, sắp xếp và chuyển đổi sang DTO
            var recentMinutes = _revenueByMinute
                                    .OrderByDescending(kv => kv.Key)
                                    .Take(5)
                                    .OrderBy(kv => kv.Key) // Sắp xếp lại theo thứ tự tăng dần thời gian
                                    .Select(kv => new RevenueSummaryDto.MinuteRevenueEntry
                                    {
                                        Minute = kv.Key.ToString("HH:mm"),
                                        Revenue = kv.Value
                                    })
                                    .ToList();

            return new RevenueSummaryDto
            {
                CurrentTotalRevenue = _currentTotalRevenue,
                SuccessfulTransactionsCount = _successfulTransactions,
                RevenueByProductType = new Dictionary<string, decimal>(_revenueByProductType),
                RevenueByMinute = recentMinutes
            };
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _processingTask?.Wait(); // Chờ task hoàn thành trước khi dispose
            _cts?.Dispose();
        }
    }
}
