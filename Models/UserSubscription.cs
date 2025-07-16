using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models;

public partial class UserSubscription
{
    public int SubscriptionId { get; set; }

    public int UserId { get; set; }

    public int PackageId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal? RemainingCreditAmount { get; set; }

    public virtual Package Package { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
