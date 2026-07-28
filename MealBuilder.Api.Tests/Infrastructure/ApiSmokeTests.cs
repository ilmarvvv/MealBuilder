namespace MealBuilder.Api.Tests.Infrastructure;

public sealed class ApiSmokeTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task CsrfTokenEndpoint_ReturnsSuccessStatusCode()
    {
        using var client = factory.CreateHttpsClient();

        var response = await client.GetAsync(
            "/api/security/csrf-token");

        response.EnsureSuccessStatusCode();
    }
}