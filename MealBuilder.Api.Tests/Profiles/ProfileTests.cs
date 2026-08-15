using MealBuilder.Api.Contracts.Authentication;
using MealBuilder.Api.Contracts.Profiles;
using MealBuilder.Api.Tests.Infrastructure;
using MealBuilder.Domain.Profiles;
using System.Net;
using System.Net.Http.Json;

namespace MealBuilder.Api.Tests.Profiles;

public sealed class ProfileTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task GetCurrent_WhenProfileDoesNotExist_ReturnsNotFound()
    {
        using var client = factory.CreateHttpsClient();

        await RegisterUserAsync(client);

        var response = await client.GetAsync(
            "/api/profile");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task SaveDailyCalorieTarget_WithValidTarget_CreatesManualProfile()
    {
        using var client = factory.CreateHttpsClient();

        await RegisterUserAsync(client);

        var request = new DailyCalorieTargetRequest(
            DailyCalorieTarget: 2200);

        var response = await client.PutWithCsrfAsync(
            "/api/profile/calorie-target",
            request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var savedProfile = await response.Content
            .ReadFromJsonAsync<UserNutritionProfileResponse>();

        Assert.NotNull(savedProfile);
        Assert.Equal(
            request.DailyCalorieTarget,
            savedProfile.DailyCalorieTarget);
        Assert.False(savedProfile.HasCalculationInputs);

        var persistedProfile = await client
            .GetFromJsonAsync<UserNutritionProfileResponse>(
                "/api/profile");

        Assert.NotNull(persistedProfile);
        Assert.Equal(
            request.DailyCalorieTarget,
            persistedProfile.DailyCalorieTarget);
        Assert.False(persistedProfile.HasCalculationInputs);
    }

    [Fact]
    public async Task CalculateTarget_WithValidInputs_ReturnsEstimateWithoutSavingProfile()
    {
        using var client = factory.CreateHttpsClient();

        await RegisterUserAsync(client);

        var request = CreateValidCalculationRequest();

        var response = await client.PostWithCsrfAsync(
            "/api/profile/calorie-target/calculate",
            request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var estimate = await response.Content
            .ReadFromJsonAsync<CalorieTargetEstimateResponse>();

        Assert.NotNull(estimate);
        Assert.Equal(30, estimate.Age);
        Assert.Equal(
            1780,
            estimate.RestingEnergyExpenditure);
        Assert.Equal(
            2848,
            estimate.MaintenanceCalories);
        Assert.Equal(
            2848,
            estimate.RecommendedDailyCalorieTarget);

        var profileResponse = await client.GetAsync(
            "/api/profile");

        Assert.Equal(
            HttpStatusCode.NotFound,
            profileResponse.StatusCode);
    }

    [Fact]
    public async Task SaveCalculatedProfile_WithValidRequest_CompletesOnboarding()
    {
        using var client = factory.CreateHttpsClient();

        await RegisterUserAsync(client);

        var calculationInputs =
            CreateValidCalculationRequest();

        var request = new CalculatedProfileRequest(
            DailyCalorieTarget: 2800,
            CalculationInputs: calculationInputs);

        var response = await client.PutWithCsrfAsync(
            "/api/profile/calculated",
            request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var savedProfile = await response.Content
            .ReadFromJsonAsync<UserNutritionProfileResponse>();

        Assert.NotNull(savedProfile);
        Assert.Equal(
            request.DailyCalorieTarget,
            savedProfile.DailyCalorieTarget);
        Assert.Equal(
            calculationInputs.BirthDate,
            savedProfile.BirthDate);
        Assert.Equal(
            calculationInputs.SexForCalculation,
            savedProfile.SexForCalculation);
        Assert.Equal(
            calculationInputs.HeightCm,
            savedProfile.HeightCm);
        Assert.Equal(
            calculationInputs.WeightKg,
            savedProfile.WeightKg);
        Assert.Equal(
            calculationInputs.ActivityLevel,
            savedProfile.ActivityLevel);
        Assert.Equal(
            calculationInputs.WeightGoal,
            savedProfile.WeightGoal);
        Assert.True(savedProfile.HasCalculationInputs);

        var currentUser = await client
            .GetFromJsonAsync<AuthUserResponse>(
                "/api/auth/me");

        Assert.NotNull(currentUser);
        Assert.True(currentUser.IsOnboardingComplete);
    }

    [Fact]
    public async Task UpdateCalculationInputs_WhenProfileExists_KeepsSavedTarget()
    {
        using var client = factory.CreateHttpsClient();

        await RegisterUserAsync(client);

        const int savedTarget = 2200;

        var targetResponse = await client.PutWithCsrfAsync(
            "/api/profile/calorie-target",
            new DailyCalorieTargetRequest(savedTarget));

        targetResponse.EnsureSuccessStatusCode();

        var calculationInputs =
            CreateValidCalculationRequest();

        var updateResponse = await client.PutWithCsrfAsync(
            "/api/profile/calculation-inputs",
            calculationInputs);

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode);

        var updatedProfile = await updateResponse.Content
            .ReadFromJsonAsync<UserNutritionProfileResponse>();

        Assert.NotNull(updatedProfile);
        Assert.Equal(
            savedTarget,
            updatedProfile.DailyCalorieTarget);
        Assert.Equal(
            calculationInputs.WeightKg,
            updatedProfile.WeightKg);
        Assert.Equal(
            calculationInputs.ActivityLevel,
            updatedProfile.ActivityLevel);
        Assert.Equal(
            calculationInputs.WeightGoal,
            updatedProfile.WeightGoal);
        Assert.True(updatedProfile.HasCalculationInputs);
    }

    private static CalorieTargetCalculationRequest
    CreateValidCalculationRequest()
    {
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        return new CalorieTargetCalculationRequest(
            BirthDate: currentDate.AddYears(-30),
            SexForCalculation: CalculationSex.Male,
            HeightCm: 180m,
            WeightKg: 80m,
            ActivityLevel: ActivityLevel.ModeratelyActive,
            WeightGoal: WeightGoal.MaintainWeight);
    }

    private static async Task RegisterUserAsync(
        HttpClient client)
    {
        var request = new RegisterRequest(
            $"profile-user-{Guid.NewGuid():N}@example.com",
            "Test123!");

        var response = await client.PostWithCsrfAsync(
            "/api/auth/register",
            request);

        response.EnsureSuccessStatusCode();
    }
}