using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly Date { get; set; }

    public int BuyerId { get; set; }

    public string Status { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal? Delivery { get; set; }

    public decimal? Assembly { get; set; }

    public virtual Buyer Buyer { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
