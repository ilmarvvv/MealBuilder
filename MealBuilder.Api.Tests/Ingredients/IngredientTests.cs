using MealBuilder.Api.Contracts.Authentication;
using MealBuilder.Api.Contracts.Ingredients;
using MealBuilder.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace MealBuilder.Api.Tests.Ingredients;

public sealed class IngredientTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task CreateReadUpdateDelete_WhenOwnedByUser_Succeeds()
    {
        using var client = factory.CreateHttpsClient();

        await RegisterUserAsync(client);

        var createRequest = new IngredientRequest(
            Name: "Test ingredient",
            CaloriesPer100g: 100m,
            ProteinPer100g: 10m,
            FatPer100g: 5m,
            CarbohydratesPer100g: 12m,
            SugarsPer100g: 3m,
            FiberPer100g: 2m,
            SaltPer100g: 0.5m);

        var createResponse = await client.PostWithCsrfAsync(
            "/api/ingredients",
            createRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var createdIngredient = await createResponse.Content
            .ReadFromJsonAsync<IngredientResponse>();

        Assert.NotNull(createdIngredient);
        Assert.Equal(createRequest.Name, createdIngredient.Name);
        Assert.False(createdIngredient.IsBuiltIn);

        var detailsResponse = await client.GetAsync(
            $"/api/ingredients/{createdIngredient.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            detailsResponse.StatusCode);

        var ingredientDetails = await detailsResponse.Content
            .ReadFromJsonAsync<IngredientResponse>();

        Assert.Equal(createdIngredient, ingredientDetails);

        var updateRequest = createRequest with
        {
            Name = "Updated ingredient",
            CaloriesPer100g = 120m
        };

        var updateResponse = await client.PutWithCsrfAsync(
            $"/api/ingredients/{createdIngredient.Id}",
            updateRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode);

        var updatedIngredient = await updateResponse.Content
            .ReadFromJsonAsync<IngredientResponse>();

        Assert.NotNull(updatedIngredient);
        Assert.Equal(
            createdIngredient.Id,
            updatedIngredient.Id);
        Assert.Equal(
            updateRequest.Name,
            updatedIngredient.Name);
        Assert.Equal(
            updateRequest.CaloriesPer100g,
            updatedIngredient.CaloriesPer100g);

        var deleteResponse = await client.DeleteWithCsrfAsync(
            $"/api/ingredients/{createdIngredient.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var deletedIngredientResponse = await client.GetAsync(
            $"/api/ingredients/{createdIngredient.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            deletedIngredientResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WithOnlyName_UsesZeroNutritionValues()
    {
        using var client = factory.CreateHttpsClient();

        await RegisterUserAsync(client);

        var request = new
        {
            Name = "Zero nutrition ingredient"
        };

        var response = await client.PostWithCsrfAsync(
            "/api/ingredients",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var ingredient = await response.Content
            .ReadFromJsonAsync<IngredientResponse>();

        Assert.NotNull(ingredient);
        Assert.Equal(0m, ingredient.CaloriesPer100g);
        Assert.Equal(0m, ingredient.ProteinPer100g);
        Assert.Equal(0m, ingredient.FatPer100g);
        Assert.Equal(0m, ingredient.CarbohydratesPer100g);
        Assert.Equal(0m, ingredient.SugarsPer100g);
        Assert.Equal(0m, ingredient.FiberPer100g);
        Assert.Equal(0m, ingredient.SaltPer100g);
    }

    [Fact]
    public async Task Create_WithDuplicateName_CreatesSeparateIngredients()
    {
        using var client = factory.CreateHttpsClient();

        await RegisterUserAsync(client);

        var request = new IngredientRequest(
            Name: "Duplicate ingredient",
            CaloriesPer100g: 100m,
            ProteinPer100g: 10m,
            FatPer100g: 5m,
            CarbohydratesPer100g: 12m,
            SugarsPer100g: 3m,
            FiberPer100g: 2m,
            SaltPer100g: 0.5m);

        var firstResponse = await client.PostWithCsrfAsync(
            "/api/ingredients",
            request);

        var secondResponse = await client.PostWithCsrfAsync(
            "/api/ingredients",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);
        Assert.Equal(
            HttpStatusCode.Created,
            secondResponse.StatusCode);

        var firstIngredient = await firstResponse.Content
            .ReadFromJsonAsync<IngredientResponse>();

        var secondIngredient = await secondResponse.Content
            .ReadFromJsonAsync<IngredientResponse>();

        Assert.NotNull(firstIngredient);
        Assert.NotNull(secondIngredient);
        Assert.Equal(firstIngredient.Name, secondIngredient.Name);
        Assert.NotEqual(firstIngredient.Id, secondIngredient.Id);
    }

    [Fact]
    public async Task GetAll_WhenAuthenticated_ReturnsBuiltInIngredients()
    {
        using var client = factory.CreateHttpsClient();

        await RegisterUserAsync(client);

        var listResponse = await client.GetAsync(
            "/api/ingredients");

        Assert.Equal(
            HttpStatusCode.OK,
            listResponse.StatusCode);

        var ingredients = await listResponse.Content
            .ReadFromJsonAsync<IReadOnlyList<IngredientResponse>>();

        Assert.NotNull(ingredients);

        var builtInIngredients = ingredients
            .Where(ingredient => ingredient.IsBuiltIn)
            .ToArray();

        Assert.Equal(20, builtInIngredients.Length);

        Assert.All(
            builtInIngredients,
            ingredient =>
            {
                Assert.NotNull(ingredient.SourceName);
                Assert.NotNull(ingredient.SourceCode);
                Assert.NotNull(ingredient.SourceVersion);
            });

        var expectedIngredient = builtInIngredients[0];

        var detailsResponse = await client.GetAsync(
            $"/api/ingredients/{expectedIngredient.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            detailsResponse.StatusCode);

        var ingredientDetails = await detailsResponse.Content
            .ReadFromJsonAsync<IngredientResponse>();

        Assert.Equal(expectedIngredient, ingredientDetails);
    }

    [Fact]
    public async Task UpdateAndDelete_WhenIngredientIsBuiltIn_ReturnForbidden()
    {
        using var client = factory.CreateHttpsClient();

        await RegisterUserAsync(client);

        var ingredients = await client
            .GetFromJsonAsync<IReadOnlyList<IngredientResponse>>(
                "/api/ingredients");

        Assert.NotNull(ingredients);

        var builtInIngredient = ingredients.First(
            ingredient => ingredient.IsBuiltIn);

        var updateRequest = new IngredientRequest(
            Name: "Changed built-in ingredient",
            CaloriesPer100g: builtInIngredient.CaloriesPer100g,
            ProteinPer100g: builtInIngredient.ProteinPer100g,
            FatPer100g: builtInIngredient.FatPer100g,
            CarbohydratesPer100g:
                builtInIngredient.CarbohydratesPer100g,
            SugarsPer100g: builtInIngredient.SugarsPer100g,
            FiberPer100g: builtInIngredient.FiberPer100g,
            SaltPer100g: builtInIngredient.SaltPer100g);

        var updateResponse = await client.PutWithCsrfAsync(
            $"/api/ingredients/{builtInIngredient.Id}",
            updateRequest);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            updateResponse.StatusCode);

        var deleteResponse = await client.DeleteWithCsrfAsync(
            $"/api/ingredients/{builtInIngredient.Id}");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WithNegativeCalories_ReturnsValidationError()
    {
        using var client = factory.CreateHttpsClient();

        await RegisterUserAsync(client);

        var request = new
        {
            Name = "Invalid calories ingredient",
            CaloriesPer100g = -1m
        };

        var response = await client.PostWithCsrfAsync(
            "/api/ingredients",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Contains(
            nameof(IngredientRequest.CaloriesPer100g),
            problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_WhenSugarsExceedCarbohydrates_ReturnsValidationError()
    {
        using var client = factory.CreateHttpsClient();

        await RegisterUserAsync(client);

        var request = new
        {
            Name = "Invalid sugars ingredient",
            CarbohydratesPer100g = 10m,
            SugarsPer100g = 11m
        };

        var response = await client.PostWithCsrfAsync(
            "/api/ingredients",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Contains(
            nameof(IngredientRequest.SugarsPer100g),
            problem.Errors.Keys);
    }

    [Fact]
    public async Task Endpoints_WhenUnauthenticated_ReturnUnauthorized()
    {
        using var client = factory.CreateHttpsClient();

        var request = new IngredientRequest(
            Name: "Anonymous ingredient",
            CaloriesPer100g: 100m,
            ProteinPer100g: 10m,
            FatPer100g: 5m,
            CarbohydratesPer100g: 12m,
            SugarsPer100g: 3m,
            FiberPer100g: 2m,
            SaltPer100g: 0.5m);

        var listResponse = await client.GetAsync(
            "/api/ingredients");

        var detailsResponse = await client.GetAsync(
            "/api/ingredients/1");

        var createResponse = await client.PostWithCsrfAsync(
            "/api/ingredients",
            request);

        var updateResponse = await client.PutWithCsrfAsync(
            "/api/ingredients/1",
            request);

        var deleteResponse = await client.DeleteWithCsrfAsync(
            "/api/ingredients/1");

        Assert.All(
            new[]
            {
            listResponse,
            detailsResponse,
            createResponse,
            updateResponse,
            deleteResponse
            },
            response => Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode));
    }

    [Fact]
    public async Task Endpoints_WhenIngredientBelongsToAnotherUser_ReturnNotFound()
    {
        using var ownerClient = factory.CreateHttpsClient();
        using var otherUserClient = factory.CreateHttpsClient();

        await RegisterUserAsync(ownerClient);
        await RegisterUserAsync(otherUserClient);

        var createRequest = new IngredientRequest(
            Name: "Private ingredient",
            CaloriesPer100g: 100m,
            ProteinPer100g: 10m,
            FatPer100g: 5m,
            CarbohydratesPer100g: 12m,
            SugarsPer100g: 3m,
            FiberPer100g: 2m,
            SaltPer100g: 0.5m);

        var createResponse = await ownerClient.PostWithCsrfAsync(
            "/api/ingredients",
            createRequest);

        createResponse.EnsureSuccessStatusCode();

        var createdIngredient = await createResponse.Content
            .ReadFromJsonAsync<IngredientResponse>();

        Assert.NotNull(createdIngredient);

        var listResponse = await otherUserClient.GetAsync(
            "/api/ingredients");

        listResponse.EnsureSuccessStatusCode();

        var visibleIngredients = await listResponse.Content
            .ReadFromJsonAsync<IReadOnlyList<IngredientResponse>>();

        Assert.NotNull(visibleIngredients);
        Assert.DoesNotContain(
            visibleIngredients,
            ingredient => ingredient.Id == createdIngredient.Id);

        var detailsResponse = await otherUserClient.GetAsync(
            $"/api/ingredients/{createdIngredient.Id}");

        var updateResponse = await otherUserClient.PutWithCsrfAsync(
            $"/api/ingredients/{createdIngredient.Id}",
            createRequest);

        var deleteResponse = await otherUserClient.DeleteWithCsrfAsync(
            $"/api/ingredients/{createdIngredient.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            detailsResponse.StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            updateResponse.StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            deleteResponse.StatusCode);

        var ownerDetailsResponse = await ownerClient.GetAsync(
            $"/api/ingredients/{createdIngredient.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            ownerDetailsResponse.StatusCode);
    }

    private static async Task RegisterUserAsync(
        HttpClient client)
    {
        var request = new RegisterRequest(
            $"ingredient-user-{Guid.NewGuid():N}@example.com",
            "Test123!");

        var response = await client.PostWithCsrfAsync(
            "/api/auth/register",
            request);

        response.EnsureSuccessStatusCode();
    }
}