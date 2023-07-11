using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class Supply
{
    public int SupplyId { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;

    public virtual ICollection<SupplyProduct> SupplyProducts { get; set; } = new List<SupplyProduct>();
}