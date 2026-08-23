using MealBuilder.Api.Contracts.MealPlanning;
using MealBuilder.Api.Mappings;
using MealBuilder.Domain.MealPlanning;
using MealBuilder.Infrastructure.Data;
using MealBuilder.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/daily-plans")]
public sealed class DailyPlansController(
    AppDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("{date}")]
    public async Task<ActionResult<DailyPlanResponse>> GetByDate(
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var dailyPlan = await dbContext.DailyPlans
            .AsNoTracking()
            .Where(dailyPlan =>
                dailyPlan.OwnerId == userId &&
                dailyPlan.Date == date)
            .Include(dailyPlan => dailyPlan.Items)
                .ThenInclude(item => item.Ingredient)
            .Include(dailyPlan => dailyPlan.Items)
                .ThenInclude(item => item.PreparedRecipe)
                    .ThenInclude(preparedRecipe =>
                        preparedRecipe!.Ingredients)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        if (dailyPlan is null)
        {
            return Ok(
                DailyPlanResponseMapper.ToEmptyResponse(
                    date));
        }

        return Ok(
            DailyPlanResponseMapper.ToResponse(
                dailyPlan));
    }

    [HttpGet("week/{startDate}")]
    public async Task<ActionResult<WeeklySummaryResponse>> GetWeek(
        DateOnly startDate,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        if (startDate.DayOfWeek != DayOfWeek.Monday)
        {
            ModelState.AddModelError(
                nameof(startDate),
                "The week start date must be a Monday.");

            return ValidationProblem(ModelState);
        }

        if (startDate.DayNumber >
            DateOnly.MaxValue.DayNumber - 6)
        {
            ModelState.AddModelError(
                nameof(startDate),
                "The requested week exceeds the supported calendar.");

            return ValidationProblem(ModelState);
        }

        var endDate = startDate.AddDays(6);

        var dailyPlans = await dbContext.DailyPlans
            .AsNoTracking()
            .Where(dailyPlan =>
                dailyPlan.OwnerId == userId &&
                dailyPlan.Date >= startDate &&
                dailyPlan.Date <= endDate)
            .Include(dailyPlan => dailyPlan.Items)
                .ThenInclude(item => item.Ingredient)
            .Include(dailyPlan => dailyPlan.Items)
                .ThenInclude(item => item.PreparedRecipe)
                    .ThenInclude(preparedRecipe =>
                        preparedRecipe!.Ingredients)
            .OrderBy(dailyPlan => dailyPlan.Date)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return Ok(
            WeeklySummaryResponseMapper.ToResponse(
                startDate,
                dailyPlans));
    }

    [HttpPost("{date}/ingredients")]
    public async Task<ActionResult<DailyPlanResponse>> AddIngredient(
        DateOnly date,
        AddDailyPlanIngredientRequest request,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var ingredient = await dbContext.Ingredients
            .SingleOrDefaultAsync(
                ingredient =>
                    ingredient.Id == request.IngredientId &&
                    (ingredient.OwnerId == null ||
                     ingredient.OwnerId == userId),
                cancellationToken);

        if (ingredient is null)
        {
            ModelState.AddModelError(
                nameof(request.IngredientId),
                "The ingredient was not found.");

            return ValidationProblem(ModelState);
        }

        var dailyPlan = await dbContext.DailyPlans
            .Where(dailyPlan =>
                dailyPlan.OwnerId == userId &&
                dailyPlan.Date == date)
            .Include(dailyPlan => dailyPlan.Items)
                .ThenInclude(item => item.Ingredient)
            .Include(dailyPlan => dailyPlan.Items)
                .ThenInclude(item => item.PreparedRecipe)
                    .ThenInclude(preparedRecipe =>
                        preparedRecipe!.Ingredients)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        if (dailyPlan is null)
        {
            dailyPlan = DailyPlan.Create(
                userId,
                date);

            dbContext.DailyPlans.Add(dailyPlan);
        }

        try
        {
            dailyPlan.AddIngredient(
                ingredient,
                request.Grams,
                request.PlannedTime);

            dailyPlan.EnsureCanBeSaved();

            await dbContext.SaveChangesAsync(
                cancellationToken);

            return Ok(
                DailyPlanResponseMapper.ToResponse(
                    dailyPlan));
        }
        catch (Exception exception)
            when (exception is ArgumentException or
                  InvalidOperationException)
        {
            ModelState.AddModelError(
                nameof(request),
                exception.Message);

            return ValidationProblem(ModelState);
        }
    }

    [HttpPost("{date}/prepared-recipes")]
    public async Task<ActionResult<DailyPlanResponse>>
        AddPreparedRecipe(
            DateOnly date,
            AddDailyPlanPreparedRecipeRequest request,
            CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var preparedRecipe = await dbContext.PreparedRecipes
            .Where(preparedRecipe =>
                preparedRecipe.Id ==
                    request.PreparedRecipeId &&
                preparedRecipe.OwnerId == userId)
            .Include(preparedRecipe =>
                preparedRecipe.Ingredients)
            .SingleOrDefaultAsync(cancellationToken);

        if (preparedRecipe is null)
        {
            ModelState.AddModelError(
                nameof(request.PreparedRecipeId),
                "The Prepared Recipe was not found.");

            return ValidationProblem(ModelState);
        }

        var allocatedPortions =
            await GetAllocatedPortionsAsync(
                preparedRecipe.Id,
                cancellationToken);

        try
        {
            PreparedRecipePortionCalculator
                .EnsureCanAllocate(
                    preparedRecipe,
                    allocatedPortions,
                    request.Portions);

            var dailyPlan = await dbContext.DailyPlans
                .Where(dailyPlan =>
                    dailyPlan.OwnerId == userId &&
                    dailyPlan.Date == date)
                .Include(dailyPlan => dailyPlan.Items)
                    .ThenInclude(item => item.Ingredient)
                .Include(dailyPlan => dailyPlan.Items)
                    .ThenInclude(item =>
                        item.PreparedRecipe)
                        .ThenInclude(itemPreparedRecipe =>
                            itemPreparedRecipe!.Ingredients)
                .AsSplitQuery()
                .SingleOrDefaultAsync(cancellationToken);

            if (dailyPlan is null)
            {
                dailyPlan = DailyPlan.Create(
                    userId,
                    date);

                dbContext.DailyPlans.Add(dailyPlan);
            }

            dailyPlan.AddPreparedRecipe(
                preparedRecipe,
                request.Portions,
                request.PlannedTime);

            dailyPlan.EnsureCanBeSaved();

            await dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Ok(
                DailyPlanResponseMapper.ToResponse(
                    dailyPlan));
        }
        catch (Exception exception)
            when (exception is ArgumentException or
                  InvalidOperationException)
        {
            ModelState.AddModelError(
                nameof(request),
                exception.Message);

            return ValidationProblem(ModelState);
        }
    }

    [HttpPut("{dailyPlanId:int}/weekly-summary")]
    public async Task<ActionResult<DailyPlanResponse>>
        SetWeeklySummaryInclusion(
            int dailyPlanId,
            DailyPlanInclusionRequest request,
            CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var dailyPlan = await dbContext.DailyPlans
            .Where(dailyPlan =>
                dailyPlan.Id == dailyPlanId &&
                dailyPlan.OwnerId == userId)
            .Include(dailyPlan => dailyPlan.Items)
                .ThenInclude(item => item.Ingredient)
            .Include(dailyPlan => dailyPlan.Items)
                .ThenInclude(item => item.PreparedRecipe)
                    .ThenInclude(preparedRecipe =>
                        preparedRecipe!.Ingredients)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        if (dailyPlan is null)
        {
            return NotFound();
        }

        dailyPlan.SetWeeklySummaryInclusion(
            request.IncludeInWeeklySummary);

        dailyPlan.EnsureCanBeSaved();

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Ok(
            DailyPlanResponseMapper.ToResponse(
                dailyPlan));
    }

    private async Task<decimal> GetAllocatedPortionsAsync(
        int preparedRecipeId,
        CancellationToken cancellationToken)
    {
        return await dbContext.DailyPlanItems
            .AsNoTracking()
            .Where(item =>
                item.PreparedRecipeId ==
                preparedRecipeId)
            .SumAsync(
                item => item.Portions ?? 0m,
                cancellationToken);
    }
}