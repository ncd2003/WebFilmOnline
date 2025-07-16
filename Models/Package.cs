using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models;

public partial class Package
{
    public int PackageId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public int? DurationDays { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<PromotionTarget> PromotionTargets { get; set; } = new List<PromotionTarget>();

    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
