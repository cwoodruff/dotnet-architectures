using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Chinook.EventSourcing.Api.EventStore;

public sealed class ConcurrencyException(Guid streamId, int expectedVersion)
    : Exception($"Stream {streamId} has moved past version {expectedVersion}.");

/// <summary>
/// A hand-rolled event store: one table, two operations. Append writes events at the next
/// versions inside a transaction; the unique (StreamId, Version) key turns a lost race into a
/// <see cref="ConcurrencyException"/>. Load reads a stream back in version order.
/// </summary>
public sealed class SqlEventStore(string connectionString, EventTypeMap types)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task AppendAsync(Guid streamId, int expectedVersion, IReadOnlyList<object> events, CancellationToken cancellationToken = default)
    {
        if (events.Count == 0)
        {
            return;
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var version = expectedVersion;
            foreach (var @event in events)
            {
                version++;
                await using var command = new SqlCommand(
                    """
                    INSERT INTO dbo.Events (StreamId, Version, EventType, Payload, OccurredAt)
                    VALUES (@StreamId, @Version, @EventType, @Payload, SYSUTCDATETIME());
                    """,
                    connection,
                    transaction);
                command.Parameters.Add("@StreamId", System.Data.SqlDbType.UniqueIdentifier).Value = streamId;
                command.Parameters.Add("@Version", System.Data.SqlDbType.Int).Value = version;
                command.Parameters.Add("@EventType", System.Data.SqlDbType.NVarChar, 200).Value = types.NameOf(@event.GetType());
                command.Parameters.Add("@Payload", System.Data.SqlDbType.NVarChar, -1).Value =
                    JsonSerializer.Serialize(@event, @event.GetType(), JsonOptions);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // Duplicate key on (StreamId, Version): someone else appended first.
            await transaction.RollbackAsync(cancellationToken);
            throw new ConcurrencyException(streamId, expectedVersion);
        }
    }

    public async Task<IReadOnlyList<object>> LoadAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(
            "SELECT EventType, Payload FROM dbo.Events WHERE StreamId = @StreamId ORDER BY Version;",
            connection);
        command.Parameters.Add("@StreamId", System.Data.SqlDbType.UniqueIdentifier).Value = streamId;

        var events = new List<object>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var type = types.TypeOf(reader.GetString(0));
            events.Add(JsonSerializer.Deserialize(reader.GetString(1), type, JsonOptions)
                ?? throw new InvalidOperationException($"Event payload for {type.Name} was null."));
        }

        return events;
    }
}
