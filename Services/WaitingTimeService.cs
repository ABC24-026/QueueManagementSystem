using QueueManagementSystem.Ml;
using QueueManagementSystem.Models;

namespace QueueManagementSystem.Services
{
    public class WaitingTimeService
    {
        public const double UtilisationThreshold = 0.8;
        public const double UtilisationUplift = 1.15;
        public const double ApproachingMinutes = 10;
        public const double DelayRiseMinutes = 5;

        private const int MaxQueueTrained = 125;
        private const int MaxCountersTrained = 4;

        public static readonly string[] KnownServiceTypes = { "Bank", "Clinic", "Government office", "Retail" };

        private readonly IWaitPredictor _predictor;

        public WaitingTimeService(IWaitPredictor predictor)
        {
            _predictor = predictor;
        }

        public WaitingTimeResult Estimate(QueueSnapshot s)
        {
            Validate(s);
            var warnings = new List<string>();

            var when = s.Timestamp == default ? DateTimeOffset.UtcNow : s.Timestamp;
            var local = when.ToOffset(TimeSpan.FromHours(2)); // Botswana UTC+2
            double hour = local.Hour + local.Minute / 60.0;
            int dow = ((int)local.DayOfWeek + 6) % 7; // Monday = 0

            if (hour < 8 || hour >= 17)
            {
                warnings.Add("Time is outside 08:00-17:00; treated as nearest trained time.");
                hour = Math.Min(Math.Max(hour, 8.0), 16.99);
            }
            if (dow > 4)
            {
                warnings.Add("Weekends treated as Friday.");
                dow = 4;
            }

            int busy = Math.Min(s.BusyCounters, s.ActiveCounters);
            double rule = RuleBased(s.PeopleAhead, s.ActiveCounters, busy, s.AverageServiceMinutes);
            double minutes = rule;
            string source = "rule_based";

            string? canonical = KnownServiceTypes.FirstOrDefault(t =>
                string.Equals(t, s.ServiceType, StringComparison.OrdinalIgnoreCase));

            if (canonical == null)
            {
                warnings.Add($"Unknown service type '{s.ServiceType}'. Using rule-based estimate.");
            }
            else
            {
                var input = FeatureBuilder.Create(
                    canonical, dow, hour,
                    s.PeopleAhead, busy, s.ActiveCounters,
                    s.AverageServiceMinutes, s.ArrivalsLast15);

                float? predicted = _predictor.Predict(input);

                if (predicted.HasValue)
                {
                    minutes = Math.Max(predicted.Value, 0);
                    source = "model";

                    if (s.PeopleAhead > MaxQueueTrained || s.ActiveCounters > MaxCountersTrained)
                    {
                        minutes = Math.Max(minutes, rule);
                        source = "model_extrapolated";
                        warnings.Add("Queue or counters exceed training data; using higher of model and rule.");
                    }
                }
                else
                {
                    warnings.Add("No trained model loaded; using rule-based estimate.");
                }
            }

            minutes = Math.Round(minutes, 1, MidpointRounding.AwayFromZero);

            return new WaitingTimeResult
            {
                EstimatedWaitMinutes = minutes,
                CongestionLevel = GetCongestionLevel(minutes),
                FormattedWait = FormatWait(minutes),
                Source = source,
                RuleBasedMinutes = Math.Round(rule, 1, MidpointRounding.AwayFromZero),
                Warnings = warnings
            };
        }

        public static double RuleBased(int peopleAhead, int activeCounters, int busyCounters, double averageServiceMinutes)
        {
            double estimate = peopleAhead * averageServiceMinutes / Math.Max(activeCounters, 1);
            double utilisation = (double)busyCounters / Math.Max(activeCounters, 1);
            return utilisation > UtilisationThreshold ? estimate * UtilisationUplift : estimate;
        }

        public SmsEvent? CheckNotification(WaitingTimeResult current, double lastToldMinutes, bool approachingAlreadySent)
        {
            if (current.EstimatedWaitMinutes >= lastToldMinutes + DelayRiseMinutes)
                return SmsEvent.Delay;

            if (!approachingAlreadySent && current.EstimatedWaitMinutes <= ApproachingMinutes)
                return SmsEvent.Approaching;

            return null;
        }

        public static string GetCongestionLevel(double minutes)
        {
            if (minutes < 10) return "Low";
            if (minutes < 30) return "Moderate";
            if (minutes < 60) return "High";
            return "Very high";
        }

        public static string FormatWait(double minutes)
        {
            if (minutes < 1) return "less than 1 min";
            if (minutes < 10) return $"about {Math.Round(minutes, MidpointRounding.AwayFromZero)} min";
            if (minutes < 57.5) return $"about {5 * Math.Round(minutes / 5, MidpointRounding.AwayFromZero)} min";
            return minutes < 75 ? "about 1 hour" : "over 1 hour";
        }

        private static void Validate(QueueSnapshot s)
        {
            if (string.IsNullOrWhiteSpace(s.ServiceType))
                throw new ArgumentException("ServiceType is required.");
            if (s.PeopleAhead < 0)
                throw new ArgumentOutOfRangeException(nameof(s.PeopleAhead));
            if (s.ActiveCounters < 1)
                throw new ArgumentOutOfRangeException(nameof(s.ActiveCounters));
            if (s.BusyCounters < 0)
                throw new ArgumentOutOfRangeException(nameof(s.BusyCounters));
            if (s.AverageServiceMinutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(s.AverageServiceMinutes));
        }
    }
}