using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class Buyer
{
    public int BuyerId { get; set; }

    public string Address { get; set; } = null!;

    public int? IndividualId { get; set; }

    public int? LegalEntityId { get; set; }

    public virtual Individual? Individual { get; set; }

    public virtual LegalEntity? LegalEntity { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
