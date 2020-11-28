using FluffyBunny4.DotNetCore.Collections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluffyBunny4.DotNetCore.Hosting
{
    public class QueuedHostedService<T> : BackgroundService where T : class
    {
        private readonly IBackgroundTaskQueue<T> _taskQueue;
        private readonly ILogger _logger;

        public QueuedHostedService(
            IBackgroundTaskQueue<T> taskQueue,
            ILogger<QueuedHostedService<T>> logger)
        {
            _taskQueue = taskQueue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(QueuedHostedService<T>)} Hosted Service is running.{Environment.NewLine}");
            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem =
                    await _taskQueue.DequeueAsync(stoppingToken);
                try
                {
                    await workItem(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error occurred executing {WorkItem}.", nameof(workItem));
                }
            }
        }
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(QueuedHostedService<T>)} Hosted Service is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}
