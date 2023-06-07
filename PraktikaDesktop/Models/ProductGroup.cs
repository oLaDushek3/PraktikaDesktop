using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class ProductGroup
{
    public int ProductGroupId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ProductType> ProductTypes { get; set; } = new List<ProductType>();
}
