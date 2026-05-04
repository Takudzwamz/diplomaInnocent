

using System;
using System.Text.Json;
using Core.Entities.OrderAggregate;
using Microsoft.AspNetCore.Http;

namespace StorefrontRazor.Services;

public class CheckoutService
{
    private const string AddressSessionKey = "CheckoutAddress";
    private const string ShippingPriceSessionKey = "CheckoutShippingPrice";
    private const string SaveAddressSessionKey = "CheckoutSaveAddress";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession Session => _httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("HttpContext or Session is not available.");

    public CheckoutService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // This property is now a "smart property". When you get it, it reads from the session.
    // When you set it, it automatically saves to the session.
    public ShippingAddress ShippingAddress
    {
        get
        {
            var json = Session.GetString(AddressSessionKey);
            if (json == null)
            {
                return new ShippingAddress { Name = "", LastName = "", Line1 = "", City = "", State = "", PostalCode = "", Country = "" };
            }

            // JsonSerializer.Deserialize can return null; ensure we never return null.
            return JsonSerializer.Deserialize<ShippingAddress>(json) 
                   ?? new ShippingAddress { Name = "", LastName = "", Line1 = "", City = "", State = "", PostalCode = "", Country = "" };
        }
        set
        {
            Session.SetString(AddressSessionKey, JsonSerializer.Serialize(value));
        }
    }

    public decimal ShippingPrice
    {
        get
        {
            // Reads the price in cents and converts back to decimal
            return (Session.GetInt32(ShippingPriceSessionKey) ?? 0) / 100m;
        }
        private set
        {
            // Stores the price as an integer of cents to avoid floating point issues
            Session.SetInt32(ShippingPriceSessionKey, (int)(value * 100));
        }
    }
    
    public bool SaveAddress
    {
        get
        {
            var value = Session.GetString(SaveAddressSessionKey);
            return string.IsNullOrEmpty(value) || bool.Parse(value); // Defaults to true
        }
        set
        {
            Session.SetString(SaveAddressSessionKey, value.ToString());
        }
    }

    public void SetShippingPrice(decimal price)
    {
        ShippingPrice = price;
    }
}

