using MealBuilder.Domain.Ingredients;

namespace MealBuilder.Domain.Recipes;

public sealed class Recipe
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 1000;
    public const int MaxServings = 100;

    private readonly List<RecipeIngredient> _ingredients = [];
    private readonly List<RecipeStep> _steps = [];

    private Recipe()
    {
    }

    private Recipe(
        string ownerId,
        string name,
        string? description,
        int servings)
    {
        OwnerId = NormalizeRequired(ownerId, nameof(ownerId));
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        Servings = ValidateServings(servings);
    }

    public int Id { get; private set; }

    public string OwnerId { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int Servings { get; private set; }

    public IReadOnlyCollection<RecipeIngredient> Ingredients =>
        _ingredients.AsReadOnly();

    public IReadOnlyCollection<RecipeStep> Steps =>
        _steps.AsReadOnly();

    public static Recipe Create(
        string ownerId,
        string name,
        string? description,
        int servings = 1)
    {
        return new Recipe(
            ownerId,
            name,
            description,
            servings);
    }

    public void UpdateDetails(
        string name,
        string? description,
        int servings)
    {
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        Servings = ValidateServings(servings);
    }

    public RecipeIngredient AddIngredient(
        Ingredient ingredient,
        decimal grams)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        if (_ingredients.Any(
            recipeIngredient =>
                recipeIngredient.IngredientId == ingredient.Id))
        {
            throw new InvalidOperationException(
                "The ingredient already exists in this recipe.");
        }

        var recipeIngredient = new RecipeIngredient(
            ingredient,
            grams,
            _ingredients.Count + 1);

        _ingredients.Add(recipeIngredient);

        return recipeIngredient;
    }

    public void UpdateIngredient(
        int ingredientId,
        decimal grams)
    {
        var recipeIngredient = FindIngredient(ingredientId);
        recipeIngredient.UpdateGrams(grams);
    }

    public void RemoveIngredient(int ingredientId)
    {
        var recipeIngredient = FindIngredient(ingredientId);

        if (_ingredients.Count == 1)
        {
            throw new InvalidOperationException(
                "A recipe must contain at least one ingredient.");
        }

        _ingredients.Remove(recipeIngredient);
        ReindexIngredients();
    }

    public void MoveIngredient(
        int ingredientId,
        int newPosition)
    {
        var recipeIngredient = FindIngredient(ingredientId);

        ValidatePosition(
            newPosition,
            _ingredients.Count,
            nameof(newPosition));

        _ingredients.Remove(recipeIngredient);
        _ingredients.Insert(newPosition - 1, recipeIngredient);

        ReindexIngredients();
    }

    public RecipeStep AddStep(string instruction)
    {
        var recipeStep = new RecipeStep(
            instruction,
            _steps.Count + 1);

        _steps.Add(recipeStep);

        return recipeStep;
    }

    public void UpdateStep(
        int stepId,
        string instruction)
    {
        var recipeStep = FindStep(stepId);
        recipeStep.UpdateInstruction(instruction);
    }

    public void RemoveStep(int stepId)
    {
        var recipeStep = FindStep(stepId);

        if (_steps.Count == 1)
        {
            throw new InvalidOperationException(
                "A recipe must contain at least one cooking step.");
        }

        _steps.Remove(recipeStep);
        ReindexSteps();
    }

    public void MoveStep(
        int stepId,
        int newPosition)
    {
        var recipeStep = FindStep(stepId);

        ValidatePosition(
            newPosition,
            _steps.Count,
            nameof(newPosition));

        _steps.Remove(recipeStep);
        _steps.Insert(newPosition - 1, recipeStep);

        ReindexSteps();
    }

    public void EnsureCanBeSaved()
    {
        if (_ingredients.Count == 0)
        {
            throw new InvalidOperationException(
                "A recipe must contain at least one ingredient.");
        }

        if (_steps.Count == 0)
        {
            throw new InvalidOperationException(
                "A recipe must contain at least one cooking step.");
        }
    }

    private RecipeStep FindStep(int stepId)
    {
        return _steps.SingleOrDefault(
            recipeStep => recipeStep.Id == stepId)
            ?? throw new KeyNotFoundException(
                $"Recipe step {stepId} was not found.");
    }

    private void ReindexSteps()
    {
        for (var index = 0; index < _steps.Count; index++)
        {
            _steps[index].SetPosition(index + 1);
        }
    }

    private RecipeIngredient FindIngredient(int ingredientId)
    {
        return _ingredients.SingleOrDefault(
            recipeIngredient =>
                recipeIngredient.IngredientId == ingredientId)
            ?? throw new KeyNotFoundException(
                $"Ingredient {ingredientId} was not found in the recipe.");
    }

    private void ReindexIngredients()
    {
        for (var index = 0; index < _ingredients.Count; index++)
        {
            _ingredients[index].SetPosition(index + 1);
        }
    }

    private static void ValidatePosition(
        int position,
        int itemCount,
        string parameterName)
    {
        if (position <= 0 || position > itemCount)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                position,
                $"Position must be between 1 and {itemCount}.");
        }
    }

    private static string NormalizeName(string name)
    {
        var normalizedName = NormalizeRequired(
            name,
            nameof(name));

        if (normalizedName.Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"Recipe name cannot exceed {MaxNameLength} characters.",
                nameof(name));
        }

        return normalizedName;
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var normalizedDescription = description.Trim();

        if (normalizedDescription.Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"Recipe description cannot exceed {MaxDescriptionLength} characters.",
                nameof(description));
        }

        return normalizedDescription;
    }

    private static int ValidateServings(int servings)
    {
        if (servings <= 0 || servings > MaxServings)
        {
            throw new ArgumentOutOfRangeException(
                nameof(servings),
                servings,
                $"Servings must be greater than zero and cannot exceed {MaxServings}.");
        }

        return servings;
    }

    private static string NormalizeRequired(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be empty.",
                parameterName);
        }

        return value.Trim();
    }
}