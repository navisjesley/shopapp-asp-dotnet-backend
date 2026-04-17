using System.Data;
using UserAPI.Contracts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace UserAPI.Data;

public sealed class UserBootstrapRepository(IConfiguration config) : IUserBootstrapRepository
{
    private readonly string _connectionString = config.GetConnectionString("UserDb")!;
    
    public async Task<UserBootstrapResponse> UpsertFederatedUserOnLoginAsync(UserBootstrapRequest request, string provider,
    string providerSubId, Guid providerOId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.usp_UpsertFederatedUserOnLogin";
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 30;

        command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200){Value = NormalizeOrDbNull(request.Name)});
        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320){Value = NormalizeOrDbNull(request.Email)});
        command.Parameters.Add(new SqlParameter("@Provider", SqlDbType.NVarChar, 50){Value = NormalizeOrDbNull(provider)});
        command.Parameters.Add(new SqlParameter("@ProviderSubId", SqlDbType.NVarChar, 255){Value = NormalizeOrDbNull(providerSubId)});
        command.Parameters.Add(new SqlParameter("@ProviderOId", SqlDbType.UniqueIdentifier){Value = providerOId});

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("The stored procedure did not return the bootstrapped user.");
        }

        return new UserBootstrapResponse
        {
            UserId = GetRequiredInt32(reader, "UserId"),
            Name = GetRequiredString(reader, "UserName")
        };
    }

    public async Task<UserIdResponse> GetUserIdByProviderOidAsync(string provider, Guid providerOId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.usp_GetUserIdByProviderOId";
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 30;

        command.Parameters.Add(new SqlParameter("@Provider", SqlDbType.NVarChar, 50){Value = NormalizeOrDbNull(provider)});
        command.Parameters.Add(new SqlParameter("@ProviderOId", SqlDbType.UniqueIdentifier){Value = providerOId});

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new KeyNotFoundException("No user found for the supplied provider oid.");
        }

        return new UserIdResponse
        {
            UserId = GetRequiredInt32(reader, "UserId")
        };
    }

    private static object NormalizeOrDbNull(string? value)
    => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();

    private static int GetRequiredInt32(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
        {
            throw new InvalidOperationException($"Column '{columnName}' was null.");
        }
        return reader.GetInt32(ordinal);
    }

    private static string GetRequiredString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
        {
            throw new InvalidOperationException($"Column '{columnName}' was null.");
        }
        return reader.GetString(ordinal);
    }
}