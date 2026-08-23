using System.Net;
using System.Net.Http.Json;
using MealBuilder.Api.Contracts.MealPlanning.DailyPlans;
using MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;
using MealBuilder.Api.Tests.Infrastructure;
using MealBuilder.Api.Tests.Recipes;

namespace MealBuilder.Api.Tests.MealPlanning;

public sealed class DailyPlanItemTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task AdjustMoveAndRemove_WhenValid_UpdatesDailyPlans()
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
                    "Daily plan item recipe"));

        var sourceDate = new DateOnly(2026, 8, 24);
        var destinationDate = sourceDate.AddDays(1);

        var preparedRecipe =
            await MealPlanningTestHelper.CreatePreparedRecipeAsync(
                client,
                new CreatePreparedRecipeRequest(
                    RecipeId: sourceRecipe.Id,
                    PreparedDate: sourceDate,
                    TotalPortions: 4m,
                    Allocations:
                    [
                        new PreparedRecipeAllocationRequest(
                            sourceDate,
                            3m)
                    ]));

        var sourcePlan = await client
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{sourceDate:yyyy-MM-dd}");

        Assert.NotNull(sourcePlan);
        Assert.True(sourcePlan.Id.HasValue);

        var sourcePlanId = sourcePlan.Id.Value;
        var sourceItem = Assert.Single(sourcePlan.Items);

        var amountResponse = await client.PutWithCsrfAsync(
            $"/api/daily-plans/{sourcePlanId}/items/{sourceItem.Id}/amount",
            new DailyPlanItemAmountRequest(2.5m));

        Assert.Equal(
            HttpStatusCode.OK,
            amountResponse.StatusCode);

        var amountUpdatedPlan = await amountResponse.Content
            .ReadFromJsonAsync<DailyPlanResponse>();

        Assert.NotNull(amountUpdatedPlan);

        var amountUpdatedItem =
            Assert.Single(amountUpdatedPlan.Items);

        Assert.Equal(
            (decimal?)2.5m,
            amountUpdatedItem.Portions);

        var plannedTime = new TimeOnly(12, 30);

        var timeResponse = await client.PutWithCsrfAsync(
            $"/api/daily-plans/{sourcePlanId}/items/{sourceItem.Id}/time",
            new DailyPlanItemTimeRequest(plannedTime));

        Assert.Equal(
            HttpStatusCode.OK,
            timeResponse.StatusCode);

        var timeUpdatedPlan = await timeResponse.Content
            .ReadFromJsonAsync<DailyPlanResponse>();

        Assert.NotNull(timeUpdatedPlan);
        Assert.Equal(
            plannedTime,
            Assert.Single(timeUpdatedPlan.Items).PlannedTime);

        var moveResponse = await client.PostWithCsrfAsync(
            $"/api/daily-plans/{sourcePlanId}/items/{sourceItem.Id}/move",
            new MoveDailyPlanItemRequest(
                DestinationDate: destinationDate,
                Amount: 1m));

        Assert.Equal(
            HttpStatusCode.OK,
            moveResponse.StatusCode);

        var moveResult = await moveResponse.Content
            .ReadFromJsonAsync<MoveDailyPlanItemResponse>();

        Assert.NotNull(moveResult);
        Assert.True(moveResult.DestinationPlan.Id.HasValue);

        var remainingSourceItem =
            Assert.Single(moveResult.SourcePlan.Items);

        Assert.Equal(
            (decimal?)1.5m,
            remainingSourceItem.Portions);
        Assert.Equal(
            plannedTime,
            remainingSourceItem.PlannedTime);

        var destinationItem =
            Assert.Single(moveResult.DestinationPlan.Items);

        Assert.Equal(
            (int?)preparedRecipe.Id,
            destinationItem.PreparedRecipeId);
        Assert.Equal(
            (decimal?)1m,
            destinationItem.Portions);
        Assert.Equal(
            plannedTime,
            destinationItem.PlannedTime);

        var removeResponse = await client.DeleteWithCsrfAsync(
            $"/api/daily-plans/{moveResult.DestinationPlan.Id.Value}/items/{destinationItem.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            removeResponse.StatusCode);

        var emptiedDestinationPlan = await removeResponse.Content
            .ReadFromJsonAsync<DailyPlanResponse>();

        Assert.NotNull(emptiedDestinationPlan);
        Assert.Null(emptiedDestinationPlan.Id);
        Assert.Empty(emptiedDestinationPlan.Items);

        var availability = await client
            .GetFromJsonAsync<PreparedRecipeAvailabilityResponse>(
                $"/api/prepared-recipes/{preparedRecipe.Id}/availability");

        Assert.NotNull(availability);
        Assert.Equal(1.5m, availability.AllocatedPortions);
        Assert.Equal(2.5m, availability.AvailablePortions);
    }

    [Fact]
    public async Task ChangeAmount_WhenPortionsAreInsufficient_ReturnsBadRequest()
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
                    "Insufficient portions recipe"));

        var planDate = new DateOnly(2026, 8, 24);

        var preparedRecipe =
            await MealPlanningTestHelper.CreatePreparedRecipeAsync(
                client,
                new CreatePreparedRecipeRequest(
                    RecipeId: sourceRecipe.Id,
                    PreparedDate: planDate,
                    TotalPortions: 2m,
                    Allocations:
                    [
                        new PreparedRecipeAllocationRequest(
                        planDate,
                        2m)
                    ]));

        var originalPlan = await client
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{planDate:yyyy-MM-dd}");

        Assert.NotNull(originalPlan);
        Assert.True(originalPlan.Id.HasValue);

        var originalItem = Assert.Single(originalPlan.Items);

        var response = await client.PutWithCsrfAsync(
            $"/api/daily-plans/{originalPlan.Id.Value}/items/{originalItem.Id}/amount",
            new DailyPlanItemAmountRequest(2.01m));

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var unchangedPlan = await client
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{planDate:yyyy-MM-dd}");

        Assert.NotNull(unchangedPlan);

        var unchangedItem =
            Assert.Single(unchangedPlan.Items);

        Assert.Equal(
            originalItem.Id,
            unchangedItem.Id);
        Assert.Equal(
            (decimal?)2m,
            unchangedItem.Portions);

        var availability = await client
            .GetFromJsonAsync<PreparedRecipeAvailabilityResponse>(
                $"/api/prepared-recipes/{preparedRecipe.Id}/availability");

        Assert.NotNull(availability);
        Assert.Equal(2m, availability.AllocatedPortions);
        Assert.Equal(0m, availability.AvailablePortions);
    }
}