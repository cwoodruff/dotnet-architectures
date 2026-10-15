using Microsoft.Data.SqlClient;

namespace Chinook.Cqrs.Api.Queries;

/// <summary>The read side opens plain connections for Dapper. It never touches the DbContext.</summary>
public sealed class SqlConnectionFactory(string connectionString)
{
    public SqlConnection Create() => new(connectionString);
}
