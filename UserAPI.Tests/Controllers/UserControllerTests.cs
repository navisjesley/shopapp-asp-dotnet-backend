using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserAPI.Contracts;
using UserAPI.Controllers;
using UserAPI.Data;

namespace UserAPI.Tests.Controllers;

// PR validation trigger test
public class UserControllerTests
{
    [Fact]
    public async Task Bootstrap_ReturnsBadRequest_WhenHeadersMissing()
    {
        var repo = new Mock<IUserBootstrapRepository>();

        var controller = new UserController(repo.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await controller.Bootstrap(
            new UserBootstrapRequest
            {
                Name = "John",
                Email = "john@test.com"
            },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Bootstrap_ReturnsOk_WhenHeadersValid()
    {
        var repo = new Mock<IUserBootstrapRepository>();

        repo.Setup(r => r.UpsertFederatedUserOnLoginAsync(
                It.IsAny<UserBootstrapRequest>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserBootstrapResponse
            {
                UserId = 1,
                Name = "John"
            });

        var controller = new UserController(repo.Object);

        var httpContext = new DefaultHttpContext();

        httpContext.Request.Headers["X-User-Provider"] = "AAD";
        httpContext.Request.Headers["X-User-Sub"] = "sub123";
        httpContext.Request.Headers["X-User-Oid"] = Guid.NewGuid().ToString();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.Bootstrap(
            new UserBootstrapRequest(),
            CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetUserIdByProviderOid_ReturnsBadRequest_WhenRequestInvalid()
    {
        var repo = new Mock<IUserBootstrapRepository>();

        var controller = new UserController(repo.Object);

        var result = await controller.GetUserIdByProviderOid(
            new UserIdLookupRequest
            {
                Provider = "",
                ProviderOId = Guid.Empty
            },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetUserIdByProviderOid_ReturnsNotFound_WhenRepositoryThrows()
    {
        var repo = new Mock<IUserBootstrapRepository>();

        repo.Setup(r => r.GetUserIdByProviderOidAsync(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        var controller = new UserController(repo.Object);

        var result = await controller.GetUserIdByProviderOid(
            new UserIdLookupRequest
            {
                Provider = "AAD",
                ProviderOId = Guid.NewGuid()
            },
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}