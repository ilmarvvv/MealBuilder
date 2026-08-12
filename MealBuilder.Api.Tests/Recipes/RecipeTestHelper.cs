using System.Net.Http.Json;
using MealBuilder.Api.Contracts.Authentication;
using MealBuilder.Api.Contracts.Ingredients;
using MealBuilder.Api.Contracts.Recipes;
using MealBuilder.Api.Tests.Infrastructure;

namespace MealBuilder.Api.Tests.Recipes;

internal static class RecipeTestHelper
{
    public static async Task RegisterUserAsync(
        HttpClient client)
    {
        var request = new RegisterRequest(
            $"recipe-user-{Guid.NewGuid():N}@example.com",
            "Test123!");

        var response = await client.PostWithCsrfAsync(
            "/api/auth/register",
            request);

        response.EnsureSuccessStatusCode();
    }

    public static async Task<IReadOnlyList<IngredientResponse>>
        GetBuiltInIngredientsAsync(HttpClient client)
    {
        var ingredients = await client
            .GetFromJsonAsync<IReadOnlyList<IngredientResponse>>(
                "/api/ingredients");

        if (ingredients is null)
        {
            throw new InvalidOperationException(
                "The ingredient response was empty.");
        }

        return ingredients
            .Where(ingredient => ingredient.IsBuiltIn)
            .ToArray();
    }

    public static RecipeRequest CreateValidRequest(
        int ingredientId,
        string name = "Test recipe")
    {
        return new RecipeRequest(
            Name: name,
            Description: "Recipe created by an integration test.",
            Servings: 2,
            Ingredients:
            [
                new RecipeIngredientRequest(
                    IngredientId: ingredientId,
                    Grams: 200m)
            ],
            Steps:
            [
                new RecipeStepRequest(
                    "Prepare the ingredient."),
                new RecipeStepRequest(
                    "Cook and serve.")
            ]);
    }

    public static async Task<RecipeResponse> CreateRecipeAsync(
        HttpClient client,
        RecipeRequest request)
    {
        var response = await client.PostWithCsrfAsync(
            "/api/recipes",
            request);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<RecipeResponse>()
            ?? throw new InvalidOperationException(
                "The recipe response was empty.");
    }
}