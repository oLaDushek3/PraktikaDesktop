using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class ProductPriceCategory
{
    public int ProductId { get; set; }

    public int PriceCategoryId { get; set; }

    public decimal Price { get; set; }

    public virtual PriceCategory PriceCategory { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
