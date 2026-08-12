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
[Route("api/recipes/{recipeId:int}/steps")]
public sealed class RecipeStepsController(
    AppDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RecipeStepResponse>> Add(
        int recipeId,
        RecipeStepRequest request,
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
            .Include(recipe => recipe.Steps)
            .SingleOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        var recipeStep = recipe.AddStep(
            request.Instruction);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(recipeStep));
    }

    [HttpPut("{stepId:int}")]
    public async Task<ActionResult<RecipeStepResponse>> Update(
        int recipeId,
        int stepId,
        RecipeStepRequest request,
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
            .Include(recipe => recipe.Steps)
            .SingleOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        var recipeStep = recipe.Steps
            .SingleOrDefault(step => step.Id == stepId);

        if (recipeStep is null)
        {
            return NotFound();
        }

        recipe.UpdateStep(
            stepId,
            request.Instruction);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(recipeStep));
    }

    [HttpPut("{stepId:int}/position")]
    public async Task<IActionResult> Move(
    int recipeId,
    int stepId,
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
            .Include(recipe => recipe.Steps)
            .SingleOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        var exists = recipe.Steps.Any(
            recipeStep => recipeStep.Id == stepId);

        if (!exists)
        {
            return NotFound();
        }

        if (request.Position > recipe.Steps.Count)
        {
            ModelState.AddModelError(
                nameof(request.Position),
                $"Position cannot exceed {recipe.Steps.Count}.");

            return ValidationProblem(ModelState);
        }

        recipe.MoveStep(
            stepId,
            request.Position);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{stepId:int}")]
    public async Task<IActionResult> Delete(
        int recipeId,
        int stepId,
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
            .Include(recipe => recipe.Steps)
            .SingleOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        var exists = recipe.Steps.Any(
            recipeStep => recipeStep.Id == stepId);

        if (!exists)
        {
            return NotFound();
        }

        if (recipe.Steps.Count == 1)
        {
            ModelState.AddModelError(
                nameof(stepId),
                "A recipe must contain at least one cooking step.");

            return ValidationProblem(ModelState);
        }

        recipe.RemoveStep(stepId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static RecipeStepResponse ToResponse(
        RecipeStep recipeStep)
    {
        return new RecipeStepResponse(
            recipeStep.Id,
            recipeStep.Instruction,
            recipeStep.Position);
    }
}