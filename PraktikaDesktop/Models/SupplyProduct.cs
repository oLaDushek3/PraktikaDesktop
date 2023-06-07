using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class SupplyProduct
{
    public int SupplyProductsId { get; set; }

    public int SupplyId { get; set; }

    public int ProductId { get; set; }

    public int Received { get; set; }

    public int Remaining { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual Product Product { get; set; } = null!;

    public virtual Supply Supply { get; set; } = null!;
}
