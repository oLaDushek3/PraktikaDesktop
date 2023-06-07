using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class OrderProduct
{
    public int OrderId { get; set; }

    public int SupplyProductsId { get; set; }

    public int Quantity { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual SupplyProduct SupplyProducts { get; set; } = null!;
}
