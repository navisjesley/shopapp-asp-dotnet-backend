using ProductAPI.Data;
using ProductAPI.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductAPI.Controllers;

[ApiController]
[Route("products")]
[Authorize(Policy = "ProductApiCaller")]
public sealed class ProductsController(IProductCatalogRepository repository) : ControllerBase
{
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await repository.GetCategoriesAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductListItemDto>>> GetProductsByCategory([FromQuery] int categoryId,
    CancellationToken cancellationToken)
    {
        if (categoryId <= 0)
        {
            ModelState.AddModelError(nameof(categoryId), "categoryId must be greater than zero.");
            return ValidationProblem(ModelState);
        }

        var products = await repository.GetProductsByCategoryAsync(categoryId, cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDetailDto>> GetProductById(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            ModelState.AddModelError(nameof(id), "id must be greater than zero.");
            return ValidationProblem(ModelState);
        }

        var product = await repository.GetProductByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }
}