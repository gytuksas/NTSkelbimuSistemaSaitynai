namespace NTSkelbimuSistemaSaitynai.Models;

public class PublicListingDetailsDto : PublicListingDto
{
    public double? ApartmentArea { get; set; }
    public int? Rooms { get; set; }
    public long? ApartmentId { get; set; }
    public long? BuildingId { get; set; }
    public string? BrokerName { get; set; }
    public string? BrokerPhone { get; set; }
    public List<string> GalleryPictureIds { get; set; } = new();
    public List<string> GalleryPictureUrls { get; set; } = new();
    public IEnumerable<PublicAvailabilityDto> Availabilities { get; set; } = Array.Empty<PublicAvailabilityDto>();
}
