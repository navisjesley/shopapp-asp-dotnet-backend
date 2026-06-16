using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderAPI.Contracts;
using OrderAPI.Controllers;
using OrderAPI.Data;
using OrderAPI.Services;

namespace OrderAPI.Tests.Controllers;

// PR validation trigger test 2
public class OrdersControllerTests
{
    [Fact]
    public async Task PlaceOrder_ReturnsBadRequest_WhenRequestInvalid()
    {
        var repo = new Mock<IOrderRepository>();
        var userApi = new Mock<IUserApiClient>();

        var controller =
            new OrdersController(repo.Object, userApi.Object);

        var result = await controller.PlaceOrder(
            new PlaceOrderRequest
            {
                CartId = 0,
                DeliveryAddress = "",
                Items = []
            },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PlaceOrder_ReturnsBadRequest_WhenOidMissing()
    {
        var repo = new Mock<IOrderRepository>();
        var userApi = new Mock<IUserApiClient>();

        var controller =
            new OrdersController(repo.Object, userApi.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await controller.PlaceOrder(
            new PlaceOrderRequest
            {
                CartId = 1,
                DeliveryAddress = "123 Test Street",
                Items =
                [
                    new PlaceOrderItemRequest
                    {
                        CartItemId = 1,
                        ProductId = 100,
                        QuantityInCart = 2
                    }
                ]
            },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PlaceOrder_ReturnsOk_WhenRequestValid()
    {
        var repo = new Mock<IOrderRepository>();
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
                x.PlaceOrderAsync(
                    123,
                    It.IsAny<PlaceOrderRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaceOrderResponse
            {
                OrderId = 500
            });

        var controller =
            new OrdersController(repo.Object, userApi.Object);

        var httpContext = new DefaultHttpContext();

        httpContext.Request.Headers["X-User-Provider"] = "AAD";
        httpContext.Request.Headers["X-User-Oid"] =
            Guid.NewGuid().ToString();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.PlaceOrder(
            new PlaceOrderRequest
            {
                CartId = 1,
                DeliveryAddress = "123 Test Street",
                Items =
                [
                    new PlaceOrderItemRequest
                    {
                        CartItemId = 1,
                        ProductId = 100,
                        QuantityInCart = 2
                    }
                ]
            },
            CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        var response =
            Assert.IsType<PlaceOrderResponse>(okResult.Value);

        Assert.Equal(500, response.OrderId);
    }

    [Fact]
    public async Task PlaceOrder_CallsRepository_WithResolvedUserId()
    {
        var repo = new Mock<IOrderRepository>();
        var userApi = new Mock<IUserApiClient>();

        userApi.Setup(x =>
                x.GetUserIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdResponse
            {
                UserId = 777
            });

        repo.Setup(x =>
                x.PlaceOrderAsync(
                    It.IsAny<int>(),
                    It.IsAny<PlaceOrderRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaceOrderResponse
            {
                OrderId = 1
            });

        var controller =
            new OrdersController(repo.Object, userApi.Object);

        var httpContext = new DefaultHttpContext();

        httpContext.Request.Headers["X-User-Provider"] = "AAD";
        httpContext.Request.Headers["X-User-Oid"] =
            Guid.NewGuid().ToString();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        await controller.PlaceOrder(
            new PlaceOrderRequest
            {
                CartId = 1,
                DeliveryAddress = "Address",
                Items =
                [
                    new PlaceOrderItemRequest
                    {
                        CartItemId = 1,
                        ProductId = 1,
                        QuantityInCart = 1
                    }
                ]
            },
            CancellationToken.None);

        repo.Verify(r =>
                r.PlaceOrderAsync(
                    777,
                    It.IsAny<PlaceOrderRequest>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
}