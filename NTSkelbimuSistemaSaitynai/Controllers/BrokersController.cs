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
    public class BrokersController : ControllerBase
    {
        private readonly PostgresContext _context;

        public BrokersController(PostgresContext context)
        {
            _context = context;
        }

        // GET: api/Brokers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Broker>>> GetBrokers()
        {
            return await _context.Brokers.ToListAsync();
        }

        // GET: api/Brokers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Broker>> GetBroker(long id)
        {
            var broker = await _context.Brokers.FindAsync(id);

            if (broker == null)
            {
                return NotFound();
            }

            return broker;
        }

        // PUT: api/Brokers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBroker(long id, Broker broker)
        {
            broker.IdUser = id;

            _context.Entry(broker).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BrokerExists(id))
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

        // POST: api/Brokers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Broker>> PostBroker(Broker broker)
        {
            _context.Brokers.Add(broker);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BrokerExists(broker.IdUser))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetBroker", new { id = broker.IdUser }, broker);
        }

        // DELETE: api/Brokers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBroker(long id)
        {
            var broker = await _context.Brokers.FindAsync(id);
            if (broker == null)
            {
                return NotFound();
            }

            _context.Brokers.Remove(broker);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BrokerExists(long id)
        {
            return _context.Brokers.Any(e => e.IdUser == id);
        }
    }
}
