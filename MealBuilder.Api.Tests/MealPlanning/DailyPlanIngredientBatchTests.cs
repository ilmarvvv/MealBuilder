using System.Net;
using System.Net.Http.Json;
using MealBuilder.Api.Contracts.MealPlanning.DailyPlans;
using MealBuilder.Api.Tests.Infrastructure;
using MealBuilder.Api.Tests.Recipes;

namespace MealBuilder.Api.Tests.MealPlanning;

public sealed class DailyPlanIngredientBatchTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task AddIngredientBatch_AddsDifferentAmountsToDifferentDays()
    {
        using var client = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(client);

        var ingredients =
            await RecipeTestHelper.GetBuiltInIngredientsAsync(
                client);

        Assert.NotEmpty(ingredients);

        var ingredientId = ingredients[0].Id;
        var firstDate = new DateOnly(2026, 9, 24);
        var secondDate = firstDate.AddDays(1);
        var firstTime = new TimeOnly(10, 30);

        var response = await client.PostWithCsrfAsync(
            "/api/daily-plans/ingredients/batch",
            new AddDailyPlanIngredientBatchRequest(
                IngredientId: ingredientId,
                Entries:
                [
                    new AddDailyPlanIngredientBatchEntryRequest(
                        Date: firstDate,
                        Grams: 90m,
                        PlannedTime: firstTime),

                    new AddDailyPlanIngredientBatchEntryRequest(
                        Date: secondDate,
                        Grams: 120m,
                        PlannedTime: null)
                ]));

        response.EnsureSuccessStatusCode();

        var dailyPlans = await response.Content
            .ReadFromJsonAsync<
                IReadOnlyList<DailyPlanResponse>>();

        Assert.NotNull(dailyPlans);
        Assert.Equal(2, dailyPlans.Count);

        var firstPlan = dailyPlans.Single(
            dailyPlan => dailyPlan.Date == firstDate);

        var firstItem = Assert.Single(firstPlan.Items);

        Assert.Equal(ingredientId, firstItem.IngredientId);
        Assert.Equal((decimal?)90m, firstItem.Grams);
        Assert.Equal((TimeOnly?)firstTime, firstItem.PlannedTime);

        var secondPlan = dailyPlans.Single(
            dailyPlan => dailyPlan.Date == secondDate);

        var secondItem = Assert.Single(secondPlan.Items);

        Assert.Equal(ingredientId, secondItem.IngredientId);
        Assert.Equal((decimal?)120m, secondItem.Grams);
        Assert.Null(secondItem.PlannedTime);
    }

    [Fact]
    public async Task AddIngredientBatch_WithDuplicateDates_SavesNothing()
    {
        using var client = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(client);

        var ingredients =
            await RecipeTestHelper.GetBuiltInIngredientsAsync(
                client);

        Assert.NotEmpty(ingredients);

        var ingredientId = ingredients[0].Id;
        var date = new DateOnly(2026, 9, 26);

        var response = await client.PostWithCsrfAsync(
            "/api/daily-plans/ingredients/batch",
            new AddDailyPlanIngredientBatchRequest(
                IngredientId: ingredientId,
                Entries:
                [
                    new AddDailyPlanIngredientBatchEntryRequest(
                        Date: date,
                        Grams: 90m,
                        PlannedTime: null),

                    new AddDailyPlanIngredientBatchEntryRequest(
                        Date: date,
                        Grams: 120m,
                        PlannedTime: null)
                ]));

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var dailyPlan = await client
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{date:yyyy-MM-dd}");

        Assert.NotNull(dailyPlan);
        Assert.Null(dailyPlan.Id);
        Assert.Empty(dailyPlan.Items);
    }
}