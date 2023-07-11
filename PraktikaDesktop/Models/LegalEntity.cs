using System;
using System.Collections.Generic;

namespace PraktikaDesktop.Models;

public partial class LegalEntity
{
    public int LegalEntityId { get; set; }

    public string Organization { get; set; } = null!;

    public string CheckingAccount { get; set; } = null!;

    public string Bank { get; set; } = null!;

    public string CorrespondentAccount { get; set; } = null!;

    public string Bic { get; set; } = null!;

    public string Rrc { get; set; } = null!;

    public string Tin { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public virtual Buyer? Buyer { get; set; }
}
