using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace NTSkelbimuSistemaSaitynai.Authorization
{
    /// <summary>
    /// Centralized ownership resolution helpers to verify broker ownership of resources.
    /// </summary>
    public class OwnershipService
    {
        private readonly PostgresContext _context;
        public OwnershipService(PostgresContext context) => _context = context;

        public long? GetCurrentUserId(ClaimsPrincipal user)
        {
            var idClaim = user.FindFirst("id")?.Value;
            return long.TryParse(idClaim, out var id) ? id : null;
        }

        public bool IsAdmin(ClaimsPrincipal user) => user.IsInRole("Administrator");

        public async Task<bool> BrokerOwnsBuilding(long brokerUserId, long buildingId) =>
            await _context.Buildings.AnyAsync(b => b.IdBuilding == buildingId && b.FkBrokeridUser == brokerUserId);

        public async Task<bool> BrokerOwnsApartment(long brokerUserId, long apartmentId) =>
            await _context.Apartments
                .Where(a => a.IdApartment == apartmentId)
                .Join(_context.Buildings,
                      a => a.FkBuildingidBuilding,
                      b => b.IdBuilding,
                      (a, b) => new { a, b })
                .AnyAsync(x => x.b.FkBrokeridUser == brokerUserId);

        public async Task<bool> BrokerOwnsPicture(long brokerUserId, string pictureId) =>
            await _context.Pictures
                .Where(p => p.Id == pictureId)
                .Join(_context.Apartments,
                      p => p.FkApartmentidApartment,
                      a => a.IdApartment,
                      (p, a) => new { p, a })
                .Join(_context.Buildings,
                      pa => pa.a.FkBuildingidBuilding,
                      b => b.IdBuilding,
                      (pa, b) => new { pa.p, b })
                .AnyAsync(x => x.b.FkBrokeridUser == brokerUserId);

        public async Task<bool> BrokerOwnsListing(long brokerUserId, long listingId) =>
            await _context.Listings
                .Where(l => l.IdListing == listingId)
                .Join(_context.Pictures,
                      l => l.FkPictureid,
                      p => p.Id,
                      (l, p) => new { l, p })
                .Join(_context.Apartments,
                      lp => lp.p.FkApartmentidApartment,
                      a => a.IdApartment,
                      (lp, a) => new { lp.l, a })
                .Join(_context.Buildings,
                      la => la.a.FkBuildingidBuilding,
                      b => b.IdBuilding,
                      (la, b) => new { la.l, b })
                .AnyAsync(x => x.b.FkBrokeridUser == brokerUserId);

        public async Task<bool> BrokerOwnsAvailability(long brokerUserId, long availabilityId) =>
            await _context.Availabilities.AnyAsync(a => a.IdAvailability == availabilityId && a.FkBrokeridUser == brokerUserId);

        public async Task<bool> BrokerOwnsViewing(long brokerUserId, long viewingId) =>
            await _context.Viewings
                .Where(v => v.IdViewing == viewingId)
                .Join(_context.Availabilities,
                      v => v.FkAvailabilityidAvailability,
                      a => a.IdAvailability,
                      (v, a) => new { v, a })
                .AnyAsync(x => x.a.FkBrokeridUser == brokerUserId);
    }
}
