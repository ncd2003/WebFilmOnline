using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? Ssoprovider { get; set; }

    public string? Ssoid { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Package> Packages { get; set; } = new List<Package>();

    public virtual PointWallet? PointWallet { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();

    public virtual ICollection<StreamingProvider> StreamingProviders { get; set; } = new List<StreamingProvider>();

    public virtual ICollection<UserChannel> UserChannels { get; set; } = new List<UserChannel>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
