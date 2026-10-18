using Microsoft.EntityFrameworkCore;
using QueueManagementSystem.Data;
using QueueManagementSystem.Models;

namespace QueueManagementSystem.Services
{
    public class QueueService
    {
        private readonly AppDbContext _db;
        private readonly WaitingTimeService _wait;
        private readonly ISmsSender _sms;

        public QueueService(AppDbContext db, WaitingTimeService wait, ISmsSender sms)
        {
            _db = db;
            _wait = wait;
            _sms = sms;
        }

        public async Task<int> CheckNotificationsAsync()
        {
            var waitingTickets = await _db.Tickets
                .Where(t => t.Status == "Waiting")
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            int sent = 0;

            foreach (var ticket in waitingTickets)
            {
                int peopleAhead = waitingTickets.Count(t => t.CreatedAt < ticket.CreatedAt);

                var snapshot = new QueueSnapshot
                {
                    ServiceType = ticket.ServiceType,
                    PeopleAhead = peopleAhead,
                    ActiveCounters = 3,
                    BusyCounters = 2,
                    AverageServiceMinutes = 4.5,
                    ArrivalsLast15 = 5,
                    Timestamp = DateTimeOffset.UtcNow
                };

                var result = _wait.Estimate(snapshot);

                var notification = _wait.CheckNotification(
                    result,
                    ticket.LastToldMinutes,
                    ticket.ApproachingSent);

                if (notification == null) continue;

                string text = SmsTextBuilder.Build(
                    notification.Value,
                    venue: "Sesigo Queue",
                    ticket: ticket.TicketNumber,
                    position: peopleAhead + 1,
                    waitText: result.FormattedWait,
                    oldWaitText: $"{ticket.LastToldMinutes:0} min");

                await _sms.SendAsync(ticket, notification.Value.ToString(), text);

                if (notification == SmsEvent.Approaching)
                    ticket.ApproachingSent = true;

                ticket.LastToldMinutes = result.EstimatedWaitMinutes;
                sent++;
            }

            if (sent > 0)
                await _db.SaveChangesAsync();

            return sent;
        }

        public async Task<Ticket> JoinQueueAsync(string phone, string serviceType)
        {
            var ticket = new Ticket
            {
                Phone = phone,
                ServiceType = serviceType,
                TicketNumber = GenerateTicketNumber(),
                Status = "Waiting",
                CreatedAt = DateTime.UtcNow,
                LastToldMinutes = 0,
                ApproachingSent = false
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();

            int peopleAhead = await _db.Tickets
                .CountAsync(t => t.Status == "Waiting" && t.CreatedAt < ticket.CreatedAt);

            var snapshot = new QueueSnapshot
            {
                ServiceType = serviceType,
                PeopleAhead = peopleAhead,
                ActiveCounters = 3,
                BusyCounters = 2,
                AverageServiceMinutes = 4.5,
                ArrivalsLast15 = 5,
                Timestamp = DateTimeOffset.UtcNow
            };

            var result = _wait.Estimate(snapshot);

            string text = SmsTextBuilder.Build(
                SmsEvent.Joined,
                "Sesigo Queue",
                ticket.TicketNumber,
                position: peopleAhead + 1,
                waitText: result.FormattedWait);

            await _sms.SendAsync(ticket, "Joined", text);

            ticket.LastToldMinutes = result.EstimatedWaitMinutes;
            await _db.SaveChangesAsync();

            return ticket;
        }

        private string GenerateTicketNumber()
        {
            return "T" + DateTime.Now.ToString("HHmmss") + new Random().Next(10, 99);
        }
    }
}