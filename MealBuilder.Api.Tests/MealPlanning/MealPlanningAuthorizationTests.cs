using System.Net;
using System.Net.Http.Json;
using MealBuilder.Api.Contracts.MealPlanning.DailyPlans;
using MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;
using MealBuilder.Api.Tests.Infrastructure;
using MealBuilder.Api.Tests.Recipes;

namespace MealBuilder.Api.Tests.MealPlanning;

public sealed class MealPlanningAuthorizationTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task Resources_WhenAccessedByAnotherUser_AreHiddenAndUnchanged()
    {
        using var ownerClient = factory.CreateHttpsClient();
        using var otherUserClient = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(ownerClient);
        await RecipeTestHelper.RegisterUserAsync(otherUserClient);

        var builtInIngredients =
            await RecipeTestHelper.GetBuiltInIngredientsAsync(
                ownerClient);

        Assert.NotEmpty(builtInIngredients);

        var sourceRecipe =
            await RecipeTestHelper.CreateRecipeAsync(
                ownerClient,
                RecipeTestHelper.CreateValidRequest(
                    builtInIngredients[0].Id,
                    "Private meal planning recipe"));

        var planDate = new DateOnly(2026, 8, 24);

        var preparedRecipe =
            await MealPlanningTestHelper.CreatePreparedRecipeAsync(
                ownerClient,
                new CreatePreparedRecipeRequest(
                    RecipeId: sourceRecipe.Id,
                    PreparedDate: planDate,
                    TotalPortions: 3m,
                    Allocations:
                    [
                        new PreparedRecipeAllocationRequest(
                            planDate,
                            1m)
                    ]));

        var ownerPlan = await ownerClient
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{planDate:yyyy-MM-dd}");

        Assert.NotNull(ownerPlan);
        Assert.True(ownerPlan.Id.HasValue);

        var ownerItem = Assert.Single(ownerPlan.Items);

        var otherUserPreparedRecipes = await otherUserClient
            .GetFromJsonAsync<
                IReadOnlyList<PreparedRecipeSummaryResponse>>(
                "/api/prepared-recipes");

        Assert.NotNull(otherUserPreparedRecipes);
        Assert.Empty(otherUserPreparedRecipes);

        var detailsResponse = await otherUserClient.GetAsync(
            $"/api/prepared-recipes/{preparedRecipe.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            detailsResponse.StatusCode);

        var otherUserPlan = await otherUserClient
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{planDate:yyyy-MM-dd}");

        Assert.NotNull(otherUserPlan);
        Assert.Null(otherUserPlan.Id);
        Assert.Empty(otherUserPlan.Items);

        var updateResponse =
            await otherUserClient.PutWithCsrfAsync(
                $"/api/daily-plans/{ownerPlan.Id.Value}/items/{ownerItem.Id}/amount",
                new DailyPlanItemAmountRequest(2m));

        Assert.Equal(
            HttpStatusCode.NotFound,
            updateResponse.StatusCode);

        var deleteResponse =
            await otherUserClient.DeleteWithCsrfAsync(
                $"/api/prepared-recipes/{preparedRecipe.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            deleteResponse.StatusCode);

        var createFromOwnerRecipeResponse =
            await otherUserClient.PostWithCsrfAsync(
                "/api/prepared-recipes",
                new CreatePreparedRecipeRequest(
                    RecipeId: sourceRecipe.Id,
                    PreparedDate: planDate,
                    TotalPortions: 1m,
                    Allocations: []));

        Assert.Equal(
            HttpStatusCode.BadRequest,
            createFromOwnerRecipeResponse.StatusCode);

        var preservedPreparedRecipe = await ownerClient
            .GetFromJsonAsync<PreparedRecipeResponse>(
                $"/api/prepared-recipes/{preparedRecipe.Id}");

        Assert.NotNull(preservedPreparedRecipe);
        Assert.Equal(
            preparedRecipe.Id,
            preservedPreparedRecipe.Id);

        var preservedOwnerPlan = await ownerClient
            .GetFromJsonAsync<DailyPlanResponse>(
                $"/api/daily-plans/{planDate:yyyy-MM-dd}");

        Assert.NotNull(preservedOwnerPlan);

        var preservedOwnerItem =
            Assert.Single(preservedOwnerPlan.Items);

        Assert.Equal(ownerItem.Id, preservedOwnerItem.Id);
        Assert.Equal(
            (decimal?)1m,
            preservedOwnerItem.Portions);
    }
}