using Microsoft.AspNetCore.Mvc;
using QueueManagementSystem.Services;

namespace QueueManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly QueueService _queue;

        public TicketsController(QueueService queue)
        {
            _queue = queue;
        }

        // Customer joins the queue
        // POST /api/tickets/join
        [HttpPost("join")]
        public async Task<IActionResult> Join([FromBody] JoinRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.ServiceType))
            {
                return BadRequest(new { error = "Phone and ServiceType are required." });
            }

            var ticket = await _queue.JoinQueueAsync(request.Phone, request.ServiceType);

            return Ok(new
            {
                ticket.TicketNumber,
                ticket.Phone,
                ticket.ServiceType,
                Message = "You have joined the queue. Check your SMS."
            });
        }
    }

    public class JoinRequest
    {
        public string Phone { get; set; } = "";
        public string ServiceType { get; set; } = "";
    }
}