using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class SupplyProduct
{
    public int SupplyProductsId { get; set; }

    public int SupplyId { get; set; }

    public int ProductId { get; set; }

    public string? Status { get; set; }

    public string? ListStatus { internal get; set; }

    public int? TextileId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Supply Supply { get; set; } = null!;

    public virtual Textile? Textile { get; set; }

    public virtual List<Order> Orders { get; set; } = new List<Order>();
}
