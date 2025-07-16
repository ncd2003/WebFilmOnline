using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models
{
    public class RevenueSummaryDto
    {
        public decimal CurrentTotalRevenue { get; set; }
        public long SuccessfulTransactionsCount { get; set; }
        public Dictionary<string, decimal> RevenueByProductType { get; set; }
        public List<MinuteRevenueEntry> RevenueByMinute { get; set; }

        public class MinuteRevenueEntry
        {
            public string Minute { get; set; } // Định dạng HH:mm
            public decimal Revenue { get; set; }
        }
    }
}
