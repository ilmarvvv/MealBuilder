using MealBuilder.Api.Contracts.Recipes;
using MealBuilder.Api.Mappings;
using MealBuilder.Domain.Ingredients;
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
[Route("api/recipes")]
public sealed class RecipesController(
    AppDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<RecipeSummaryResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var recipes = await dbContext.Recipes
            .AsNoTracking()
            .Where(recipe => recipe.OwnerId == userId)
            .Include(recipe => recipe.Ingredients)
                .ThenInclude(recipeIngredient =>
                    recipeIngredient.Ingredient)
            .OrderBy(recipe => recipe.Name)
            .ThenBy(recipe => recipe.Id)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return Ok(recipes
            .Select(RecipeResponseMapper.ToSummaryResponse)
            .ToArray());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecipeResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var recipe = await dbContext.Recipes
            .AsNoTracking()
            .Where(recipe =>
                recipe.Id == id &&
                recipe.OwnerId == userId)
            .Include(recipe => recipe.Ingredients)
                .ThenInclude(recipeIngredient =>
                    recipeIngredient.Ingredient)
            .Include(recipe => recipe.Steps)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        return Ok(RecipeResponseMapper.ToResponse(recipe));
    }

    [HttpPost]
    public async Task<ActionResult<RecipeResponse>> Create(
        RecipeRequest request,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var ingredientsById = await LoadAccessibleIngredientsAsync(
            request,
            userId,
            cancellationToken);

        if (ingredientsById is null)
        {
            return ValidationProblem(ModelState);
        }

        var recipe = Recipe.Create(
            userId,
            request.Name,
            request.Description,
            request.Servings);

        foreach (var requestedIngredient in request.Ingredients)
        {
            recipe.AddIngredient(
                ingredientsById[requestedIngredient.IngredientId],
                requestedIngredient.Grams);
        }

        foreach (var requestedStep in request.Steps)
        {
            recipe.AddStep(requestedStep.Instruction);
        }

        recipe.EnsureCanBeSaved();

        dbContext.Recipes.Add(recipe);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = RecipeResponseMapper.ToResponse(recipe);

        return CreatedAtAction(
            nameof(GetById),
            new { id = recipe.Id },
            response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RecipeResponse>> Update(
    int id,
    RecipeRequest request,
    CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized();
        }

        var recipe = await dbContext.Recipes
            .Where(recipe =>
                recipe.Id == id &&
                recipe.OwnerId == userId)
            .Include(recipe => recipe.Ingredients)
                .ThenInclude(recipeIngredient =>
                    recipeIngredient.Ingredient)
            .Include(recipe => recipe.Steps)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        var ingredientsById = await LoadAccessibleIngredientsAsync(
            request,
            userId,
            cancellationToken);

        if (ingredientsById is null)
        {
            return ValidationProblem(ModelState);
        }

        recipe.UpdateDetails(
            request.Name,
            request.Description,
            request.Servings);

        SynchronizeIngredients(
            recipe,
            request,
            ingredientsById);

        SynchronizeSteps(
            recipe,
            request);

        recipe.EnsureCanBeSaved();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(RecipeResponseMapper.ToResponse(recipe));
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

        var recipe = await dbContext.Recipes
            .FirstOrDefaultAsync(
                recipe =>
                    recipe.Id == id &&
                    recipe.OwnerId == userId,
                cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        dbContext.Recipes.Remove(recipe);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static void SynchronizeIngredients(
    Recipe recipe,
    RecipeRequest request,
    IReadOnlyDictionary<int, Ingredient> ingredientsById)
    {
        var requestedIngredientIds = request.Ingredients
            .Select(ingredient => ingredient.IngredientId)
            .ToHashSet();

        foreach (var requestedIngredient in request.Ingredients)
        {
            var alreadyExists = recipe.Ingredients.Any(
                recipeIngredient =>
                    recipeIngredient.IngredientId ==
                    requestedIngredient.IngredientId);

            if (!alreadyExists)
            {
                recipe.AddIngredient(
                    ingredientsById[requestedIngredient.IngredientId],
                    requestedIngredient.Grams);
            }
        }

        var ingredientsToRemove = recipe.Ingredients
            .Where(recipeIngredient =>
                !requestedIngredientIds.Contains(
                    recipeIngredient.IngredientId))
            .ToArray();

        foreach (var recipeIngredient in ingredientsToRemove)
        {
            recipe.RemoveIngredient(
                recipeIngredient.IngredientId);
        }

        for (var index = 0;
             index < request.Ingredients.Length;
             index++)
        {
            var requestedIngredient = request.Ingredients[index];

            recipe.UpdateIngredient(
                requestedIngredient.IngredientId,
                requestedIngredient.Grams);

            recipe.MoveIngredient(
                requestedIngredient.IngredientId,
                index + 1);
        }
    }

    private static void SynchronizeSteps(
        Recipe recipe,
        RecipeRequest request)
    {
        var existingSteps = recipe.Steps
            .OrderBy(step => step.Position)
            .ToArray();

        var sharedStepCount = Math.Min(
            existingSteps.Length,
            request.Steps.Length);

        for (var index = 0;
             index < sharedStepCount;
             index++)
        {
            recipe.UpdateStep(
                existingSteps[index].Id,
                request.Steps[index].Instruction);
        }

        for (var index = sharedStepCount;
             index < request.Steps.Length;
             index++)
        {
            recipe.AddStep(
                request.Steps[index].Instruction);
        }

        for (var index = existingSteps.Length - 1;
             index >= request.Steps.Length;
             index--)
        {
            recipe.RemoveStep(existingSteps[index].Id);
        }
    }

    private async Task<Dictionary<int, Ingredient>?>
    LoadAccessibleIngredientsAsync(
        RecipeRequest request,
        string userId,
        CancellationToken cancellationToken)
    {
        var ingredientIds = request.Ingredients
            .Select(ingredient => ingredient.IngredientId)
            .ToArray();

        if (ingredientIds.Distinct().Count() != ingredientIds.Length)
        {
            ModelState.AddModelError(
                nameof(request.Ingredients),
                "An ingredient can only be added once.");

            return null;
        }

        var ingredientsById = await dbContext.Ingredients
            .Where(ingredient =>
                ingredientIds.Contains(ingredient.Id) &&
                (ingredient.OwnerId == null ||
                 ingredient.OwnerId == userId))
            .ToDictionaryAsync(
                ingredient => ingredient.Id,
                cancellationToken);

        if (ingredientsById.Count != ingredientIds.Length)
        {
            ModelState.AddModelError(
                nameof(request.Ingredients),
                "One or more ingredients were not found.");

            return null;
        }

        return ingredientsById;
    }
}