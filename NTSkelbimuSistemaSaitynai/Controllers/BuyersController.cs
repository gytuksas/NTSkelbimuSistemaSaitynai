using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;
using NTSkelbimuSistemaSaitynai;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyersController : ControllerBase
    {
        private readonly PostgresContext _context;

        public BuyersController(PostgresContext context)
        {
            _context = context;
        }

        // GET: api/Buyers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Buyer>>> GetBuyers()
        {
            return await _context.Buyers.ToListAsync();
        }

        // GET: api/Buyers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Buyer>> GetBuyer(long id)
        {
            var buyer = await _context.Buyers.FindAsync(id);

            if (buyer == null)
            {
                return NotFound();
            }

            return buyer;
        }

        // PUT: api/Buyers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBuyer(long id, Buyer buyer)
        {
            buyer.IdUser = id;

            _context.Entry(buyer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BuyerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Buyers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Buyer>> PostBuyer(Buyer buyer)
        {
            _context.Buyers.Add(buyer);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BuyerExists(buyer.IdUser))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetBuyer", new { id = buyer.IdUser }, buyer);
        }

        // DELETE: api/Buyers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBuyer(long id)
        {
            var buyer = await _context.Buyers.FindAsync(id);
            if (buyer == null)
            {
                return NotFound();
            }

            _context.Buyers.Remove(buyer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BuyerExists(long id)
        {
            return _context.Buyers.Any(e => e.IdUser == id);
        }
    }
}
