using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class Textile
{
    public int TextileId { get; set; }

    public int PriceCategoryId { get; set; }

    public string Name { get; set; } = null!;

    public virtual PriceCategory PriceCategory { get; set; } = null!;

    public virtual ICollection<SupplyProduct> SupplyProducts { get; set; } = new List<SupplyProduct>();
}
