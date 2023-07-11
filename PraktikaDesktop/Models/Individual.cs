using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class Individual
{
    public int IndividualId { get; set; }

    public string FullName { get; set; } = null!;

    public string SeriesPassportNumber { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public virtual Buyer? Buyer { get; set; }
}
