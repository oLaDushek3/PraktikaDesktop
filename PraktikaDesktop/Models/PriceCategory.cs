using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class PriceCategory
{
    public int PriceCategoryId { get; set; }

    public string Category { get; set; } = null!;

    public virtual ICollection<ProductPriceCategory> ProductPriceCategories { get; set; } = new List<ProductPriceCategory>();

    public virtual ICollection<Textile> Textiles { get; set; } = new List<Textile>();
}
