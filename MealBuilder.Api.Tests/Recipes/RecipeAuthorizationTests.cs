using MealBuilder.Api.Contracts.Ingredients;
using MealBuilder.Api.Contracts.Recipes;
using MealBuilder.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace MealBuilder.Api.Tests.Recipes;

public sealed class RecipeAuthorizationTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task Endpoints_WhenUnauthenticated_ReturnUnauthorized()
    {
        using var client = factory.CreateHttpsClient();

        var request =
            RecipeTestHelper.CreateValidRequest(
                ingredientId: 1);

        var listResponse = await client.GetAsync(
            "/api/recipes");

        var detailsResponse = await client.GetAsync(
            "/api/recipes/1");

        var createResponse = await client.PostWithCsrfAsync(
            "/api/recipes",
            request);

        var updateResponse = await client.PutWithCsrfAsync(
            "/api/recipes/1",
            request);

        var deleteResponse = await client.DeleteWithCsrfAsync(
            "/api/recipes/1");

        var addIngredientResponse =
            await client.PostWithCsrfAsync(
                "/api/recipes/1/ingredients",
                new RecipeIngredientRequest(1, 100m));

        var addStepResponse = await client.PostWithCsrfAsync(
            "/api/recipes/1/steps",
            new RecipeStepRequest("Anonymous step."));

        Assert.All(
            new[]
            {
                listResponse,
                detailsResponse,
                createResponse,
                updateResponse,
                deleteResponse,
                addIngredientResponse,
                addStepResponse
            },
            response => Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode));
    }

    [Fact]
    public async Task Endpoints_WhenRecipeBelongsToAnotherUser_ReturnNotFound()
    {
        using var ownerClient = factory.CreateHttpsClient();
        using var otherUserClient = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(ownerClient);
        await RecipeTestHelper.RegisterUserAsync(otherUserClient);

        var ingredients =
            await RecipeTestHelper.GetBuiltInIngredientsAsync(
                ownerClient);

        Assert.NotEmpty(ingredients);

        var request = RecipeTestHelper.CreateValidRequest(
            ingredients[0].Id,
            "Private recipe");

        var recipe = await RecipeTestHelper.CreateRecipeAsync(
            ownerClient,
            request);

        var listResponse = await otherUserClient.GetAsync(
            "/api/recipes");

        Assert.Equal(
            HttpStatusCode.OK,
            listResponse.StatusCode);

        var visibleRecipes = await listResponse.Content
            .ReadFromJsonAsync<
                IReadOnlyList<RecipeSummaryResponse>>();

        Assert.NotNull(visibleRecipes);
        Assert.DoesNotContain(
            visibleRecipes,
            visibleRecipe =>
                visibleRecipe.Id == recipe.Id);

        var detailsResponse = await otherUserClient.GetAsync(
            $"/api/recipes/{recipe.Id}");

        var updateResponse =
            await otherUserClient.PutWithCsrfAsync(
                $"/api/recipes/{recipe.Id}",
                request);

        var deleteResponse =
            await otherUserClient.DeleteWithCsrfAsync(
                $"/api/recipes/{recipe.Id}");

        var addIngredientResponse =
            await otherUserClient.PostWithCsrfAsync(
                $"/api/recipes/{recipe.Id}/ingredients",
                new RecipeIngredientRequest(
                    ingredients[0].Id,
                    100m));

        var addStepResponse =
            await otherUserClient.PostWithCsrfAsync(
                $"/api/recipes/{recipe.Id}/steps",
                new RecipeStepRequest(
                    "Unauthorized step."));

        Assert.All(
            new[]
            {
                detailsResponse,
                updateResponse,
                deleteResponse,
                addIngredientResponse,
                addStepResponse
            },
            response => Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode));

        var ownerDetailsResponse = await ownerClient.GetAsync(
            $"/api/recipes/{recipe.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            ownerDetailsResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WithAnotherUsersIngredient_ReturnsValidationError()
    {
        using var ownerClient = factory.CreateHttpsClient();
        using var otherUserClient = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(ownerClient);
        await RecipeTestHelper.RegisterUserAsync(otherUserClient);

        var ingredientRequest = new IngredientRequest(
            Name: "Private recipe ingredient",
            CaloriesPer100g: 100m,
            ProteinPer100g: 10m,
            FatPer100g: 5m,
            CarbohydratesPer100g: 12m,
            SugarsPer100g: 3m,
            FiberPer100g: 2m,
            SaltPer100g: 0.5m);

        var createIngredientResponse =
            await ownerClient.PostWithCsrfAsync(
                "/api/ingredients",
                ingredientRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            createIngredientResponse.StatusCode);

        var ingredient = await createIngredientResponse.Content
            .ReadFromJsonAsync<IngredientResponse>();

        Assert.NotNull(ingredient);

        var recipeRequest =
            RecipeTestHelper.CreateValidRequest(
                ingredient.Id,
                "Recipe with inaccessible ingredient");

        var createRecipeResponse =
            await otherUserClient.PostWithCsrfAsync(
                "/api/recipes",
                recipeRequest);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            createRecipeResponse.StatusCode);

        var problem = await createRecipeResponse.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Contains(
            nameof(RecipeRequest.Ingredients),
            problem.Errors.Keys);

        var recipes = await otherUserClient
            .GetFromJsonAsync<
                IReadOnlyList<RecipeSummaryResponse>>(
                    "/api/recipes");

        Assert.NotNull(recipes);
        Assert.Empty(recipes);
    }
}