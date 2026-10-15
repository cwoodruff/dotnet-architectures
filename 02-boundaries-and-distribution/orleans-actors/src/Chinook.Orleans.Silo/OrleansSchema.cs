using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Chinook.Orleans.Silo;

/// <summary>
/// Creates the ChinookOrleans database and the Orleans ADO.NET tables if they are missing.
/// The scripts in database/ are the official ones from the dotnet/orleans repository; they are
/// copied to the output folder at build time and run once, in order, against an empty database.
/// </summary>
public static class OrleansSchema
{
    private static readonly string[] Scripts =
    [
        "SQLServer-Main.sql",
        "SQLServer-Clustering.sql",
        "SQLServer-Persistence.sql",
    ];

    public static async Task EnsureAsync(string connectionString, ILogger logger, CancellationToken cancellationToken = default)
    {
        var target = new SqlConnectionStringBuilder(connectionString);
        var databaseName = target.InitialCatalog;

        var master = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
        await using (var connection = new SqlConnection(master.ConnectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await using var create = connection.CreateCommand();
            create.CommandText = "IF DB_ID(@name) IS NULL EXEC('CREATE DATABASE ' + @quoted);";
            create.Parameters.AddWithValue("@name", databaseName);
            create.Parameters.AddWithValue("@quoted", "[" + databaseName.Replace("]", "]]") + "]");
            await create.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var db = new SqlConnection(connectionString);
        await db.OpenAsync(cancellationToken);

        await using (var check = db.CreateCommand())
        {
            check.CommandText = "SELECT CASE WHEN OBJECT_ID(N'dbo.OrleansQuery') IS NULL THEN 0 ELSE 1 END;";
            if ((int)(await check.ExecuteScalarAsync(cancellationToken))! == 1)
            {
                logger.LogInformation("Orleans schema already present in {Database}.", databaseName);
                return;
            }
        }

        foreach (var script in Scripts)
        {
            var file = Path.Combine(AppContext.BaseDirectory, "database", script);
            await using var command = db.CreateCommand();
            command.CommandText = await File.ReadAllTextAsync(file, cancellationToken);
            await command.ExecuteNonQueryAsync(cancellationToken);
            logger.LogInformation("Applied {Script} to {Database}.", script, databaseName);
        }
    }
}
