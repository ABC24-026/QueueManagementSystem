namespace QueueManagementSystem.Models
{
    public class SmsMessage
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Phone { get; set; } = "";
        public string Event { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "";
    }
}