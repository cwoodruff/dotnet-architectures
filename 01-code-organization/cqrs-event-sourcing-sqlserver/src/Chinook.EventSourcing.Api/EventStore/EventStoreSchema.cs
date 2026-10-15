using Microsoft.Data.SqlClient;

namespace Chinook.EventSourcing.Api.EventStore;

/// <summary>
/// Creates the ChinookEvents database and its Events table if they are missing. Runs once at
/// startup, never at build time, so the project builds without SQL Server.
/// </summary>
public static class EventStoreSchema
{
    public static async Task EnsureCreatedAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var target = new SqlConnectionStringBuilder(connectionString);
        var databaseName = target.InitialCatalog;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("The ChinookEvents connection string must name a database.");
        }

        var master = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
        await using (var connection = new SqlConnection(master.ConnectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(
                """
                IF DB_ID(@name) IS NULL
                BEGIN
                    DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@name);
                    EXEC (@sql);
                END
                """,
                connection);
            command.Parameters.AddWithValue("@name", databaseName);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(ReadDdl(), connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static string ReadDdl()
    {
        using var stream = typeof(EventStoreSchema).Assembly.GetManifestResourceStream("chinook-events.sql")
            ?? throw new InvalidOperationException("Embedded resource chinook-events.sql is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
