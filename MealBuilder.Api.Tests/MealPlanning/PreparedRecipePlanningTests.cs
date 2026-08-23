using System.Net;
using System.Net.Http.Json;
using MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;
using MealBuilder.Api.Tests.Infrastructure;
using MealBuilder.Api.Tests.Recipes;
using MealBuilder.Api.Contracts.MealPlanning.DailyPlans;

namespace MealBuilder.Api.Tests.MealPlanning;

public sealed class PreparedRecipePlanningTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task PreviewPlanning_WithMultipleDays_ReturnsBalancedDistribution()
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
                    "Planning preview recipe"));

        var startDate = new DateOnly(2026, 8, 24);

        var response = await client.PostWithCsrfAsync(
            "/api/prepared-recipes/planning-preview",
            new PreparedRecipePlanningPreviewRequest(
                RecipeId: sourceRecipe.Id,
                PreparedDate: new DateOnly(2026, 8, 23),
                TotalPortions: 4m,
                StartDate: startDate,
                PlannedDays: 3));

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var allocations = await response.Content
            .ReadFromJsonAsync<
                IReadOnlyList<PreparedRecipeAllocationResponse>>();

        Assert.NotNull(allocations);

        Assert.Collection(
            allocations,
            allocation =>
            {
                Assert.Equal(startDate, allocation.Date);
                Assert.Equal(1.34m, allocation.Portions);
            },
            allocation =>
            {
                Assert.Equal(
                    startDate.AddDays(1),
                    allocation.Date);
                Assert.Equal(1.33m, allocation.Portions);
            },
            allocation =>
            {
                Assert.Equal(
                    startDate.AddDays(2),
                    allocation.Date);
                Assert.Equal(1.33m, allocation.Portions);
            });

        var preparedRecipes = await client
            .GetFromJsonAsync<
                IReadOnlyList<PreparedRecipeSummaryResponse>>(
                "/api/prepared-recipes");

        Assert.NotNull(preparedRecipes);
        Assert.Empty(preparedRecipes);
    }

    [Fact]
    public async Task Create_WithOptionalAllocations_UpdatesAvailabilityAndDailyPlans()
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
                    "Allocated prepared recipe"));

        var preparedDate = new DateOnly(2026, 8, 24);
        var secondDate = preparedDate.AddDays(2);

        var preparedRecipe =
            await MealPlanningTestHelper.CreatePreparedRecipeAsync(
                client,
                new CreatePreparedRecipeRequest(
                    RecipeId: sourceRecipe.Id,
                    PreparedDate: preparedDate,
                    TotalPortions: 5m,
                    Allocations:
                    [
                        new PreparedRecipeAllocationRequest(
                        preparedDate,
                        2.5m),
                    new PreparedRecipeAllocationRequest(
                        secondDate,
                        1m)
                    ]));

        Assert.Equal(5m, preparedRecipe.TotalPortions);
        Assert.Equal(3.5m, preparedRecipe.AllocatedPortions);
        Assert.Equal(1.5m, preparedRecipe.AvailablePortions);

        var availability = await client
            .GetFromJsonAsync<PreparedRecipeAvailabilityResponse>(
                $"/api/prepared-recipes/{preparedRecipe.Id}/availability");

        Assert.NotNull(availability);
        Assert.Equal(
            preparedRecipe.Id,
            availability.PreparedRecipeId);
        Assert.Equal(5m, availability.TotalPortions);
        Assert.Equal(3.5m, availability.AllocatedPortions);
        Assert.Equal(1.5m, availability.AvailablePortions);

        var firstDailyPlan = await client
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{preparedDate:yyyy-MM-dd}");

        Assert.NotNull(firstDailyPlan);

        var firstItem = Assert.Single(firstDailyPlan.Items);

        Assert.Equal(
            (int?)preparedRecipe.Id,
            firstItem.PreparedRecipeId);
        Assert.Equal((decimal?)2.5m, firstItem.Portions);

        var secondDailyPlan = await client
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{secondDate:yyyy-MM-dd}");

        Assert.NotNull(secondDailyPlan);

        var secondItem = Assert.Single(secondDailyPlan.Items);

        Assert.Equal(
            (int?)preparedRecipe.Id,
            secondItem.PreparedRecipeId);
        Assert.Equal((decimal?)1m, secondItem.Portions);
    }
}