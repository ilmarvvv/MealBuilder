using System.Net.Http.Json;
using MealBuilder.Api.Contracts.Security;

namespace MealBuilder.Api.Tests.Infrastructure;

public static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> PostWithCsrfAsync<TRequest>(
        this HttpClient client,
        string requestUri,
        TRequest requestBody)
    {
        return SendPostWithCsrfAsync(
            client,
            requestUri,
            JsonContent.Create(requestBody));
    }

    public static Task<HttpResponseMessage> PostWithCsrfAsync(
        this HttpClient client,
        string requestUri)
    {
        return SendPostWithCsrfAsync(
            client,
            requestUri,
            content: null);
    }

    private static async Task<HttpResponseMessage> SendPostWithCsrfAsync(
        HttpClient client,
        string requestUri,
        HttpContent? content)
    {
        var tokenResponse = await client
            .GetFromJsonAsync<AntiforgeryTokenResponse>(
                "/api/security/csrf-token");

        if (tokenResponse is null)
        {
            throw new InvalidOperationException(
                "The CSRF token response was empty.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            requestUri)
        {
            Content = content
        };

        request.Headers.Add(
            "X-CSRF-TOKEN",
            tokenResponse.Token);

        return await client.SendAsync(request);
    }
}