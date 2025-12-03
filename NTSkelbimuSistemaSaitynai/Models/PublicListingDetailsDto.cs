namespace NTSkelbimuSistemaSaitynai.Models;

public class PublicListingDetailsDto : PublicListingDto
{
    public double? ApartmentArea { get; set; }
    public int? Rooms { get; set; }
    public long? ApartmentId { get; set; }
    public long? BuildingId { get; set; }
    public string? BuildingEnergyClass { get; set; }
    public int? BuildingFloors { get; set; }
    public int? BuildingYear { get; set; }
    public int? BuildingLastRenovationYear { get; set; }
    public int? ApartmentNumber { get; set; }
    public int? ApartmentFloor { get; set; }
    public bool? ApartmentIsWholeBuilding { get; set; }
    public string? ApartmentNotes { get; set; }
    public string? ApartmentHeating { get; set; }
    public string? ApartmentFinish { get; set; }
    public string? BrokerName { get; set; }
    public string? BrokerPhone { get; set; }
    public List<string> GalleryPictureIds { get; set; } = new();
    public List<string> GalleryPictureUrls { get; set; } = new();
    public IEnumerable<PublicAvailabilityDto> Availabilities { get; set; } = Array.Empty<PublicAvailabilityDto>();
    public List<PublicAvailabilitySlotDto> AvailableSlots { get; set; } = new();
}
