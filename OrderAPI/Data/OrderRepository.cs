using System.Data;
using OrderAPI.Contracts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace OrderAPI.Data;

public sealed class OrderRepository(IConfiguration config) : IOrderRepository
{
    private readonly string _connectionString = config.GetConnectionString("OrderDb")!;

    public async Task<PlaceOrderResponse> PlaceOrderAsync(int userId, PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.usp_Order_PlaceOrder";
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 30;

        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int){Value = userId});
        command.Parameters.Add(new SqlParameter("@CartId", SqlDbType.Int){Value = request.CartId});
        command.Parameters.Add(new SqlParameter("@DeliveryAddress", SqlDbType.NVarChar, 500){Value = request.DeliveryAddress});
        command.Parameters.Add(new SqlParameter("@Items", SqlDbType.Structured)
        {TypeName = "dbo.OrderItemInput", Value = BuildOrderItemsDataTable(request.Items)});

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Stored procedure did not return an Order Id.");
        }

        return new PlaceOrderResponse
        {
            OrderId = GetRequiredInt32(reader, "OrderId")
        };
    }

    private static DataTable BuildOrderItemsDataTable(IReadOnlyList<PlaceOrderItemRequest> items)
    {
        var table = new DataTable();
        table.Columns.Add("CartItemId", typeof(int));
        table.Columns.Add("ProductId", typeof(int));
        table.Columns.Add("QuantityInCart", typeof(int));

        foreach (var item in items)
        {
            table.Rows.Add(item.CartItemId, item.ProductId, item.QuantityInCart);
        }

        return table;
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
}