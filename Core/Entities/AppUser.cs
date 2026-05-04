using System;
using Microsoft.AspNetCore.Identity;

namespace Core.Entities;

public class AppUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public Address? Address { get; set; }

    public List<ProductReview> Reviews { get; set; } = [];

    public Wishlist? Wishlist { get; set; }

    public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
}



