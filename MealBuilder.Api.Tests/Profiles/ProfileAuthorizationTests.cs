using System.Net;
using MealBuilder.Api.Contracts.Authentication;
using MealBuilder.Api.Contracts.Profiles;
using MealBuilder.Api.Tests.Infrastructure;

namespace MealBuilder.Api.Tests.Profiles;

public sealed class ProfileAuthorizationTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task GetCurrent_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = factory.CreateHttpsClient();

        var response = await client.GetAsync(
            "/api/profile");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetCurrent_WhenAnotherUserOwnsProfile_ReturnsNotFound()
    {
        using var ownerClient = factory.CreateHttpsClient();
        using var otherUserClient = factory.CreateHttpsClient();

        await RegisterUserAsync(ownerClient);
        await RegisterUserAsync(otherUserClient);

        var saveResponse = await ownerClient.PutWithCsrfAsync(
            "/api/profile/calorie-target",
            new DailyCalorieTargetRequest(2300));

        saveResponse.EnsureSuccessStatusCode();

        var ownerResponse = await ownerClient.GetAsync(
            "/api/profile");

        Assert.Equal(
            HttpStatusCode.OK,
            ownerResponse.StatusCode);

        var otherUserResponse = await otherUserClient.GetAsync(
            "/api/profile");

        Assert.Equal(
            HttpStatusCode.NotFound,
            otherUserResponse.StatusCode);
    }

    private static async Task RegisterUserAsync(
        HttpClient client)
    {
        var request = new RegisterRequest(
            $"profile-authorization-{Guid.NewGuid():N}@example.com",
            "Test123!");

        var response = await client.PostWithCsrfAsync(
            "/api/auth/register",
            request);

        response.EnsureSuccessStatusCode();
    }
}