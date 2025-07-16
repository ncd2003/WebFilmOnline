using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? PaymentStatus { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentTransactionId { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();

    public virtual User User { get; set; } = null!;
}
