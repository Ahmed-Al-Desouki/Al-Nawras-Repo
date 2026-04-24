using Al_Nawras.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Infrastructure.BackgroundJobs
{
    public class OverduePaymentHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OverduePaymentHostedService> _logger;
        private readonly TimeOnly _runAt;

        // Run time — 2:00 AM UTC every day
        private static readonly TimeOnly RunAt = new(2, 0, 0);

        public OverduePaymentHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<OverduePaymentHostedService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var section = configuration.GetSection("BackgroundJobs:OverduePayments");
            var hour = section.GetValue<int>("RunAtUtcHour", 2);
            var minute = section.GetValue<int>("RunAtUtcMinute", 0);

            _runAt = new TimeOnly(hour, minute, 0);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "OverduePaymentHostedService started. Runs daily at {RunAt} UTC.", RunAt);

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = CalculateDelayUntilNextRun();

                _logger.LogInformation(
                    "OverduePaymentHostedService: next run in {Hours}h {Minutes}m.",
                    (int)delay.TotalHours, delay.Minutes);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // App is shutting down — exit cleanly
                    break;
                }

                if (stoppingToken.IsCancellationRequested)
                    break;

                await RunJobSafelyAsync(stoppingToken);
            }

            _logger.LogInformation("OverduePaymentHostedService stopped.");
        }

        private async Task RunJobSafelyAsync(CancellationToken stoppingToken)
        {
            // BackgroundService runs as a singleton — must create a scope
            // to resolve scoped services (DbContext, repositories)
            using var scope = _scopeFactory.CreateScope();

            try
            {
                var job = scope.ServiceProvider.GetRequiredService<IOverduePaymentJob>();
                await job.RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Never let an unhandled exception kill the hosted service
                _logger.LogError(ex,
                    "OverduePaymentHostedService: unhandled exception during job run.");
            }
        }

        private TimeSpan CalculateDelayUntilNextRun()
        {
            var now = DateTime.UtcNow;
            var nextRun = DateTime.UtcNow.Date.Add(_runAt.ToTimeSpan());

            if (now >= nextRun)
                nextRun = nextRun.AddDays(1);

            return nextRun - now;
        }
    }
}
