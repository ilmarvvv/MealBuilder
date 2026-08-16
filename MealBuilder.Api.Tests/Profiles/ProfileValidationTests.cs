using System.Net;
using MealBuilder.Api.Contracts.Authentication;
using MealBuilder.Api.Contracts.Profiles;
using MealBuilder.Api.Tests.Infrastructure;
using MealBuilder.Domain.Profiles;

namespace MealBuilder.Api.Tests.Profiles;

public sealed class ProfileValidationTests(
    MealBuilderApiFactory factory)
    : IClassFixture<MealBuilderApiFactory>
{
    [Fact]
    public async Task SaveDailyCalorieTarget_BelowSupportedRange_ReturnsBadRequest()
    {
        using var client = factory.CreateHttpsClient();

        await RegisterUserAsync(client);

        var request = new DailyCalorieTargetRequest(
            DailyCalorieTarget: 999);

        var response = await client.PutWithCsrfAsync(
            "/api/profile/calorie-target",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task CalculateTarget_ForUserUnder18_ReturnsBadRequest()
    {
        using var client = factory.CreateHttpsClient();

        await RegisterUserAsync(client);

        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var request = new CalorieTargetCalculationRequest(
            BirthDate: currentDate.AddYears(-17),
            SexForCalculation: CalculationSex.Male,
            HeightCm: 180m,
            WeightKg: 80m,
            ActivityLevel: ActivityLevel.ModeratelyActive,
            WeightGoal: WeightGoal.MaintainWeight);

        var response = await client.PostWithCsrfAsync(
            "/api/profile/calorie-target/calculate",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    private static async Task RegisterUserAsync(
        HttpClient client)
    {
        var request = new RegisterRequest(
            $"profile-validation-{Guid.NewGuid():N}@example.com",
            "Test123!");

        var response = await client.PostWithCsrfAsync(
            "/api/auth/register",
            request);

        response.EnsureSuccessStatusCode();
    }
}