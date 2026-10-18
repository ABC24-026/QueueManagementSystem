namespace QueueManagementSystem.Services
{
    public enum SmsEvent
    {
        Joined,
        Approaching,
        Delay,
        YourTurn
    }

    public static class SmsTextBuilder
    {
        public static string Build(
            SmsEvent ev,
            string venue,
            string ticket,
            int? position = null,
            string? waitText = null,
            string? oldWaitText = null,
            int? counter = null)
        {
            string msg = ev switch
            {
                SmsEvent.Joined => $"{venue}: You are in the queue. Ticket {ticket}, position {position}. Est. wait: {waitText}. We will SMS you when your turn is near.",
                SmsEvent.Approaching => $"{venue}: Ticket {ticket}, your turn is approaching (est. {waitText}). Please come to the service area.",
                SmsEvent.Delay => $"{venue}: Delay for ticket {ticket}. New est. wait: {waitText} (was {oldWaitText}). Thank you for your patience.",
                SmsEvent.YourTurn => $"{venue}: Ticket {ticket}, it is your turn. Please go to counter {counter}.",
                _ => throw new ArgumentOutOfRangeException(nameof(ev))
            };

            if (ev == SmsEvent.Joined && msg.Length > 160)
                msg = $"{venue}: Queue joined. Ticket {ticket}, position {position}. Est. wait: {waitText}.";

            return msg.Length <= 160 ? msg : msg[..160];
        }
    }
}