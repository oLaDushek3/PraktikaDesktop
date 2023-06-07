using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int RoleId { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Role Role { get; set; } = null!;
}
