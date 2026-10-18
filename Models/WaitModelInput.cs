namespace QueueManagementSystem.Models
{
    public class WaitModelInput
    {
        public string ServiceType { get; set; } = "";
        public float DayOfWeek { get; set; }
        public float Hour { get; set; }
        public float QueueLength { get; set; }
        public float BusyCounters { get; set; }
        public float ActiveCounters { get; set; }
        public float AvgServiceRecent { get; set; }
        public float ArrivalsLast15 { get; set; }
        public float QueuePerCounter { get; set; }
        public float WorkloadEstimate { get; set; }
        public float UtilisationNow { get; set; }
        public float WaitMinutes { get; set; }
    }

    public class WaitModelOutput
    {
        public float Score { get; set; }
    }
}