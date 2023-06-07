using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class ProductType
{
    public int ProductTypeId { get; set; }

    public string Name { get; set; } = null!;

    public int ProductGroupId { get; set; }

    public virtual ProductGroup ProductGroup { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
