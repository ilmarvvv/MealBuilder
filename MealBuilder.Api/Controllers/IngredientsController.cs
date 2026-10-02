using MealBuilder.Api.Contracts.Ingredients;
using MealBuilder.Api.Contracts.Common;
using MealBuilder.Domain.Ingredients;
using MealBuilder.Infrastructure.Data;
using MealBuilder.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/ingredients")]
public sealed class IngredientsController(
    AppDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IngredientResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var ingredients = await dbContext.Ingredients
            .AsNoTracking()
            .Where(ingredient =>
                ingredient.OwnerId == null ||
                ingredient.OwnerId == userId)
            .OrderBy(ingredient => ingredient.Name)
            .ThenBy(ingredient => ingredient.Id)
            .ToListAsync(cancellationToken);

        return Ok(ingredients
            .Select(ToResponse)
            .ToArray());
    }

    [HttpGet("page")]
    public async Task<ActionResult<PagedResponse<IngredientResponse>>> GetPage(
    [FromQuery] PaginationQuery pagination,
    CancellationToken cancellationToken,
    [FromQuery] IngredientOwnershipFilter ownership =
        IngredientOwnershipFilter.All)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        if (!Enum.IsDefined(ownership))
        {
            ModelState.AddModelError(
                nameof(ownership),
                "The selected ownership filter is invalid.");

            return ValidationProblem(ModelState);
        }

        var normalizedSearch = pagination.Search?.Trim();

        var query = dbContext.Ingredients
            .AsNoTracking()
            .Where(ingredient =>
                ingredient.OwnerId == null ||
                ingredient.OwnerId == userId);

        query = ownership switch
        {
            IngredientOwnershipFilter.BuiltIn =>
                query.Where(ingredient => ingredient.OwnerId == null),

            IngredientOwnershipFilter.Mine =>
                query.Where(ingredient => ingredient.OwnerId == userId),

            _ => query
        };

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(ingredient =>
                EF.Functions.Like(
                    ingredient.Name,
                    $"%{normalizedSearch}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var ingredients = await query
            .OrderBy(ingredient => ingredient.Name)
            .ThenBy(ingredient => ingredient.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(
                totalCount / (double)pagination.PageSize);

        return Ok(new PagedResponse<IngredientResponse>(
            ingredients
                .Select(ToResponse)
                .ToArray(),
            pagination.Page,
            pagination.PageSize,
            totalCount,
            totalPages));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IngredientResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var ingredient = await dbContext.Ingredients
            .AsNoTracking()
            .FirstOrDefaultAsync(
                ingredient =>
                    ingredient.Id == id &&
                    (ingredient.OwnerId == null ||
                     ingredient.OwnerId == userId),
                cancellationToken);

        if (ingredient is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(ingredient));
    }

    [HttpPost]
    public async Task<ActionResult<IngredientResponse>> Create(
        IngredientRequest request,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var ingredient = Ingredient.CreateUserCreated(
            userId,
            request.Name,
            request.CaloriesPer100g,
            request.ProteinPer100g,
            request.FatPer100g,
            request.CarbohydratesPer100g,
            request.SugarsPer100g,
            request.FiberPer100g,
            request.SaltPer100g);

        dbContext.Ingredients.Add(ingredient);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = ToResponse(ingredient);

        return CreatedAtAction(
            nameof(GetById),
            new { id = ingredient.Id },
            response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<IngredientResponse>> Update(
    int id,
        IngredientRequest request,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var ingredient = await dbContext.Ingredients
            .FirstOrDefaultAsync(
                ingredient =>
                    ingredient.Id == id &&
                    (ingredient.OwnerId == null ||
                     ingredient.OwnerId == userId),
                cancellationToken);

        if (ingredient is null)
        {
            return NotFound();
        }

        if (ingredient.IsBuiltIn)
        {
            return Forbid();
        }

        ingredient.UpdateUserCreated(
            request.Name,
            request.CaloriesPer100g,
            request.ProteinPer100g,
            request.FatPer100g,
            request.CarbohydratesPer100g,
            request.SugarsPer100g,
            request.FiberPer100g,
            request.SaltPer100g);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(ingredient));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var ingredient = await dbContext.Ingredients
            .FirstOrDefaultAsync(
                ingredient =>
                    ingredient.Id == id &&
                    (ingredient.OwnerId == null ||
                     ingredient.OwnerId == userId),
                cancellationToken);

        if (ingredient is null)
        {
            return NotFound();
        }

        if (ingredient.IsBuiltIn)
        {
            return Forbid();
        }

        var isUsedByRecipe = await dbContext.RecipeIngredients
            .AsNoTracking()
            .AnyAsync(
        recipeIngredient =>
            recipeIngredient.IngredientId == ingredient.Id,
        cancellationToken);

        if (isUsedByRecipe)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Ingredient is in use.",
                detail:
                    "An ingredient used by a recipe cannot be deleted.");
        }

        dbContext.Ingredients.Remove(ingredient);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static IngredientResponse ToResponse(
        Ingredient ingredient)
    {
        return new IngredientResponse(
            ingredient.Id,
            ingredient.Name,
            ingredient.CaloriesPer100g,
            ingredient.ProteinPer100g,
            ingredient.FatPer100g,
            ingredient.CarbohydratesPer100g,
            ingredient.SugarsPer100g,
            ingredient.FiberPer100g,
            ingredient.SaltPer100g,
            ingredient.IsBuiltIn,
            ingredient.SourceName,
            ingredient.SourceCode,
            ingredient.SourceVersion);
    }
}