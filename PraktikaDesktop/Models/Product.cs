using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int ProductTypeId { get; set; }

    public int? ColorId { get; set; }

    public string Dimensions { get; set; } = null!;

    public decimal? Price { get; set; }

    public string Name { get; set; } = null!;

    public virtual Color? Color { get; set; }

    public virtual ICollection<ProductPriceCategory> ProductPriceCategories { get; set; } = new List<ProductPriceCategory>();

    public virtual ProductType ProductType { get; set; } = null!;

    public virtual ICollection<SupplyProduct> SupplyProducts { get; set; } = new List<SupplyProduct>();
}
