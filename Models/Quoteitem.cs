using System;
using System.Collections.Generic;

namespace myProducts.Models;

public partial class Quoteitem
{
    public int QuoteItemId { get; set; }

    public int QuoteId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Subtotal { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Quote Quote { get; set; } = null!;
}
