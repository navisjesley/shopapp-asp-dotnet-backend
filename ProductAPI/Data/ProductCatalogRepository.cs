using System.Data;
using ProductAPI.Contracts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ProductAPI.Data;

public sealed class ProductCatalogRepository(IConfiguration config) : IProductCatalogRepository
{
    private readonly string _connectionString = config.GetConnectionString("ProductDb")!;
    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.usp_ProductCategories_GetAll";
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 30;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var results = new List<CategoryDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new CategoryDto
            {
                ProductCategoryId = GetRequiredInt32(reader, "ProductCategoryId"),
                ProductCategory = GetRequiredString(reader, "ProductCategory")
            });
        }
        return results;
    }

    public async Task<IReadOnlyList<ProductListItemDto>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.usp_Products_GetByCategoryId";
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 30;

        command.Parameters.Add(new SqlParameter("@ProductCategoryId", SqlDbType.Int) { Value = categoryId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var results = new List<ProductListItemDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ProductListItemDto
            {
                ProductId = GetRequiredInt32(reader, "ProductId"),
                ProductName = GetRequiredString(reader, "ProductName"),
            });
        }
        return results;
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.usp_Products_GetDetailsByProductId";
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 30;

        command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("The stored procedure did not return the product data.");
        }

        return new ProductDetailDto
        {
            ProductId = GetRequiredInt32(reader, "ProductId"),
            ProductName = GetRequiredString(reader, "ProductName"),
            Quantity = GetRequiredInt32(reader, "Quantity"),
            ProductImageUrl = GetRequiredString(reader, "ProductImageUrl")
        };
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

    private static decimal GetRequiredDecimal(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
        {
            throw new InvalidOperationException($"Column '{columnName}' was null.");
        }
        return reader.GetDecimal(ordinal);
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