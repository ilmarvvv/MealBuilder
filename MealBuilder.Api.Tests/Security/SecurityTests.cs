using System.Net;
using System.Net.Http.Json;
using MealBuilder.Api.Contracts.Authentication;
using MealBuilder.Api.Tests.Infrastructure;

namespace MealBuilder.Api.Tests.Security;

public sealed class SecurityTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task Register_WithoutCsrfToken_ReturnsBadRequest()
    {
        using var client = factory.CreateHttpsClient();

        var request = new RegisterRequest(
            $"user-{Guid.NewGuid():N}@example.com",
            "Test123!");

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}