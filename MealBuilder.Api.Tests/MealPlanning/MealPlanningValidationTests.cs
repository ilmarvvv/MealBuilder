using System.Net;
using System.Net.Http.Json;
using MealBuilder.Api.Contracts.MealPlanning.DailyPlans;
using MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;
using MealBuilder.Api.Tests.Infrastructure;
using MealBuilder.Api.Tests.Recipes;

namespace MealBuilder.Api.Tests.MealPlanning;

public sealed class MealPlanningValidationTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task InvalidRequests_ReturnBadRequestAndSaveNothing()
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
                    "Validation recipe"));

        var preparedDate = new DateOnly(2026, 8, 24);

        var duplicateDatesResponse =
            await client.PostWithCsrfAsync(
                "/api/prepared-recipes",
                new CreatePreparedRecipeRequest(
                    RecipeId: sourceRecipe.Id,
                    PreparedDate: preparedDate,
                    TotalPortions: 3m,
                    Allocations:
                    [
                        new PreparedRecipeAllocationRequest(
                            preparedDate,
                            1m),
                        new PreparedRecipeAllocationRequest(
                            preparedDate,
                            1m)
                    ]));

        Assert.Equal(
            HttpStatusCode.BadRequest,
            duplicateDatesResponse.StatusCode);

        var invalidPreviewResponse =
            await client.PostWithCsrfAsync(
                "/api/prepared-recipes/planning-preview",
                new PreparedRecipePlanningPreviewRequest(
                    RecipeId: sourceRecipe.Id,
                    PreparedDate: preparedDate,
                    TotalPortions: 3m,
                    StartDate: preparedDate.AddDays(-1),
                    PlannedDays: 3));

        Assert.Equal(
            HttpStatusCode.BadRequest,
            invalidPreviewResponse.StatusCode);

        var invalidWeekResponse = await client.GetAsync(
            $"/api/daily-plans/week/{preparedDate.AddDays(1):yyyy-MM-dd}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            invalidWeekResponse.StatusCode);

        var preparedRecipes = await client
            .GetFromJsonAsync<
                IReadOnlyList<PreparedRecipeSummaryResponse>>(
                "/api/prepared-recipes");

        Assert.NotNull(preparedRecipes);
        Assert.Empty(preparedRecipes);

        var dailyPlan = await client
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{preparedDate:yyyy-MM-dd}");

        Assert.NotNull(dailyPlan);
        Assert.Null(dailyPlan.Id);
        Assert.Empty(dailyPlan.Items);
    }
}