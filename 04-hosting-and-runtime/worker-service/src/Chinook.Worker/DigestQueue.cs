using System.Threading.Channels;

namespace Chinook.Worker;

/// <summary>A sales digest computed by the producer and handed to the consumer.</summary>
public sealed record SalesDigest(DateTime WindowStartUtc, DateTime WindowEndUtc, int InvoiceCount);

/// <summary>
/// An in-process queue between the part of the worker that computes digests and the part that
/// delivers them. A bounded channel keeps a slow consumer from letting digests pile up forever.
/// </summary>
public sealed class DigestQueue
{
    private readonly Channel<SalesDigest> _channel = Channel.CreateBounded<SalesDigest>(
        new BoundedChannelOptions(capacity: 16)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true,
        });

    public ValueTask EnqueueAsync(SalesDigest digest, CancellationToken cancellationToken) =>
        _channel.Writer.WriteAsync(digest, cancellationToken);

    public IAsyncEnumerable<SalesDigest> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
