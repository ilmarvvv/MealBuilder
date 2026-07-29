using System.Net;
using System.Net.Http.Json;
using MealBuilder.Api.Contracts.Authentication;
using MealBuilder.Api.Tests.Infrastructure;

namespace MealBuilder.Api.Tests.Authentication;

public sealed class AuthenticationTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task Register_WithValidCredentials_AuthenticatesUser()
    {
        using var client = factory.CreateHttpsClient();

        var email = $"user-{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest(
            email,
            "Test123!");

        var registerResponse = await client.PostWithCsrfAsync(
            "/api/auth/register",
            request);

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        var registeredUser = await registerResponse.Content
            .ReadFromJsonAsync<AuthUserResponse>();

        Assert.NotNull(registeredUser);
        Assert.Equal(email, registeredUser.Email);

        var currentUserResponse = await client.GetAsync(
            "/api/auth/me");

        Assert.Equal(
            HttpStatusCode.OK,
            currentUserResponse.StatusCode);

        var currentUser = await currentUserResponse.Content
            .ReadFromJsonAsync<AuthUserResponse>();

        Assert.NotNull(currentUser);
        Assert.Equal(registeredUser.Id, currentUser.Id);
        Assert.Equal(email, currentUser.Email);
    }

    [Fact]
    public async Task Login_WithValidCredentials_AuthenticatesUser()
    {
        var email = $"user-{Guid.NewGuid():N}@example.com";
        const string password = "Test123!";

        using (var registrationClient = factory.CreateHttpsClient())
        {
            var registerRequest = new RegisterRequest(
                email,
                password);

            var registerResponse =
                await registrationClient.PostWithCsrfAsync(
                    "/api/auth/register",
                    registerRequest);

            registerResponse.EnsureSuccessStatusCode();
        }

        using var client = factory.CreateHttpsClient();

        var loginRequest = new LoginRequest(
            email,
            password);

        var loginResponse = await client.PostWithCsrfAsync(
            "/api/auth/login",
            loginRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var currentUserResponse = await client.GetAsync(
            "/api/auth/me");

        Assert.Equal(
            HttpStatusCode.OK,
            currentUserResponse.StatusCode);

        var currentUser = await currentUserResponse.Content
            .ReadFromJsonAsync<AuthUserResponse>();

        Assert.NotNull(currentUser);
        Assert.Equal(email, currentUser.Email);
    }

    [Fact]
    public async Task Logout_WhenAuthenticated_EndsSession()
    {
        using var client = factory.CreateHttpsClient();

        var email = $"user-{Guid.NewGuid():N}@example.com";
        var registerRequest = new RegisterRequest(
            email,
            "Test123!");

        var registerResponse = await client.PostWithCsrfAsync(
            "/api/auth/register",
            registerRequest);

        registerResponse.EnsureSuccessStatusCode();

        var logoutResponse = await client.PostWithCsrfAsync(
            "/api/auth/logout");

        Assert.Equal(
            HttpStatusCode.NoContent,
            logoutResponse.StatusCode);

        var currentUserResponse = await client.GetAsync(
            "/api/auth/me");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            currentUserResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var email = $"user-{Guid.NewGuid():N}@example.com";

        using (var registrationClient = factory.CreateHttpsClient())
        {
            var registerRequest = new RegisterRequest(
                email,
                "Test123!");

            var registerResponse =
                await registrationClient.PostWithCsrfAsync(
                    "/api/auth/register",
                    registerRequest);

            registerResponse.EnsureSuccessStatusCode();
        }

        using var client = factory.CreateHttpsClient();

        var loginRequest = new LoginRequest(
            email,
            "Wrong123!");

        var loginResponse = await client.PostWithCsrfAsync(
            "/api/auth/login",
            loginRequest);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            loginResponse.StatusCode);
    }
}