using System.Net;
using System.Net.Http.Json;
using MealBuilder.Api.Contracts.Recipes;
using MealBuilder.Api.Tests.Infrastructure;

namespace MealBuilder.Api.Tests.Recipes;

public sealed class RecipeStepTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task Operations_WhenRecipeOwnedByUser_MaintainOrdering()
    {
        using var client = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(client);

        var builtInIngredients =
            await RecipeTestHelper.GetBuiltInIngredientsAsync(client);

        Assert.NotEmpty(builtInIngredients);

        var recipe = await RecipeTestHelper.CreateRecipeAsync(
            client,
            RecipeTestHelper.CreateValidRequest(
                builtInIngredients[0].Id));

        Assert.Equal(2, recipe.Steps.Count);

        var originalFirstStep = recipe.Steps[0];
        var originalSecondStep = recipe.Steps[1];

        var addResponse = await client.PostWithCsrfAsync(
            $"/api/recipes/{recipe.Id}/steps",
            new RecipeStepRequest("Third step."));

        Assert.Equal(
            HttpStatusCode.OK,
            addResponse.StatusCode);

        var addedStep = await addResponse.Content
            .ReadFromJsonAsync<RecipeStepResponse>();

        Assert.NotNull(addedStep);
        Assert.True(addedStep.Id > 0);
        Assert.Equal(3, addedStep.Position);

        var updateResponse = await client.PutWithCsrfAsync(
            $"/api/recipes/{recipe.Id}/steps/{addedStep.Id}",
            new RecipeStepRequest("Updated third step."));

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode);

        var updatedStep = await updateResponse.Content
            .ReadFromJsonAsync<RecipeStepResponse>();

        Assert.NotNull(updatedStep);
        Assert.Equal(
            "Updated third step.",
            updatedStep.Instruction);

        var moveResponse = await client.PutWithCsrfAsync(
            $"/api/recipes/{recipe.Id}/steps/" +
            $"{addedStep.Id}/position",
            new RecipePositionRequest(1));

        Assert.Equal(
            HttpStatusCode.NoContent,
            moveResponse.StatusCode);

        var reorderedRecipe = await client
            .GetFromJsonAsync<RecipeResponse>(
                $"/api/recipes/{recipe.Id}");

        Assert.NotNull(reorderedRecipe);

        Assert.Collection(
            reorderedRecipe.Steps,
            step =>
            {
                Assert.Equal(addedStep.Id, step.Id);
                Assert.Equal(1, step.Position);
            },
            step =>
            {
                Assert.Equal(originalFirstStep.Id, step.Id);
                Assert.Equal(2, step.Position);
            },
            step =>
            {
                Assert.Equal(originalSecondStep.Id, step.Id);
                Assert.Equal(3, step.Position);
            });

        var deleteFirstResponse =
            await client.DeleteWithCsrfAsync(
                $"/api/recipes/{recipe.Id}/steps/" +
                $"{originalFirstStep.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteFirstResponse.StatusCode);

        var deleteSecondResponse =
            await client.DeleteWithCsrfAsync(
                $"/api/recipes/{recipe.Id}/steps/" +
                $"{originalSecondStep.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteSecondResponse.StatusCode);

        var recipeWithOneStep = await client
            .GetFromJsonAsync<RecipeResponse>(
                $"/api/recipes/{recipe.Id}");

        Assert.NotNull(recipeWithOneStep);

        Assert.Collection(
            recipeWithOneStep.Steps,
            step =>
            {
                Assert.Equal(addedStep.Id, step.Id);
                Assert.Equal(1, step.Position);
            });

        var deleteLastResponse =
            await client.DeleteWithCsrfAsync(
                $"/api/recipes/{recipe.Id}/steps/" +
                $"{addedStep.Id}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            deleteLastResponse.StatusCode);

        var finalRecipe = await client
            .GetFromJsonAsync<RecipeResponse>(
                $"/api/recipes/{recipe.Id}");

        Assert.NotNull(finalRecipe);
        Assert.Single(finalRecipe.Steps);
    }
}