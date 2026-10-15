using Microsoft.Data.SqlClient;

namespace Chinook.EventDriven.Sales.Api;

/// <summary>
/// Wolverine provisions its own tables, but not the database they live in. This creates the
/// ChinookMessaging database on the shared SQL Server if it is missing. It runs at startup only;
/// building and testing never touch the database.
/// </summary>
internal static class MessagingDatabase
{
    public static void EnsureExists(string connectionString)
    {
        var target = new SqlConnectionStringBuilder(connectionString);
        var databaseName = target.InitialCatalog;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("The messaging connection string must name a database.");
        }

        var master = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
        using var connection = new SqlConnection(master.ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            IF DB_ID(@name) IS NULL
            BEGIN
                DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@name);
                EXEC (@sql);
            END
            """;
        command.Parameters.AddWithValue("@name", databaseName);
        command.ExecuteNonQuery();
    }
}
