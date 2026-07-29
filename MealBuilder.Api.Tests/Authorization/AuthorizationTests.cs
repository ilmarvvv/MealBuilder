using System.Net;
using MealBuilder.Api.Tests.Infrastructure;

namespace MealBuilder.Api.Tests.Authorization;

public sealed class AuthorizationTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task GetCurrentUser_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = factory.CreateHttpsClient();

        var response = await client.GetAsync(
            "/api/auth/me");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Logout_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = factory.CreateHttpsClient();

        var response = await client.PostWithCsrfAsync(
            "/api/auth/logout");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}