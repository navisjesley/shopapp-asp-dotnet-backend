using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductAPI.Contracts;
using ProductAPI.Controllers;
using ProductAPI.Data;

namespace ProductAPI.Tests.Controllers;

// PR validation trigger test 1
public class ProductsControllerTests
{
    [Fact]
    public async Task GetCategories_ReturnsOk_WithCategories()
    {
        var repo = new Mock<IProductCatalogRepository>();

        repo.Setup(r => r.GetCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new CategoryDto
                {
                    ProductCategoryId = 1,
                    ProductCategory = "Electronics"
                }
            ]);

        var controller = new ProductsController(repo.Object);

        var result = await controller.GetCategories(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var categories =
            Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(okResult.Value);

        Assert.Single(categories);
    }

    [Fact]
    public async Task GetProductsByCategory_ReturnsValidationProblem_WhenCategoryIdInvalid()
    {
        var repo = new Mock<IProductCatalogRepository>();

        var controller = new ProductsController(repo.Object);

        var result =
            await controller.GetProductsByCategory(0, CancellationToken.None);

        Assert.IsType<ObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetProductsByCategory_ReturnsOk_WhenCategoryIdValid()
    {
        var repo = new Mock<IProductCatalogRepository>();

        repo.Setup(r =>
                r.GetProductsByCategoryAsync(
                    1,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ProductListItemDto
                {
                    ProductId = 10,
                    ProductName = "Laptop"
                }
            ]);

        var controller = new ProductsController(repo.Object);

        var result =
            await controller.GetProductsByCategory(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var products =
            Assert.IsAssignableFrom<IEnumerable<ProductListItemDto>>(okResult.Value);

        Assert.Single(products);
    }

    [Fact]
    public async Task GetProductById_ReturnsValidationProblem_WhenIdInvalid()
    {
        var repo = new Mock<IProductCatalogRepository>();

        var controller = new ProductsController(repo.Object);

        var result =
            await controller.GetProductById(0, CancellationToken.None);

        Assert.IsType<ObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetProductById_ReturnsNotFound_WhenRepositoryReturnsNull()
    {
        var repo = new Mock<IProductCatalogRepository>();

        repo.Setup(r =>
                r.GetProductByIdAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductDetailDto?)null);

        var controller = new ProductsController(repo.Object);

        var result =
            await controller.GetProductById(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetProductById_ReturnsOk_WhenProductExists()
    {
        var repo = new Mock<IProductCatalogRepository>();

        repo.Setup(r =>
                r.GetProductByIdAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductDetailDto
            {
                ProductId = 1,
                ProductName = "Laptop",
                Quantity = 5,
                ProductImageUrl = "/images/laptop.png"
            });

        var controller = new ProductsController(repo.Object);

        var result =
            await controller.GetProductById(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var product =
            Assert.IsType<ProductDetailDto>(okResult.Value);

        Assert.Equal(1, product.ProductId);
    }
}