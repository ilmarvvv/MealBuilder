using MealBuilder.Api.Contracts.Ingredients;
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