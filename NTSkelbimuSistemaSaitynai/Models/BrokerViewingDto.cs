using System;

namespace NTSkelbimuSistemaSaitynai.Models;

public class BrokerViewingDto
{
    public long IdViewing { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int Status { get; set; }
    public long FkAvailabilityidAvailability { get; set; }
    public long FkListingidListing { get; set; }
    public long? FkBuyeridUser { get; set; }
    public string? BuyerName { get; set; }
    public string? BuyerPhone { get; set; }
    public string? BuyerEmail { get; set; }
}
