using System.Net;
using System.Net.Http.Json;
using MealBuilder.Api.Contracts.Recipes;
using MealBuilder.Api.Tests.Infrastructure;

namespace MealBuilder.Api.Tests.Recipes;

public sealed class RecipeTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task CompleteRecipeCrud_WhenOwnedByUser_Succeeds()
    {
        using var client = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(client);

        var builtInIngredients =
            await RecipeTestHelper.GetBuiltInIngredientsAsync(client);

        Assert.True(builtInIngredients.Count >= 2);

        var createRequest =
            RecipeTestHelper.CreateValidRequest(
                builtInIngredients[0].Id);

        var createResponse = await client.PostWithCsrfAsync(
            "/api/recipes",
            createRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var createdRecipe = await createResponse.Content
            .ReadFromJsonAsync<RecipeResponse>();

        Assert.NotNull(createdRecipe);
        Assert.True(createdRecipe.Id > 0);
        Assert.Equal(createRequest.Name, createdRecipe.Name);
        Assert.Equal(createRequest.Servings, createdRecipe.Servings);

        Assert.Collection(
            createdRecipe.Ingredients,
            ingredient =>
            {
                Assert.Equal(
                    builtInIngredients[0].Id,
                    ingredient.IngredientId);
                Assert.Equal(200m, ingredient.Grams);
                Assert.Equal(1, ingredient.Position);
            });

        Assert.Collection(
            createdRecipe.Steps,
            firstStep => Assert.Equal(1, firstStep.Position),
            secondStep => Assert.Equal(2, secondStep.Position));

        var listResponse = await client.GetAsync(
            "/api/recipes");

        Assert.Equal(
            HttpStatusCode.OK,
            listResponse.StatusCode);

        var recipes = await listResponse.Content
            .ReadFromJsonAsync<
                IReadOnlyList<RecipeSummaryResponse>>();

        Assert.NotNull(recipes);
        Assert.Contains(
            recipes,
            recipe => recipe.Id == createdRecipe.Id);

        var detailsResponse = await client.GetAsync(
            $"/api/recipes/{createdRecipe.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            detailsResponse.StatusCode);

        var updateRequest = createRequest with
        {
            Name = "Updated recipe",
            Description = null,
            Servings = 4,
            Ingredients =
            [
                new RecipeIngredientRequest(
                    builtInIngredients[1].Id,
                    150m),
                new RecipeIngredientRequest(
                    builtInIngredients[0].Id,
                    50m)
            ],
            Steps =
            [
                new RecipeStepRequest("First updated step."),
                new RecipeStepRequest("Second updated step."),
                new RecipeStepRequest("Third updated step.")
            ]
        };

        var updateResponse = await client.PutWithCsrfAsync(
            $"/api/recipes/{createdRecipe.Id}",
            updateRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode);

        var updatedRecipe = await updateResponse.Content
            .ReadFromJsonAsync<RecipeResponse>();

        Assert.NotNull(updatedRecipe);
        Assert.Equal(updateRequest.Name, updatedRecipe.Name);
        Assert.Null(updatedRecipe.Description);
        Assert.Equal(4, updatedRecipe.Servings);

        Assert.Collection(
            updatedRecipe.Ingredients,
            firstIngredient =>
            {
                Assert.Equal(
                    builtInIngredients[1].Id,
                    firstIngredient.IngredientId);
                Assert.Equal(150m, firstIngredient.Grams);
                Assert.Equal(1, firstIngredient.Position);
            },
            secondIngredient =>
            {
                Assert.Equal(
                    builtInIngredients[0].Id,
                    secondIngredient.IngredientId);
                Assert.Equal(50m, secondIngredient.Grams);
                Assert.Equal(2, secondIngredient.Position);
            });

        Assert.Collection(
            updatedRecipe.Steps,
            firstStep =>
            {
                Assert.Equal(
                    "First updated step.",
                    firstStep.Instruction);
                Assert.Equal(1, firstStep.Position);
            },
            secondStep =>
                Assert.Equal(2, secondStep.Position),
            thirdStep =>
                Assert.Equal(3, thirdStep.Position));

        var deleteResponse = await client.DeleteWithCsrfAsync(
            $"/api/recipes/{createdRecipe.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var deletedRecipeResponse = await client.GetAsync(
            $"/api/recipes/{createdRecipe.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            deletedRecipeResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WithMultipleIngredients_ReturnsOrderedNutrition()
    {
        using var client = factory.CreateHttpsClient();

        await RecipeTestHelper.RegisterUserAsync(client);

        var builtInIngredients =
            await RecipeTestHelper.GetBuiltInIngredientsAsync(client);

        Assert.True(builtInIngredients.Count >= 2);

        var firstIngredient = builtInIngredients[0];
        var secondIngredient = builtInIngredients[1];

        const decimal firstGrams = 150m;
        const decimal secondGrams = 75m;
        const int servings = 3;

        var request = new RecipeRequest(
            Name: "Nutrition calculation recipe",
            Description: null,
            Servings: servings,
            Ingredients:
            [
                new RecipeIngredientRequest(
                    firstIngredient.Id,
                    firstGrams),
                new RecipeIngredientRequest(
                    secondIngredient.Id,
                    secondGrams)
            ],
            Steps:
            [
                new RecipeStepRequest("First step."),
                new RecipeStepRequest("Second step."),
                new RecipeStepRequest("Third step.")
            ]);

        var recipe = await RecipeTestHelper.CreateRecipeAsync(
            client,
            request);

        Assert.Collection(
            recipe.Ingredients,
            ingredient =>
            {
                Assert.Equal(
                    firstIngredient.Id,
                    ingredient.IngredientId);
                Assert.Equal(1, ingredient.Position);
            },
            ingredient =>
            {
                Assert.Equal(
                    secondIngredient.Id,
                    ingredient.IngredientId);
                Assert.Equal(2, ingredient.Position);
            });

        Assert.Collection(
            recipe.Steps,
            step => Assert.Equal(1, step.Position),
            step => Assert.Equal(2, step.Position),
            step => Assert.Equal(3, step.Position));

        var expectedTotal = new RecipeNutritionResponse(
            Calories: CalculateNutrition(
                firstIngredient.CaloriesPer100g,
                firstGrams,
                secondIngredient.CaloriesPer100g,
                secondGrams),
            Protein: CalculateNutrition(
                firstIngredient.ProteinPer100g,
                firstGrams,
                secondIngredient.ProteinPer100g,
                secondGrams),
            Fat: CalculateNutrition(
                firstIngredient.FatPer100g,
                firstGrams,
                secondIngredient.FatPer100g,
                secondGrams),
            Carbohydrates: CalculateNutrition(
                firstIngredient.CarbohydratesPer100g,
                firstGrams,
                secondIngredient.CarbohydratesPer100g,
                secondGrams),
            Sugars: CalculateNutrition(
                firstIngredient.SugarsPer100g,
                firstGrams,
                secondIngredient.SugarsPer100g,
                secondGrams),
            Fiber: CalculateNutrition(
                firstIngredient.FiberPer100g,
                firstGrams,
                secondIngredient.FiberPer100g,
                secondGrams),
            Salt: CalculateNutrition(
                firstIngredient.SaltPer100g,
                firstGrams,
                secondIngredient.SaltPer100g,
                secondGrams));

        var expectedPerServing = new RecipeNutritionResponse(
            Calories: CalculateNutrition(
                firstIngredient.CaloriesPer100g,
                firstGrams,
                secondIngredient.CaloriesPer100g,
                secondGrams,
                servings),
            Protein: CalculateNutrition(
                firstIngredient.ProteinPer100g,
                firstGrams,
                secondIngredient.ProteinPer100g,
                secondGrams,
                servings),
            Fat: CalculateNutrition(
                firstIngredient.FatPer100g,
                firstGrams,
                secondIngredient.FatPer100g,
                secondGrams,
                servings),
            Carbohydrates: CalculateNutrition(
                firstIngredient.CarbohydratesPer100g,
                firstGrams,
                secondIngredient.CarbohydratesPer100g,
                secondGrams,
                servings),
            Sugars: CalculateNutrition(
                firstIngredient.SugarsPer100g,
                firstGrams,
                secondIngredient.SugarsPer100g,
                secondGrams,
                servings),
            Fiber: CalculateNutrition(
                firstIngredient.FiberPer100g,
                firstGrams,
                secondIngredient.FiberPer100g,
                secondGrams,
                servings),
            Salt: CalculateNutrition(
                firstIngredient.SaltPer100g,
                firstGrams,
                secondIngredient.SaltPer100g,
                secondGrams,
                servings));

        Assert.Equal(
            expectedTotal,
            recipe.TotalNutrition);

        Assert.Equal(
            expectedPerServing,
            recipe.NutritionPerServing);
    }

    private static decimal CalculateNutrition(
        decimal firstValuePer100g,
        decimal firstGrams,
        decimal secondValuePer100g,
        decimal secondGrams,
        int divisor = 1)
    {
        var total =
            firstValuePer100g * firstGrams / 100m +
            secondValuePer100g * secondGrams / 100m;

        return Math.Round(
            total / divisor,
            2,
            MidpointRounding.AwayFromZero);
    }
}