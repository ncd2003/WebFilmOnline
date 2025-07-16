using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models;

public partial class UserChannel
{
    public int ChannelId { get; set; }

    public int UserId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? ProviderId { get; set; }

    public string? ApiKey { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ProductStreaming> ProductStreamings { get; set; } = new List<ProductStreaming>();

    public virtual StreamingProvider? Provider { get; set; }

    public virtual User User { get; set; } = null!;
}
