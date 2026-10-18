namespace QueueManagementSystem.Models
{
    public class QueueSnapshot
    {
        public string ServiceType { get; set; } = "";
        public int PeopleAhead { get; set; }
        public int ActiveCounters { get; set; }
        public int BusyCounters { get; set; }
        public double AverageServiceMinutes { get; set; }
        public double ArrivalsLast15 { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}