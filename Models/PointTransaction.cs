using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models;

public partial class PointTransaction
{
    public int TransactionId { get; set; }

    public int WalletId { get; set; }

    public decimal? Amount { get; set; }

    public string? Type { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? RelatedOrderId { get; set; }

    public virtual Order? RelatedOrder { get; set; }

    public virtual PointWallet Wallet { get; set; } = null!;
}
