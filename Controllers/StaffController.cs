using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueManagementSystem.Data;
using QueueManagementSystem.Models;
using QueueManagementSystem.Services;

namespace QueueManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(StaffPinFilter))]
    public class StaffController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ISmsSender _sms;

        public StaffController(AppDbContext db, ISmsSender sms)
        {
            _db = db;
            _sms = sms;
        }

        // GET /api/staff/waiting
        [HttpGet("waiting")]
        public async Task<IActionResult> GetWaiting()
        {
            var list = await _db.Tickets
                .Where(t => t.Status == "Waiting")
                .OrderBy(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.TicketNumber,
                    t.Phone,
                    t.ServiceType,
                    t.CreatedAt,
                    t.LastToldMinutes
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET /api/staff/sms
        [HttpGet("sms")]
        public async Task<IActionResult> GetSmsLog()
        {
            var list = await _db.SmsMessages
                .OrderByDescending(m => m.CreatedAt)
                .Take(100)
                .ToListAsync();

            return Ok(list);
        }

        // POST /api/staff/call-next/{id}
        [HttpPost("call-next/{id}")]
        public async Task<IActionResult> CallNext(int id)
        {
            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null || ticket.Status != "Waiting")
                return NotFound(new { error = "Ticket not found or not waiting." });

            ticket.Status = "Serving";
            await _db.SaveChangesAsync();

            // Send "Your Turn" SMS
            string text = SmsTextBuilder.Build(
                SmsEvent.YourTurn,
                "Sesigo Queue",
                ticket.TicketNumber,
                counter: 1);

            await _sms.SendAsync(ticket, "YourTurn", text);

            return Ok(new { message = $"Ticket {ticket.TicketNumber} is now being served." });
        }

        // POST /api/staff/complete/{id}
        [HttpPost("complete/{id}")]
        public async Task<IActionResult> Complete(int id)
        {
            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound(new { error = "Ticket not found." });

            ticket.Status = "Completed";
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Ticket {ticket.TicketNumber} marked as completed." });
        }
    }
}