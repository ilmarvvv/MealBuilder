using MealBuilder.Api.Contracts.Recipes;
using MealBuilder.Domain.Recipes;
using MealBuilder.Infrastructure.Data;
using MealBuilder.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/recipes/{recipeId:int}/ingredients")]
public sealed class RecipeIngredientsController(
    AppDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RecipeIngredientResponse>> Add(
        int recipeId,
        RecipeIngredientRequest request,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var recipe = await dbContext.Recipes
            .Where(recipe =>
                recipe.Id == recipeId &&
                recipe.OwnerId == userId)
            .Include(recipe => recipe.Ingredients)
            .SingleOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        if (recipe.Ingredients.Any(
            recipeIngredient =>
                recipeIngredient.IngredientId ==
                request.IngredientId))
        {
            ModelState.AddModelError(
                nameof(request.IngredientId),
                "The ingredient already exists in this recipe.");

            return ValidationProblem(ModelState);
        }

        var ingredient = await dbContext.Ingredients
            .FirstOrDefaultAsync(
                ingredient =>
                    ingredient.Id == request.IngredientId &&
                    (ingredient.OwnerId == null ||
                     ingredient.OwnerId == userId),
                cancellationToken);

        if (ingredient is null)
        {
            return NotFound();
        }

        var recipeIngredient = recipe.AddIngredient(
            ingredient,
            request.Grams);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(recipeIngredient));
    }

    [HttpPut("{ingredientId:int}")]
    public async Task<ActionResult<RecipeIngredientResponse>> UpdateGrams(
        int recipeId,
        int ingredientId,
        RecipeIngredientGramsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var recipe = await dbContext.Recipes
            .Where(recipe =>
                recipe.Id == recipeId &&
                recipe.OwnerId == userId)
            .Include(recipe => recipe.Ingredients)
                .ThenInclude(recipeIngredient =>
                    recipeIngredient.Ingredient)
            .SingleOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        var recipeIngredient = recipe.Ingredients
            .SingleOrDefault(recipeIngredient =>
                recipeIngredient.IngredientId == ingredientId);

        if (recipeIngredient is null)
        {
            return NotFound();
        }

        recipe.UpdateIngredient(
            ingredientId,
            request.Grams);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(recipeIngredient));
    }

    [HttpPut("{ingredientId:int}/position")]
    public async Task<IActionResult> Move(
    int recipeId,
    int ingredientId,
    RecipePositionRequest request,
    CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var recipe = await dbContext.Recipes
            .Where(recipe =>
                recipe.Id == recipeId &&
                recipe.OwnerId == userId)
            .Include(recipe => recipe.Ingredients)
            .SingleOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        var exists = recipe.Ingredients.Any(
            recipeIngredient =>
                recipeIngredient.IngredientId == ingredientId);

        if (!exists)
        {
            return NotFound();
        }

        if (request.Position > recipe.Ingredients.Count)
        {
            ModelState.AddModelError(
                nameof(request.Position),
                $"Position cannot exceed {recipe.Ingredients.Count}.");

            return ValidationProblem(ModelState);
        }

        recipe.MoveIngredient(
            ingredientId,
            request.Position);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{ingredientId:int}")]
    public async Task<IActionResult> Delete(
        int recipeId,
        int ingredientId,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var recipe = await dbContext.Recipes
            .Where(recipe =>
                recipe.Id == recipeId &&
                recipe.OwnerId == userId)
            .Include(recipe => recipe.Ingredients)
            .SingleOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        var exists = recipe.Ingredients.Any(
            recipeIngredient =>
                recipeIngredient.IngredientId == ingredientId);

        if (!exists)
        {
            return NotFound();
        }

        if (recipe.Ingredients.Count == 1)
        {
            ModelState.AddModelError(
                nameof(ingredientId),
                "A recipe must contain at least one ingredient.");

            return ValidationProblem(ModelState);
        }

        recipe.RemoveIngredient(ingredientId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static RecipeIngredientResponse ToResponse(
        RecipeIngredient recipeIngredient)
    {
        return new RecipeIngredientResponse(
            recipeIngredient.IngredientId,
            recipeIngredient.Ingredient.Name,
            recipeIngredient.Grams,
            recipeIngredient.Position);
    }
}