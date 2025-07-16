using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models;

public partial class ProductStreaming
{
    public int ProductStreamingId { get; set; }

    public int ProductId { get; set; }

    public int? ProviderId { get; set; }

    public int? ChannelId { get; set; }

    public int? Priority { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual UserChannel? Channel { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual StreamingProvider? Provider { get; set; }
}
