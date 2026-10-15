using System;
using System.Collections.Generic;

namespace Chinook.ModMono.Modules.Sales.Data.Entities;

public partial class InvoiceLine
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    public int TrackId { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;
}
