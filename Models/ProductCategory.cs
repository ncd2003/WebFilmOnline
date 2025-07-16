using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models;

public partial class ProductCategory
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<PromotionTarget> PromotionTargets { get; set; } = new List<PromotionTarget>();
}
