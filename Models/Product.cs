using System;
using System.Collections.Generic;

namespace myProducts.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Quoteitem> Quoteitems { get; set; } = new List<Quoteitem>();
}
