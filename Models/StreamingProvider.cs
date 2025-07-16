using System;
using System.Collections.Generic;

namespace WebFilmOnline.Models;

public partial class StreamingProvider
{
    public int ProviderId { get; set; }

    public string? Name { get; set; }

    public string? ApiKey { get; set; }

    public string? BaseUrl { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<ProductStreaming> ProductStreamings { get; set; } = new List<ProductStreaming>();

    public virtual ICollection<UserChannel> UserChannels { get; set; } = new List<UserChannel>();
}
