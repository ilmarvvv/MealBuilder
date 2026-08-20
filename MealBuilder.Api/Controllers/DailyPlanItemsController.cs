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
[Route("api/daily-plans/{dailyPlanId:int}/items")]
public sealed class DailyPlanItemsController(
    AppDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPut("{itemId:int}/amount")]
    public async Task<ActionResult<DailyPlanResponse>> ChangeAmount(
        int dailyPlanId,
        int itemId,
        DailyPlanItemAmountRequest request,
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

        var dailyPlan = await LoadDailyPlanAsync(
            dailyPlanId,
            userId,
            cancellationToken);

        if (dailyPlan is null)
        {
            return NotFound();
        }

        var item = dailyPlan.Items.SingleOrDefault(
            item => item.Id == itemId);

        if (item is null)
        {
            return NotFound();
        }

        try
        {
            switch (item.ItemType)
            {
                case DailyPlanItemType.Ingredient:
                    dailyPlan.ChangeIngredientAmount(
                        item.Id,
                        request.Amount);

                    break;

                case DailyPlanItemType.PreparedRecipe:
                    var preparedRecipe = item.PreparedRecipe
                        ?? throw new InvalidOperationException(
                            "The Prepared Recipe must be loaded.");

                    var currentPortions = item.Portions
                        ?? throw new InvalidOperationException(
                            "The Prepared Recipe item must contain portions.");

                    var allocatedPortions =
                        await GetAllocatedPortionsAsync(
                            preparedRecipe.Id,
                            cancellationToken);

                    PreparedRecipePortionCalculator
                        .EnsureCanChangeAllocation(
                            preparedRecipe,
                            allocatedPortions,
                            currentPortions,
                            request.Amount);

                    dailyPlan.ChangePreparedRecipeAmount(
                        item.Id,
                        request.Amount);

                    break;

                default:
                    throw new InvalidOperationException(
                        "The Daily Plan item type is not supported.");
            }

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

    [HttpPut("{itemId:int}/time")]
    public async Task<ActionResult<DailyPlanResponse>>
        ChangePlannedTime(
            int dailyPlanId,
            int itemId,
            DailyPlanItemTimeRequest request,
            CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var dailyPlan = await LoadDailyPlanAsync(
            dailyPlanId,
            userId,
            cancellationToken);

        if (dailyPlan is null)
        {
            return NotFound();
        }

        var itemExists = dailyPlan.Items.Any(
            item => item.Id == itemId);

        if (!itemExists)
        {
            return NotFound();
        }

        try
        {
            dailyPlan.ChangePlannedTime(
                itemId,
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

    [HttpDelete("{itemId:int}")]
    public async Task<ActionResult<DailyPlanResponse>> Remove(
        int dailyPlanId,
        int itemId,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var dailyPlan = await LoadDailyPlanAsync(
            dailyPlanId,
            userId,
            cancellationToken);

        if (dailyPlan is null)
        {
            return NotFound();
        }

        var itemExists = dailyPlan.Items.Any(
            item => item.Id == itemId);

        if (!itemExists)
        {
            return NotFound();
        }

        var date = dailyPlan.Date;

        dailyPlan.RemoveItem(itemId);

        if (dailyPlan.IsEmpty)
        {
            dbContext.DailyPlans.Remove(dailyPlan);
        }
        else
        {
            dailyPlan.EnsureCanBeSaved();
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        if (dailyPlan.IsEmpty)
        {
            return Ok(
                DailyPlanResponseMapper.ToEmptyResponse(
                    date));
        }

        return Ok(
            DailyPlanResponseMapper.ToResponse(
                dailyPlan));
    }

    private async Task<DailyPlan?> LoadDailyPlanAsync(
        int dailyPlanId,
        string userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.DailyPlans
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