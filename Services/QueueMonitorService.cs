namespace QueueManagementSystem.Services
{
    public class QueueMonitorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<QueueMonitorService> _log;

        public QueueMonitorService(IServiceScopeFactory scopes, ILogger<QueueMonitorService> log)
        {
            _scopes = scopes;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                    using var scope = _scopes.CreateScope();
                    var queue = scope.ServiceProvider.GetRequiredService<QueueService>();

                    int sent = await queue.CheckNotificationsAsync();

                    if (sent > 0)
                        _log.LogInformation("Queue monitor sent {Count} notification(s).", sent);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Queue monitor failed; it will try again in 30 seconds.");
                }
            }
        }
    }
}
