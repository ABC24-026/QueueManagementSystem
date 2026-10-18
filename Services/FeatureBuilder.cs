using QueueManagementSystem.Models;

namespace QueueManagementSystem.Services
{
    public static class FeatureBuilder
    {
        public static WaitModelInput Create(
            string serviceType,
            int dayOfWeek,
            double hour,
            double queueLength,
            double busyCounters,
            double activeCounters,
            double avgServiceRecent,
            double arrivalsLast15)
        {
            double counters = Math.Max(activeCounters, 1);

            return new WaitModelInput
            {
                ServiceType = serviceType,
                DayOfWeek = dayOfWeek,
                Hour = (float)hour,
                QueueLength = (float)queueLength,
                BusyCounters = (float)busyCounters,
                ActiveCounters = (float)activeCounters,
                AvgServiceRecent = (float)avgServiceRecent,
                ArrivalsLast15 = (float)arrivalsLast15,
                QueuePerCounter = (float)(queueLength / counters),
                WorkloadEstimate = (float)(queueLength * avgServiceRecent / counters),
                UtilisationNow = (float)Math.Min(busyCounters / counters, 1.0)
            };
        }
    }
}