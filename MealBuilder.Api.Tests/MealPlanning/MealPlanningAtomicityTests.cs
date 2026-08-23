using System.Net;
using System.Net.Http.Json;
using MealBuilder.Api.Contracts.MealPlanning.DailyPlans;
using MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;
using MealBuilder.Api.Tests.Infrastructure;
using MealBuilder.Api.Tests.Recipes;

namespace MealBuilder.Api.Tests.MealPlanning;

public sealed class MealPlanningAtomicityTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task CreatePreparedRecipe_WhenAllocationsExceedTotal_SavesNothing()
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
                    "Atomic preparation recipe"));

        var firstDate = new DateOnly(2026, 8, 24);
        var secondDate = firstDate.AddDays(1);

        var response = await client.PostWithCsrfAsync(
            "/api/prepared-recipes",
            new CreatePreparedRecipeRequest(
                RecipeId: sourceRecipe.Id,
                PreparedDate: firstDate,
                TotalPortions: 2m,
                Allocations:
                [
                    new PreparedRecipeAllocationRequest(
                        firstDate,
                        1.5m),
                    new PreparedRecipeAllocationRequest(
                        secondDate,
                        1m)
                ]));

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var preparedRecipes = await client
            .GetFromJsonAsync<
                IReadOnlyList<PreparedRecipeSummaryResponse>>(
                "/api/prepared-recipes");

        Assert.NotNull(preparedRecipes);
        Assert.Empty(preparedRecipes);

        var firstDailyPlan = await client
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{firstDate:yyyy-MM-dd}");

        Assert.NotNull(firstDailyPlan);
        Assert.Null(firstDailyPlan.Id);
        Assert.Empty(firstDailyPlan.Items);

        var secondDailyPlan = await client
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{secondDate:yyyy-MM-dd}");

        Assert.NotNull(secondDailyPlan);
        Assert.Null(secondDailyPlan.Id);
        Assert.Empty(secondDailyPlan.Items);
    }

    [Fact]
    public async Task Move_WhenAmountExceedsSource_SavesNeitherPlanChange()
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
                    "Atomic move recipe"));

        var sourceDate = new DateOnly(2026, 8, 24);
        var destinationDate = sourceDate.AddDays(1);

        var preparedRecipe =
            await MealPlanningTestHelper.CreatePreparedRecipeAsync(
                client,
                new CreatePreparedRecipeRequest(
                    RecipeId: sourceRecipe.Id,
                    PreparedDate: sourceDate,
                    TotalPortions: 3m,
                    Allocations:
                    [
                        new PreparedRecipeAllocationRequest(
                        sourceDate,
                        2m)
                    ]));

        var originalSourcePlan = await client
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{sourceDate:yyyy-MM-dd}");

        Assert.NotNull(originalSourcePlan);
        Assert.True(originalSourcePlan.Id.HasValue);

        var originalSourceItem =
            Assert.Single(originalSourcePlan.Items);

        var moveResponse = await client.PostWithCsrfAsync(
            $"/api/daily-plans/{originalSourcePlan.Id.Value}/items/{originalSourceItem.Id}/move",
            new MoveDailyPlanItemRequest(
                DestinationDate: destinationDate,
                Amount: 2.01m));

        Assert.Equal(
            HttpStatusCode.BadRequest,
            moveResponse.StatusCode);

        var unchangedSourcePlan = await client
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{sourceDate:yyyy-MM-dd}");

        Assert.NotNull(unchangedSourcePlan);

        var unchangedSourceItem =
            Assert.Single(unchangedSourcePlan.Items);

        Assert.Equal(
            originalSourceItem.Id,
            unchangedSourceItem.Id);
        Assert.Equal(
            (decimal?)2m,
            unchangedSourceItem.Portions);

        var emptyDestinationPlan = await client
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{destinationDate:yyyy-MM-dd}");

        Assert.NotNull(emptyDestinationPlan);
        Assert.Null(emptyDestinationPlan.Id);
        Assert.Empty(emptyDestinationPlan.Items);

        var availability = await client
            .GetFromJsonAsync<PreparedRecipeAvailabilityResponse>(
                $"/api/prepared-recipes/{preparedRecipe.Id}/availability");

        Assert.NotNull(availability);
        Assert.Equal(2m, availability.AllocatedPortions);
        Assert.Equal(1m, availability.AvailablePortions);
    }
}