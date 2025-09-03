using System;
using System.Collections.Generic;

namespace myProducts.Models;

public partial class Quote
{
    public int QuoteId { get; set; }

    public int ClientId { get; set; }

    public int UserId { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Quoteitem> Quoteitems { get; set; } = new List<Quoteitem>();

    public virtual User User { get; set; } = null!;
}
