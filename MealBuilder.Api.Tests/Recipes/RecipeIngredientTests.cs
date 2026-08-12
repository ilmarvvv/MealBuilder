using System.Net;
using System.Net.Http.Json;
using MealBuilder.Api.Contracts.Recipes;
using MealBuilder.Api.Tests.Infrastructure;

namespace MealBuilder.Api.Tests.Recipes;

public sealed class RecipeIngredientTests(
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

        Assert.True(builtInIngredients.Count >= 2);

        var firstIngredient = builtInIngredients[0];
        var secondIngredient = builtInIngredients[1];

        var recipe = await RecipeTestHelper.CreateRecipeAsync(
            client,
            RecipeTestHelper.CreateValidRequest(
                firstIngredient.Id));

        var addResponse = await client.PostWithCsrfAsync(
            $"/api/recipes/{recipe.Id}/ingredients",
            new RecipeIngredientRequest(
                secondIngredient.Id,
                100m));

        Assert.Equal(
            HttpStatusCode.OK,
            addResponse.StatusCode);

        var addedIngredient = await addResponse.Content
            .ReadFromJsonAsync<RecipeIngredientResponse>();

        Assert.NotNull(addedIngredient);
        Assert.Equal(secondIngredient.Id, addedIngredient.IngredientId);
        Assert.Equal(2, addedIngredient.Position);

        var updateResponse = await client.PutWithCsrfAsync(
            $"/api/recipes/{recipe.Id}/ingredients/{firstIngredient.Id}",
            new RecipeIngredientGramsRequest(75m));

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode);

        var updatedIngredient = await updateResponse.Content
            .ReadFromJsonAsync<RecipeIngredientResponse>();

        Assert.NotNull(updatedIngredient);
        Assert.Equal(75m, updatedIngredient.Grams);

        var moveResponse = await client.PutWithCsrfAsync(
            $"/api/recipes/{recipe.Id}/ingredients/" +
            $"{secondIngredient.Id}/position",
            new RecipePositionRequest(1));

        Assert.Equal(
            HttpStatusCode.NoContent,
            moveResponse.StatusCode);

        var reorderedRecipe = await client
            .GetFromJsonAsync<RecipeResponse>(
                $"/api/recipes/{recipe.Id}");

        Assert.NotNull(reorderedRecipe);

        Assert.Collection(
            reorderedRecipe.Ingredients,
            ingredient =>
            {
                Assert.Equal(
                    secondIngredient.Id,
                    ingredient.IngredientId);
                Assert.Equal(1, ingredient.Position);
            },
            ingredient =>
            {
                Assert.Equal(
                    firstIngredient.Id,
                    ingredient.IngredientId);
                Assert.Equal(75m, ingredient.Grams);
                Assert.Equal(2, ingredient.Position);
            });

        var deleteResponse = await client.DeleteWithCsrfAsync(
            $"/api/recipes/{recipe.Id}/ingredients/" +
            $"{firstIngredient.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var recipeAfterDelete = await client
            .GetFromJsonAsync<RecipeResponse>(
                $"/api/recipes/{recipe.Id}");

        Assert.NotNull(recipeAfterDelete);

        Assert.Collection(
            recipeAfterDelete.Ingredients,
            ingredient =>
            {
                Assert.Equal(
                    secondIngredient.Id,
                    ingredient.IngredientId);
                Assert.Equal(1, ingredient.Position);
            });

        var deleteLastResponse =
            await client.DeleteWithCsrfAsync(
                $"/api/recipes/{recipe.Id}/ingredients/" +
                $"{secondIngredient.Id}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            deleteLastResponse.StatusCode);

        var finalRecipe = await client
            .GetFromJsonAsync<RecipeResponse>(
                $"/api/recipes/{recipe.Id}");

        Assert.NotNull(finalRecipe);
        Assert.Single(finalRecipe.Ingredients);
    }
}