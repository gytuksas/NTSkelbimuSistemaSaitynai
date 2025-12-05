using System;

namespace NTSkelbimuSistemaSaitynai.Models;

public class BuyerViewingDto
{
    public long Id { get; set; }
    public long ListingId { get; set; }
    public string? ListingTitle { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int StatusId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PictureId { get; set; }
    public string? PictureUrl { get; set; }
    public string? BrokerName { get; set; }
    public string? BrokerPhone { get; set; }
}
