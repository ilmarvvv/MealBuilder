using System.Net.Http.Json;
using MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;
using MealBuilder.Api.Tests.Infrastructure;

namespace MealBuilder.Api.Tests.MealPlanning;

internal static class MealPlanningTestHelper
{
    public static async Task<PreparedRecipeResponse>
        CreatePreparedRecipeAsync(
            HttpClient client,
            CreatePreparedRecipeRequest request)
    {
        var response = await client.PostWithCsrfAsync(
            "/api/prepared-recipes",
            request);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<PreparedRecipeResponse>()
            ?? throw new InvalidOperationException(
                "The prepared recipe response was empty.");
    }
}