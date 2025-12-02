namespace NTSkelbimuSistemaSaitynai.Models;

public class PublicListingDto
{
    public long Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public double AskingPrice { get; set; }
    public bool Rent { get; set; }
    public string? BuildingCity { get; set; }
    public string? BuildingAddress { get; set; }
    public string? PictureId { get; set; }
    public string? PictureUrl { get; set; }
    public DateTime? NextViewingFrom { get; set; }
    public DateTime? NextViewingTo { get; set; }
}
