using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models;

public partial class PromotionTarget
{
    public int Id { get; set; }

    public int PromotionId { get; set; }

    public int? ProductCategoryId { get; set; }

    public int? PackageId { get; set; }

    public virtual Package? Package { get; set; }

    public virtual ProductCategory? ProductCategory { get; set; }

    public virtual Promotion Promotion { get; set; } = null!;
}
