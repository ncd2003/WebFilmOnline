using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? DiscountPercent { get; set; }

    public decimal? MinPurchaseAmount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<PromotionTarget> PromotionTargets { get; set; } = new List<PromotionTarget>();
}
