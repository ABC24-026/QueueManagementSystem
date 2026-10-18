namespace QueueManagementSystem.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Phone { get; set; } = "";
        public string TicketNumber { get; set; } = "";
        public string ServiceType { get; set; } = "";
        public string Status { get; set; } = "Waiting";
        public double LastToldMinutes { get; set; }
        public bool ApproachingSent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}