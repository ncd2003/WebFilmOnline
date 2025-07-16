using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public string? ThumbnailUrl { get; set; }

    public decimal? Price { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public virtual ProductCategory? Category { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductStreaming> ProductStreamings { get; set; } = new List<ProductStreaming>();

    public virtual ICollection<Package> Packages { get; set; } = new List<Package>();
}
