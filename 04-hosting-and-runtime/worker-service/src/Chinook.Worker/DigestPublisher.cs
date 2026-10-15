namespace Chinook.Worker;

/// <summary>
/// The consumer. It drains the queue and "delivers" each digest. Today that means a log line;
/// an email or a message to another system would go here.
/// </summary>
public sealed class DigestPublisher(DigestQueue queue, ILogger<DigestPublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var digest in queue.ReadAllAsync(stoppingToken))
        {
            logger.LogInformation(
                "Delivered digest for {Start:u} to {End:u}: {InvoiceCount} invoices.",
                digest.WindowStartUtc, digest.WindowEndUtc, digest.InvoiceCount);
        }
    }
}
