using Chinook.Worker.Data;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Worker;

/// <summary>
/// The producer. Every thirty seconds it counts the invoices created in the last 24 hours and
/// puts a digest on the queue.
/// </summary>
public sealed class SalesDigestWorker(
    IDbContextFactory<ChinookDbContext> dbContextFactory,
    DigestQueue queue,
    TimeProvider timeProvider,
    ILogger<SalesDigestWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval, timeProvider);
        do
        {
            try
            {
                var end = timeProvider.GetUtcNow().UtcDateTime;
                var start = end.AddHours(-24);

                await using var db = await dbContextFactory.CreateDbContextAsync(stoppingToken);
                var count = await db.Invoices.CountAsync(
                    i => i.InvoiceDate >= start && i.InvoiceDate < end, stoppingToken);

                logger.LogInformation(
                    "Sales digest: {InvoiceCount} invoices created between {Start:u} and {End:u}. " +
                    "Zero is expected against the Chinook sample data, whose invoices are dated 2007-2010.",
                    count, start, end);

                await queue.EnqueueAsync(new SalesDigest(start, end, count), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Sales digest failed; will retry on the next tick.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
