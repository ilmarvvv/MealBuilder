using System.Net;
using System.Net.Http.Json;
using MealBuilder.Api.Contracts.Ingredients;
using MealBuilder.Api.Contracts.Recipes;
using MealBuilder.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace MealBuilder.Api.Tests.Recipes;

public sealed class RecipeValidationTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task Create_WithoutCookingSteps_ReturnsValidationError()
    {
        using var client = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(client);

        var ingredients =
            await RecipeTestHelper.GetBuiltInIngredientsAsync(client);

        Assert.NotEmpty(ingredients);

        var request = RecipeTestHelper.CreateValidRequest(
            ingredients[0].Id) with
        {
            Steps = []
        };

        var response = await client.PostWithCsrfAsync(
            "/api/recipes",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Contains(
            nameof(RecipeRequest.Steps),
            problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_WithDuplicateIngredients_ReturnsValidationError()
    {
        using var client = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(client);

        var ingredients =
            await RecipeTestHelper.GetBuiltInIngredientsAsync(client);

        Assert.NotEmpty(ingredients);

        var ingredientId = ingredients[0].Id;

        var request = RecipeTestHelper.CreateValidRequest(
            ingredientId) with
        {
            Ingredients =
            [
                new RecipeIngredientRequest(
                    ingredientId,
                    100m),
                new RecipeIngredientRequest(
                    ingredientId,
                    200m)
            ]
        };

        var response = await client.PostWithCsrfAsync(
            "/api/recipes",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Contains(
            nameof(RecipeRequest.Ingredients),
            problem.Errors.Keys);
    }

    [Fact]
    public async Task DeleteIngredient_WhenUsedByRecipe_ReturnsConflict()
    {
        using var client = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(client);

        var ingredientRequest = new IngredientRequest(
            Name: "Used personal ingredient",
            CaloriesPer100g: 100m,
            ProteinPer100g: 10m,
            FatPer100g: 5m,
            CarbohydratesPer100g: 12m,
            SugarsPer100g: 3m,
            FiberPer100g: 2m,
            SaltPer100g: 0.5m);

        var createIngredientResponse =
            await client.PostWithCsrfAsync(
                "/api/ingredients",
                ingredientRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            createIngredientResponse.StatusCode);

        var ingredient = await createIngredientResponse.Content
            .ReadFromJsonAsync<IngredientResponse>();

        Assert.NotNull(ingredient);

        await RecipeTestHelper.CreateRecipeAsync(
            client,
            RecipeTestHelper.CreateValidRequest(
                ingredient.Id));

        var deleteResponse = await client.DeleteWithCsrfAsync(
            $"/api/ingredients/{ingredient.Id}");

        Assert.Equal(
            HttpStatusCode.Conflict,
            deleteResponse.StatusCode);

        var problem = await deleteResponse.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(
            "Ingredient is in use.",
            problem.Title);

        var ingredientResponse = await client.GetAsync(
            $"/api/ingredients/{ingredient.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            ingredientResponse.StatusCode);
    }
}