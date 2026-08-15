using MealBuilder.Api.Contracts.Profiles;
using MealBuilder.Domain.Profiles;
using MealBuilder.Infrastructure.Data;
using MealBuilder.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public sealed class ProfileController(
    AppDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserNutritionProfileResponse>> GetCurrent(
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var profile = await dbContext.UserNutritionProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                profile => profile.UserId == userId,
                cancellationToken);

        if (profile is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(profile));
    }

    [HttpPost("calorie-target/calculate")]
    public ActionResult<CalorieTargetEstimateResponse> CalculateTarget(
        CalorieTargetCalculationRequest request)
    {
        try
        {
            var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var estimate = CalorieTargetCalculator.Calculate(
                request.BirthDate!.Value,
                request.SexForCalculation!.Value,
                request.HeightCm!.Value,
                request.WeightKg!.Value,
                request.ActivityLevel!.Value,
                request.WeightGoal!.Value,
                currentDate);

            return Ok(new CalorieTargetEstimateResponse(
                estimate.Age,
                estimate.RestingEnergyExpenditure,
                estimate.MaintenanceCalories,
                estimate.RecommendedDailyCalorieTarget));
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidOperationException)
        {
            return ProfileValidationProblem(exception);
        }
    }

    [HttpPut("calorie-target")]
    public async Task<ActionResult<UserNutritionProfileResponse>>
    SaveDailyCalorieTarget(
        DailyCalorieTargetRequest request,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var profile = await dbContext.UserNutritionProfiles
            .SingleOrDefaultAsync(
                profile => profile.UserId == userId,
                cancellationToken);

        if (profile is null)
        {
            profile = UserNutritionProfile.CreateManual(
                userId,
                request.DailyCalorieTarget);

            dbContext.UserNutritionProfiles.Add(profile);
        }
        else
        {
            profile.ConfirmDailyCalorieTarget(
                request.DailyCalorieTarget);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(profile));
    }

    [HttpPut("calculated")]
    public async Task<ActionResult<UserNutritionProfileResponse>>
    SaveCalculatedProfile(
        CalculatedProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var inputs = request.CalculationInputs!;
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        try
        {
            var profile = await dbContext.UserNutritionProfiles
                .SingleOrDefaultAsync(
                    profile => profile.UserId == userId,
                    cancellationToken);

            if (profile is null)
            {
                profile = UserNutritionProfile.CreateCalculated(
                    userId,
                    request.DailyCalorieTarget,
                    inputs.BirthDate!.Value,
                    inputs.SexForCalculation!.Value,
                    inputs.HeightCm!.Value,
                    inputs.WeightKg!.Value,
                    inputs.ActivityLevel!.Value,
                    inputs.WeightGoal!.Value,
                    currentDate);

                dbContext.UserNutritionProfiles.Add(profile);
            }
            else
            {
                profile.UpdateCalculationInputs(
                    inputs.BirthDate!.Value,
                    inputs.SexForCalculation!.Value,
                    inputs.HeightCm!.Value,
                    inputs.WeightKg!.Value,
                    inputs.ActivityLevel!.Value,
                    inputs.WeightGoal!.Value,
                    currentDate);

                profile.ConfirmDailyCalorieTarget(
                    request.DailyCalorieTarget);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ToResponse(profile));
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidOperationException)
        {
            return ProfileValidationProblem(exception);
        }
    }

    [HttpPut("calculation-inputs")]
    public async Task<ActionResult<UserNutritionProfileResponse>>
    UpdateCalculationInputs(
        CalorieTargetCalculationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var profile = await dbContext.UserNutritionProfiles
            .SingleOrDefaultAsync(
                profile => profile.UserId == userId,
                cancellationToken);

        if (profile is null)
        {
            return NotFound();
        }

        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        try
        {
            profile.UpdateCalculationInputs(
                request.BirthDate!.Value,
                request.SexForCalculation!.Value,
                request.HeightCm!.Value,
                request.WeightKg!.Value,
                request.ActivityLevel!.Value,
                request.WeightGoal!.Value,
                currentDate);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ToResponse(profile));
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidOperationException)
        {
            return ProfileValidationProblem(exception);
        }
    }

    private static UserNutritionProfileResponse ToResponse(
        UserNutritionProfile profile)
    {
        return new UserNutritionProfileResponse(
            profile.DailyCalorieTarget,
            profile.BirthDate,
            profile.SexForCalculation,
            profile.HeightCm,
            profile.WeightKg,
            profile.ActivityLevel,
            profile.WeightGoal,
            profile.HasCalculationInputs);
    }

    private ActionResult ProfileValidationProblem(Exception exception)
    {
        var key = exception is ArgumentException argumentException
            ? argumentException.ParamName ?? string.Empty
            : string.Empty;

        ModelState.AddModelError(key, exception.Message);

        return ValidationProblem(ModelState);
    }
}