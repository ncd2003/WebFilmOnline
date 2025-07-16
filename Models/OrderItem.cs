using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int? ProductId { get; set; }

    public int? PackageId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? FinalPrice { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Package? Package { get; set; }

    public virtual Product? Product { get; set; }
}
