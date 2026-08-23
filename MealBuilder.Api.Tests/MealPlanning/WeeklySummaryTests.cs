using MealBuilder.Api.Contracts.Ingredients;
using MealBuilder.Api.Contracts.MealPlanning;
using MealBuilder.Api.Contracts.MealPlanning.DailyPlans;
using MealBuilder.Api.Contracts.MealPlanning.Weekly;
using MealBuilder.Api.Tests.Infrastructure;
using MealBuilder.Api.Tests.Recipes;
using MealBuilder.Domain.Ingredients;
using System.Net;
using System.Net.Http.Json;

namespace MealBuilder.Api.Tests.MealPlanning;

public sealed class WeeklySummaryTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task GetWeek_ExcludesEmptyAndDisabledDaysFromCalculations()
    {
        using var client = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(client);

        var builtInIngredients =
            await RecipeTestHelper.GetBuiltInIngredientsAsync(client);

        Assert.NotEmpty(builtInIngredients);

        var ingredient = builtInIngredients[0];
        var ingredientId = ingredient.Id;
        var weekStart = new DateOnly(2026, 8, 24);
        var secondDate = weekStart.AddDays(1);
        var disabledDate = weekStart.AddDays(2);

        var firstPlan = await AddIngredientAsync(
            client,
            weekStart,
            ingredientId,
            100m);

        var secondPlan = await AddIngredientAsync(
            client,
            secondDate,
            ingredientId,
            200m);

        var planToDisable = await AddIngredientAsync(
            client,
            disabledDate,
            ingredientId,
            300m);

        Assert.True(planToDisable.Id.HasValue);

        var inclusionResponse = await client.PutWithCsrfAsync(
            $"/api/daily-plans/{planToDisable.Id.Value}/weekly-summary",
            new DailyPlanInclusionRequest(
                IncludeInWeeklySummary: false));

        Assert.Equal(
            HttpStatusCode.OK,
            inclusionResponse.StatusCode);

        var disabledPlan = await inclusionResponse.Content
            .ReadFromJsonAsync<DailyPlanResponse>();

        Assert.NotNull(disabledPlan);
        Assert.False(disabledPlan.IncludeInWeeklySummary);

        var weeklySummary = await client
            .GetFromJsonAsync<WeeklySummaryResponse>(
                $"/api/daily-plans/week/{weekStart:yyyy-MM-dd}");

        Assert.NotNull(weeklySummary);
        Assert.Equal(weekStart, weeklySummary.StartDate);
        Assert.Equal(
            weekStart.AddDays(6),
            weeklySummary.EndDate);
        Assert.Equal(2, weeklySummary.IncludedDayCount);
        Assert.Equal(7, weeklySummary.Days.Count);

        var expectedTotal = CalculateNutrition(
            ingredient,
            grams: 300m);

        var expectedAverage = CalculateNutrition(
            ingredient,
            grams: 300m,
            divisor: weeklySummary.IncludedDayCount);

        Assert.Equal(
            expectedTotal,
            weeklySummary.TotalNutrition);
        Assert.Equal(
            expectedAverage,
            weeklySummary.AverageNutrition);

        var firstDay = weeklySummary.Days[0];

        Assert.True(firstDay.HasPlan);
        Assert.True(firstDay.IncludeInWeeklySummary);
        Assert.Equal(
            firstPlan.Nutrition,
            firstDay.Nutrition);

        var secondDay = weeklySummary.Days[1];

        Assert.True(secondDay.HasPlan);
        Assert.True(secondDay.IncludeInWeeklySummary);
        Assert.Equal(
            secondPlan.Nutrition,
            secondDay.Nutrition);

        var excludedDay = weeklySummary.Days[2];

        Assert.True(excludedDay.HasPlan);
        Assert.False(excludedDay.IncludeInWeeklySummary);
        Assert.Equal(
            disabledPlan.Nutrition,
            excludedDay.Nutrition);

        var emptyDay = weeklySummary.Days[3];

        Assert.False(emptyDay.HasPlan);
        Assert.False(emptyDay.IncludeInWeeklySummary);
        Assert.Null(emptyDay.DailyPlanId);
    }

    private static async Task<DailyPlanResponse>
        AddIngredientAsync(
            HttpClient client,
            DateOnly date,
            int ingredientId,
            decimal grams)
    {
        var response = await client.PostWithCsrfAsync(
            $"/api/daily-plans/{date:yyyy-MM-dd}/ingredients",
            new AddDailyPlanIngredientRequest(
                IngredientId: ingredientId,
                Grams: grams,
                PlannedTime: null));

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<DailyPlanResponse>()
            ?? throw new InvalidOperationException(
                "The Daily Plan response was empty.");
    }

    private static MealPlanningNutritionResponse CalculateNutrition(
    IngredientResponse ingredient,
    decimal grams,
    int divisor = 1)
    {
        var multiplier = grams / 100m / divisor;

        return new MealPlanningNutritionResponse(
            Round(ingredient.CaloriesPer100g * multiplier),
            Round(ingredient.ProteinPer100g * multiplier),
            Round(ingredient.FatPer100g * multiplier),
            Round(ingredient.CarbohydratesPer100g * multiplier),
            Round(ingredient.SugarsPer100g * multiplier),
            Round(ingredient.FiberPer100g * multiplier),
            Round(ingredient.SaltPer100g * multiplier));
    }

    private static decimal Round(decimal value)
    {
        return Math.Round(
            value,
            2,
            MidpointRounding.AwayFromZero);
    }
}