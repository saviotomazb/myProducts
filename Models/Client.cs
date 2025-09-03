using System;
using System.Collections.Generic;

namespace myProducts.Models;

public partial class Client
{
    public int ClientId { get; set; }

    public string FullName { get; set; } = null!;

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Street { get; set; }

    public string? District { get; set; }

    public string? Complement { get; set; }

    public string? HouseNumber { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();
}
