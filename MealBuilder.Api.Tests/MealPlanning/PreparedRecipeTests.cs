using System.Net;
using System.Net.Http.Json;
using MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;
using MealBuilder.Api.Contracts.Recipes;
using MealBuilder.Api.Tests.Infrastructure;
using MealBuilder.Api.Tests.Recipes;

namespace MealBuilder.Api.Tests.MealPlanning;

public sealed class PreparedRecipeTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task Create_WhenSourceRecipeChanges_PreservesSnapshot()
    {
        using var client = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(client);

        var builtInIngredients =
            await RecipeTestHelper.GetBuiltInIngredientsAsync(client);

        Assert.True(builtInIngredients.Count >= 2);

        var sourceRequest =
            RecipeTestHelper.CreateValidRequest(
                builtInIngredients[0].Id,
                "Original recipe");

        var sourceRecipe =
            await RecipeTestHelper.CreateRecipeAsync(
                client,
                sourceRequest);

        var preparedDate = new DateOnly(2026, 8, 23);

        var preparedRecipe =
            await MealPlanningTestHelper.CreatePreparedRecipeAsync(
                client,
                new CreatePreparedRecipeRequest(
                    RecipeId: sourceRecipe.Id,
                    PreparedDate: preparedDate,
                    TotalPortions: 4m,
                    Allocations: []));

        Assert.True(preparedRecipe.Id > 0);
        Assert.Equal(
            sourceRecipe.Id,
            preparedRecipe.SourceRecipeId);
        Assert.Equal(
            sourceRequest.Name,
            preparedRecipe.Name);
        Assert.Equal(
            preparedDate,
            preparedRecipe.PreparedDate);
        Assert.Equal(4m, preparedRecipe.TotalPortions);
        Assert.Equal(0m, preparedRecipe.AllocatedPortions);
        Assert.Equal(4m, preparedRecipe.AvailablePortions);

        var originalIngredient =
            Assert.Single(preparedRecipe.Ingredients);

        Assert.Equal(
            builtInIngredients[0].Name,
            originalIngredient.Name);
        Assert.Equal(200m, originalIngredient.Grams);
        Assert.Equal(1, originalIngredient.Position);

        var updatedSourceRequest = sourceRequest with
        {
            Name = "Updated source recipe",
            Ingredients =
            [
                new RecipeIngredientRequest(
                    builtInIngredients[1].Id,
                    125m)
            ]
        };

        var updateResponse = await client.PutWithCsrfAsync(
            $"/api/recipes/{sourceRecipe.Id}",
            updatedSourceRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode);

        var detailsResponse = await client.GetAsync(
            $"/api/prepared-recipes/{preparedRecipe.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            detailsResponse.StatusCode);

        var persistedSnapshot = await detailsResponse.Content
            .ReadFromJsonAsync<PreparedRecipeResponse>();

        Assert.NotNull(persistedSnapshot);
        Assert.Equal(
            preparedRecipe.SourceRecipeId,
            persistedSnapshot.SourceRecipeId);
        Assert.Equal(
            preparedRecipe.Name,
            persistedSnapshot.Name);
        Assert.Equal(
            preparedRecipe.TotalNutrition,
            persistedSnapshot.TotalNutrition);
        Assert.Equal(
            preparedRecipe.NutritionPerPortion,
            persistedSnapshot.NutritionPerPortion);
        Assert.Equal(
            originalIngredient,
            Assert.Single(persistedSnapshot.Ingredients));

        var preparedRecipes = await client
            .GetFromJsonAsync<
                IReadOnlyList<PreparedRecipeSummaryResponse>>(
                "/api/prepared-recipes");

        Assert.NotNull(preparedRecipes);

        var summary = Assert.Single(
            preparedRecipes,
            item => item.Id == preparedRecipe.Id);

        Assert.Equal(
            preparedRecipe.Name,
            summary.Name);
        Assert.Equal(
            preparedRecipe.NutritionPerPortion,
            summary.NutritionPerPortion);
    }

    [Fact]
    public async Task DeleteSourceRecipe_WhenPreparedRecipeExists_PreservesSnapshot()
    {
        using var client = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(client);

        var builtInIngredients =
            await RecipeTestHelper.GetBuiltInIngredientsAsync(client);

        Assert.NotEmpty(builtInIngredients);

        var sourceRequest =
            RecipeTestHelper.CreateValidRequest(
                builtInIngredients[0].Id,
                "Recipe to delete");

        var sourceRecipe =
            await RecipeTestHelper.CreateRecipeAsync(
                client,
                sourceRequest);

        var preparedRecipe =
            await MealPlanningTestHelper.CreatePreparedRecipeAsync(
                client,
                new CreatePreparedRecipeRequest(
                    RecipeId: sourceRecipe.Id,
                    PreparedDate: new DateOnly(2026, 8, 23),
                    TotalPortions: 3m,
                    Allocations: []));

        var deleteResponse = await client.DeleteWithCsrfAsync(
            $"/api/recipes/{sourceRecipe.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var detailsResponse = await client.GetAsync(
            $"/api/prepared-recipes/{preparedRecipe.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            detailsResponse.StatusCode);

        var preservedSnapshot = await detailsResponse.Content
            .ReadFromJsonAsync<PreparedRecipeResponse>();

        Assert.NotNull(preservedSnapshot);
        Assert.Null(preservedSnapshot.SourceRecipeId);
        Assert.Equal(
            preparedRecipe.Name,
            preservedSnapshot.Name);
        Assert.Equal(
            preparedRecipe.TotalNutrition,
            preservedSnapshot.TotalNutrition);
        Assert.Equal(
            preparedRecipe.NutritionPerPortion,
            preservedSnapshot.NutritionPerPortion);
        Assert.Equal(
            Assert.Single(preparedRecipe.Ingredients),
            Assert.Single(preservedSnapshot.Ingredients));
    }
}