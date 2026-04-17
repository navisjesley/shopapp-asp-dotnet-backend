using System.Data;
using CartAPI.Contracts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CartAPI.Data;

public sealed class CartRepository(IConfiguration config) : ICartRepository
{
    private readonly string _connectionString = config.GetConnectionString("CartDb")!;

    public async Task AddItemToCartAsync(AddToCartRequest request, int userId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.usp_Cart_AddItem";
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 30;

        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int){Value = userId});
        command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int){Value = request.ProductId});
        command.Parameters.Add(new SqlParameter("@QuantityToAdd", SqlDbType.Int){Value = request.QuantityToAdd});

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CartItemDto>> GetCartByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.usp_Cart_GetActiveCartByUserId";
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 30;

        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int){Value = userId});

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var items = new List<CartItemDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new CartItemDto
            {
                CartItemId = GetRequiredInt32(reader, "CartItemId"),
                CartId = GetRequiredInt32(reader, "CartId"),
                ProductId = GetRequiredInt32(reader, "ProductId"),
                ProductName = GetRequiredString(reader, "ProductName"),
                QuantityInCart = GetRequiredInt32(reader, "QuantityInCart")
            });
        }
        return items;
    }

    public async Task DeleteItemFromCartAsync(DeleteCartItemRequest request, int userId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.usp_Cart_DeleteItem";
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 30;
        
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
        command.Parameters.Add(new SqlParameter("@CartItemId", SqlDbType.Int){Value = request.CartItemId});
        command.Parameters.Add(new SqlParameter("@CartId", SqlDbType.Int){Value = request.CartId});
        command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int){Value = request.ProductId});

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

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