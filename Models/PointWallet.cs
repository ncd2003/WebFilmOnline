using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models;

public partial class PointWallet
{
    public int WalletId { get; set; }

    public int UserId { get; set; }

    public decimal? Balance { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();

    public virtual User User { get; set; } = null!;
}
