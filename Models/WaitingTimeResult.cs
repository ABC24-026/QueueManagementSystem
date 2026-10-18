namespace QueueManagementSystem.Models
{
    public class WaitingTimeResult
    {
        public double EstimatedWaitMinutes { get; set; }
        public string CongestionLevel { get; set; } = "";
        public string FormattedWait { get; set; } = "";
        public string Source { get; set; } = "";
        public double RuleBasedMinutes { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}