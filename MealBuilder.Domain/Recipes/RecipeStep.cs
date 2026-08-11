namespace MealBuilder.Domain.Recipes;

public sealed class RecipeStep
{
    public const int MaxInstructionLength = 2000;

    private RecipeStep()
    {
    }

    internal RecipeStep(string instruction, int position)
    {
        Instruction = NormalizeInstruction(instruction);
        SetPosition(position);
    }

    public int Id { get; private set; }

    public int RecipeId { get; private set; }

    public string Instruction { get; private set; } = string.Empty;

    public int Position { get; private set; }

    internal void UpdateInstruction(string instruction)
    {
        Instruction = NormalizeInstruction(instruction);
    }

    internal void SetPosition(int position)
    {
        if (position <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(position),
                position,
                "Position must be greater than zero.");
        }

        Position = position;
    }

    private static string NormalizeInstruction(string instruction)
    {
        if (string.IsNullOrWhiteSpace(instruction))
        {
            throw new ArgumentException(
                "Recipe step instruction cannot be empty.",
                nameof(instruction));
        }

        var normalizedInstruction = instruction.Trim();

        if (normalizedInstruction.Length > MaxInstructionLength)
        {
            throw new ArgumentException(
                $"Recipe step instruction cannot exceed {MaxInstructionLength} characters.",
                nameof(instruction));
        }

        return normalizedInstruction;
    }
}