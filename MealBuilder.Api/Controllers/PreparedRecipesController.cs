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
[Route("api/prepared-recipes")]
public sealed class PreparedRecipesController(
    AppDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<PreparedRecipeSummaryResponse>>>
        GetAll(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var preparedRecipes = await dbContext.PreparedRecipes
            .AsNoTracking()
            .Where(preparedRecipe =>
                preparedRecipe.OwnerId == userId)
            .Include(preparedRecipe =>
                preparedRecipe.Ingredients)
            .OrderByDescending(preparedRecipe =>
                preparedRecipe.PreparedDate)
            .ThenBy(preparedRecipe =>
                preparedRecipe.NameSnapshot)
            .ThenBy(preparedRecipe =>
                preparedRecipe.Id)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        if (preparedRecipes.Count == 0)
        {
            return Ok(
                Array.Empty<PreparedRecipeSummaryResponse>());
        }

        var preparedRecipeIds = preparedRecipes
            .Select(preparedRecipe => preparedRecipe.Id)
            .ToArray();

        var allocatedPortionsById =
            await dbContext.DailyPlanItems
                .AsNoTracking()
                .Where(item =>
                    item.PreparedRecipeId.HasValue &&
                    preparedRecipeIds.Contains(
                        item.PreparedRecipeId.Value))
                .GroupBy(item =>
                    item.PreparedRecipeId!.Value)
                .Select(group => new
                {
                    PreparedRecipeId = group.Key,
                    AllocatedPortions = group.Sum(item =>
                        item.Portions ?? 0m)
                })
                .ToDictionaryAsync(
                    allocation =>
                        allocation.PreparedRecipeId,
                    allocation =>
                        allocation.AllocatedPortions,
                    cancellationToken);

        var response = preparedRecipes
            .Select(preparedRecipe =>
            {
                allocatedPortionsById.TryGetValue(
                    preparedRecipe.Id,
                    out var allocatedPortions);

                return PreparedRecipeResponseMapper
                    .ToSummaryResponse(
                        preparedRecipe,
                        allocatedPortions);
            })
            .ToArray();

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PreparedRecipeResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var preparedRecipe = await dbContext.PreparedRecipes
            .AsNoTracking()
            .Where(preparedRecipe =>
                preparedRecipe.Id == id &&
                preparedRecipe.OwnerId == userId)
            .Include(preparedRecipe =>
                preparedRecipe.Ingredients)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        if (preparedRecipe is null)
        {
            return NotFound();
        }

        var allocatedPortions =
            await GetAllocatedPortionsAsync(
                preparedRecipe.Id,
                cancellationToken);

        return Ok(
            PreparedRecipeResponseMapper.ToResponse(
                preparedRecipe,
                allocatedPortions));
    }

    [HttpGet("{id:int}/availability")]
    public async Task<
        ActionResult<PreparedRecipeAvailabilityResponse>>
        GetAvailability(
            int id,
            CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var preparedRecipe = await dbContext.PreparedRecipes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                preparedRecipe =>
                    preparedRecipe.Id == id &&
                    preparedRecipe.OwnerId == userId,
                cancellationToken);

        if (preparedRecipe is null)
        {
            return NotFound();
        }

        var allocatedPortions =
            await GetAllocatedPortionsAsync(
                preparedRecipe.Id,
                cancellationToken);

        return Ok(
            PreparedRecipeResponseMapper
                .ToAvailabilityResponse(
                    preparedRecipe,
                    allocatedPortions));
    }

    [HttpPost]
    public async Task<ActionResult<PreparedRecipeResponse>> Create(
        CreatePreparedRecipeRequest request,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var hasDuplicateDates = request.Allocations
            .GroupBy(allocation => allocation.Date)
            .Any(group => group.Count() > 1);

        if (hasDuplicateDates)
        {
            ModelState.AddModelError(
                nameof(request.Allocations),
                "Each allocation date can only appear once.");

            return ValidationProblem(ModelState);
        }

        if (request.Allocations.Any(allocation =>
                allocation.Date < request.PreparedDate))
        {
            ModelState.AddModelError(
                nameof(request.Allocations),
                "Allocations cannot be earlier than the prepared date.");

            return ValidationProblem(ModelState);
        }

        decimal requestedAllocatedPortions;

        try
        {
            requestedAllocatedPortions =
                request.Allocations.Sum(allocation =>
                    allocation.Portions);
        }
        catch (OverflowException)
        {
            ModelState.AddModelError(
                nameof(request.Allocations),
                "The total allocated portions are too large.");

            return ValidationProblem(ModelState);
        }

        if (requestedAllocatedPortions > request.TotalPortions)
        {
            ModelState.AddModelError(
                nameof(request.Allocations),
                "Allocated portions cannot exceed total portions.");

            return ValidationProblem(ModelState);
        }

        var sourceRecipe = await dbContext.Recipes
            .Where(recipe =>
                recipe.Id == request.RecipeId &&
                recipe.OwnerId == userId)
            .Include(recipe => recipe.Ingredients)
                .ThenInclude(recipeIngredient =>
                    recipeIngredient.Ingredient)
            .Include(recipe => recipe.Steps)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        if (sourceRecipe is null)
        {
            ModelState.AddModelError(
                nameof(request.RecipeId),
                "The source recipe was not found.");

            return ValidationProblem(ModelState);
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var preparedRecipe = PreparedRecipe.Create(
                userId,
                sourceRecipe,
                request.PreparedDate,
                request.TotalPortions);

            if (requestedAllocatedPortions > 0)
            {
                PreparedRecipePortionCalculator
                    .EnsureCanAllocate(
                        preparedRecipe,
                        0m,
                        requestedAllocatedPortions);
            }

            var allocationDates = request.Allocations
                .Select(allocation => allocation.Date)
                .ToArray();

            var dailyPlansByDate =
                allocationDates.Length == 0
                    ? new Dictionary<DateOnly, DailyPlan>()
                    : await dbContext.DailyPlans
                        .Where(dailyPlan =>
                            dailyPlan.OwnerId == userId &&
                            allocationDates.Contains(
                                dailyPlan.Date))
                        .Include(dailyPlan =>
                            dailyPlan.Items)
                        .ToDictionaryAsync(
                            dailyPlan => dailyPlan.Date,
                            cancellationToken);

            dbContext.PreparedRecipes.Add(preparedRecipe);

            foreach (var allocation in request.Allocations)
            {
                if (!dailyPlansByDate.TryGetValue(
                        allocation.Date,
                        out var dailyPlan))
                {
                    dailyPlan = DailyPlan.Create(
                        userId,
                        allocation.Date);

                    dailyPlansByDate.Add(
                        allocation.Date,
                        dailyPlan);

                    dbContext.DailyPlans.Add(dailyPlan);
                }

                dailyPlan.AddPreparedRecipe(
                    preparedRecipe,
                    allocation.Portions);

                dailyPlan.EnsureCanBeSaved();
            }

            await dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = preparedRecipe.Id },
                PreparedRecipeResponseMapper.ToResponse(
                    preparedRecipe,
                    requestedAllocatedPortions));
        }
        catch (Exception exception)
            when (exception is ArgumentException or
                  InvalidOperationException or
                  OverflowException)
        {
            ModelState.AddModelError(
                nameof(request),
                exception.Message);

            return ValidationProblem(ModelState);
        }
    }

    [HttpPost("planning-preview")]
    public async Task<
        ActionResult<IReadOnlyList<PreparedRecipeAllocationResponse>>>
        PreviewPlanning(
            PreparedRecipePlanningPreviewRequest request,
            CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var sourceRecipe = await dbContext.Recipes
            .AsNoTracking()
            .Where(recipe =>
                recipe.Id == request.RecipeId &&
                recipe.OwnerId == userId)
            .Include(recipe => recipe.Ingredients)
                .ThenInclude(recipeIngredient =>
                    recipeIngredient.Ingredient)
            .Include(recipe => recipe.Steps)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        if (sourceRecipe is null)
        {
            ModelState.AddModelError(
                nameof(request.RecipeId),
                "The source recipe was not found.");

            return ValidationProblem(ModelState);
        }

        try
        {
            var preparedRecipe = PreparedRecipe.Create(
                userId,
                sourceRecipe,
                request.PreparedDate,
                request.TotalPortions);

            var proposals =
                PreparedRecipePortionDistributor
                    .CreateDistribution(
                        preparedRecipe,
                        request.StartDate,
                        request.PlannedDays);

            var response = proposals
                .Select(proposal =>
                    new PreparedRecipeAllocationResponse(
                        proposal.Date,
                        proposal.Portions))
                .ToArray();

            return Ok(response);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(
                nameof(request),
                exception.Message);

            return ValidationProblem(ModelState);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(
                nameof(request),
                exception.Message);

            return ValidationProblem(ModelState);
        }
    }

    private async Task<decimal> GetAllocatedPortionsAsync(
        int preparedRecipeId,
        CancellationToken cancellationToken)
    {
        return await dbContext.DailyPlanItems
            .AsNoTracking()
            .Where(item =>
                item.PreparedRecipeId == preparedRecipeId)
            .SumAsync(
                item => item.Portions ?? 0m,
                cancellationToken);
    }
}