using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string Name { get; set; } = null!;

    public int AccessLevel { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
