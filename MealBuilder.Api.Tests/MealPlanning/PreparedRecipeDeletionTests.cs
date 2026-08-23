using System.Net;
using System.Net.Http.Json;
using MealBuilder.Api.Contracts.MealPlanning.DailyPlans;
using MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;
using MealBuilder.Api.Tests.Infrastructure;
using MealBuilder.Api.Tests.Recipes;

namespace MealBuilder.Api.Tests.MealPlanning;

public sealed class PreparedRecipeDeletionTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task Delete_WhenUsedByDailyPlans_RemovesItemsAndCleansEmptyPlan()
    {
        using var client = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(client);

        var builtInIngredients =
            await RecipeTestHelper.GetBuiltInIngredientsAsync(client);

        Assert.NotEmpty(builtInIngredients);

        var sourceRecipe =
            await RecipeTestHelper.CreateRecipeAsync(
                client,
                RecipeTestHelper.CreateValidRequest(
                    builtInIngredients[0].Id,
                    "Prepared recipe to delete"));

        var firstDate = new DateOnly(2026, 8, 24);
        var secondDate = firstDate.AddDays(1);

        var preparedRecipe =
            await MealPlanningTestHelper.CreatePreparedRecipeAsync(
                client,
                new CreatePreparedRecipeRequest(
                    RecipeId: sourceRecipe.Id,
                    PreparedDate: firstDate,
                    TotalPortions: 3m,
                    Allocations:
                    [
                        new PreparedRecipeAllocationRequest(
                            firstDate,
                            1m),
                        new PreparedRecipeAllocationRequest(
                            secondDate,
                            1m)
                    ]));

        var addIngredientResponse =
            await client.PostWithCsrfAsync(
                $"/api/daily-plans/{secondDate:yyyy-MM-dd}/ingredients",
                new AddDailyPlanIngredientRequest(
                    IngredientId: builtInIngredients[0].Id,
                    Grams: 150m,
                    PlannedTime: null));

        Assert.Equal(
            HttpStatusCode.OK,
            addIngredientResponse.StatusCode);

        var mixedDailyPlan = await addIngredientResponse.Content
            .ReadFromJsonAsync<DailyPlanResponse>();

        Assert.NotNull(mixedDailyPlan);
        Assert.Equal(2, mixedDailyPlan.Items.Count);

        var ingredientItem = Assert.Single(
            mixedDailyPlan.Items,
            item => item.IngredientId.HasValue);

        var deletionImpact = await client
            .GetFromJsonAsync<
                PreparedRecipeDeletionImpactResponse>(
                $"/api/prepared-recipes/{preparedRecipe.Id}/deletion-impact");

        Assert.NotNull(deletionImpact);
        Assert.Equal(
            preparedRecipe.Id,
            deletionImpact.PreparedRecipeId);
        Assert.Equal(2, deletionImpact.AffectedItemCount);
        Assert.Equal(2, deletionImpact.AffectedDateCount);

        var deleteResponse = await client.DeleteWithCsrfAsync(
            $"/api/prepared-recipes/{preparedRecipe.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var emptyDailyPlan = await client
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{firstDate:yyyy-MM-dd}");

        Assert.NotNull(emptyDailyPlan);
        Assert.Null(emptyDailyPlan.Id);
        Assert.Empty(emptyDailyPlan.Items);

        var remainingDailyPlan = await client
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{secondDate:yyyy-MM-dd}");

        Assert.NotNull(remainingDailyPlan);
        Assert.NotNull(remainingDailyPlan.Id);

        var remainingItem =
            Assert.Single(remainingDailyPlan.Items);

        Assert.Equal(
            ingredientItem.IngredientId,
            remainingItem.IngredientId);
        Assert.Null(remainingItem.PreparedRecipeId);
        Assert.Equal(
            ingredientItem.Nutrition,
            remainingDailyPlan.Nutrition);

        var deletedPreparedRecipeResponse =
            await client.GetAsync(
                $"/api/prepared-recipes/{preparedRecipe.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            deletedPreparedRecipeResponse.StatusCode);
    }
}