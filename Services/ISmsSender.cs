using QueueManagementSystem.Data;
using QueueManagementSystem.Models;

namespace QueueManagementSystem.Services
{
    public interface ISmsSender
    {
        Task SendAsync(Ticket ticket, string eventName, string text);
    }

    public class LoggingSmsSender : ISmsSender
    {
        private readonly AppDbContext _db;
        private readonly ILogger<LoggingSmsSender> _log;

        public LoggingSmsSender(AppDbContext db, ILogger<LoggingSmsSender> log)
        {
            _db = db;
            _log = log;
        }

        public async Task SendAsync(Ticket ticket, string eventName, string text)
        {
            _db.SmsMessages.Add(new SmsMessage
            {
                TicketId = ticket.Id,
                Phone = ticket.Phone,
                Event = eventName,
                Text = text,
                CreatedAt = DateTime.UtcNow,
                Status = "Logged (SMS gateway not connected)"
            });

            await _db.SaveChangesAsync();

            _log.LogInformation("SMS to {Phone} [{Event}]: {Text}", ticket.Phone, eventName, text);
        }
    }
}