using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class Supply
{
    public int SupplyId { get; set; }

    public DateOnly Date { get; set; }

    public virtual ICollection<SupplyProduct> SupplyProducts { get; set; } = new List<SupplyProduct>();
}
