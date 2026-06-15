using CartAPI.Contracts;
using CartAPI.Controllers;
using CartAPI.Data;
using CartAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CartAPI.Tests.Controllers;

public class CartControllerTests
{
    [Fact]
    public async Task AddItemToCart_ReturnsBadRequest_WhenQuantityInvalid()
    {
        var repo = new Mock<ICartRepository>();
        var userApi = new Mock<IUserApiClient>();

        var controller =
            new CartController(repo.Object, userApi.Object);

        var result = await controller.AddItemToCart(
            new AddToCartRequest
            {
                ProductId = 1,
                QuantityToAdd = 0
            },
            CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);

        Assert.IsType<ObjectResult>(result);
    }

    [Fact]
    public async Task AddItemToCart_ReturnsBadRequest_WhenOidHeaderMissing()
    {
        var repo = new Mock<ICartRepository>();
        var userApi = new Mock<IUserApiClient>();

        var controller =
            new CartController(repo.Object, userApi.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await controller.AddItemToCart(
            new AddToCartRequest
            {
                ProductId = 1,
                QuantityToAdd = 1
            },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddItemToCart_ReturnsNoContent_WhenRequestValid()
    {
        var repo = new Mock<ICartRepository>();
        var userApi = new Mock<IUserApiClient>();

        userApi.Setup(x =>
                x.GetUserIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdResponse
            {
                UserId = 123
            });

        var controller =
            new CartController(repo.Object, userApi.Object);

        var httpContext = new DefaultHttpContext();

        httpContext.Request.Headers["X-User-Provider"] = "AAD";
        httpContext.Request.Headers["X-User-Oid"] =
            Guid.NewGuid().ToString();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.AddItemToCart(
            new AddToCartRequest
            {
                ProductId = 1,
                QuantityToAdd = 2
            },
            CancellationToken.None);

        Assert.IsType<NoContentResult>(result);

        repo.Verify(r =>
            r.AddItemToCartAsync(
                It.IsAny<AddToCartRequest>(),
                123,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCart_ReturnsBadRequest_WhenOidMissing()
    {
        var repo = new Mock<ICartRepository>();
        var userApi = new Mock<IUserApiClient>();

        var controller =
            new CartController(repo.Object, userApi.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result =
            await controller.GetCart(CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetCart_ReturnsOk_WithCartItems()
    {
        var repo = new Mock<ICartRepository>();
        var userApi = new Mock<IUserApiClient>();

        userApi.Setup(x =>
                x.GetUserIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdResponse
            {
                UserId = 123
            });

        repo.Setup(x =>
                x.GetCartByUserIdAsync(
                    123,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new CartItemDto
                {
                    CartItemId = 1,
                    CartId = 10,
                    ProductId = 99,
                    ProductName = "Laptop",
                    QuantityInCart = 2
                }
            ]);

        var controller =
            new CartController(repo.Object, userApi.Object);

        var httpContext = new DefaultHttpContext();

        httpContext.Request.Headers["X-User-Provider"] = "AAD";
        httpContext.Request.Headers["X-User-Oid"] =
            Guid.NewGuid().ToString();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result =
            await controller.GetCart(CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        var items =
            Assert.IsAssignableFrom<IReadOnlyList<CartItemDto>>(
                okResult.Value);

        Assert.Single(items);
    }

    [Fact]
    public async Task DeleteItemFromCart_ReturnsBadRequest_WhenOidMissing()
    {
        var repo = new Mock<ICartRepository>();
        var userApi = new Mock<IUserApiClient>();

        var controller =
            new CartController(repo.Object, userApi.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await controller.DeleteItemFromCart(
            new DeleteCartItemRequest(),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteItemFromCart_ReturnsNoContent_WhenRequestValid()
    {
        var repo = new Mock<ICartRepository>();
        var userApi = new Mock<IUserApiClient>();

        userApi.Setup(x =>
                x.GetUserIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdResponse
            {
                UserId = 123
            });

        var controller =
            new CartController(repo.Object, userApi.Object);

        var httpContext = new DefaultHttpContext();

        httpContext.Request.Headers["X-User-Provider"] = "AAD";
        httpContext.Request.Headers["X-User-Oid"] =
            Guid.NewGuid().ToString();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.DeleteItemFromCart(
            new DeleteCartItemRequest
            {
                CartItemId = 1,
                CartId = 10,
                ProductId = 99
            },
            CancellationToken.None);

        Assert.IsType<NoContentResult>(result);

        repo.Verify(r =>
            r.DeleteItemFromCartAsync(
                It.IsAny<DeleteCartItemRequest>(),
                123,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}