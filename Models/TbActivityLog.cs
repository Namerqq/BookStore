using System;
using System.Collections.Generic;

namespace BookStore.Models;

public partial class TbActivityLog
{
    public int LogId { get; set; }

    public int? AccountId { get; set; }

    public string Action { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual TbAccount? Account { get; set; }
}
