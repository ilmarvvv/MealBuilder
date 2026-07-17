using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.PreparedRecipeBatches
{
    public class AddIngredientModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserAccessor _currentUser;

        public AddIngredientModel(AppDbContext context, CurrentUserAccessor currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public PreparedRecipeBatch PreparedRecipeBatch { get; set; } = new();

        public SelectList Ingredients { get; set; } = default!;

        [BindProperty]
        public int PreparedRecipeBatchId { get; set; }

        [BindProperty]
        public int IngredientId { get; set; }

        [BindProperty]
        public decimal Grams { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            PreparedRecipeBatch? preparedRecipeBatch = await _context.PreparedRecipeBatches
                .FirstOrDefaultAsync(batch =>
                    batch.Id == id &&
                    batch.OwnerId == _currentUser.UserId);

            if (preparedRecipeBatch is null)
            {
                return NotFound();
            }

            PreparedRecipeBatch = preparedRecipeBatch;
            PreparedRecipeBatchId = preparedRecipeBatch.Id;

            await LoadIngredientsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Grams < 0.01m || Grams > 100000m)
            {
                ModelState.AddModelError(nameof(Grams), "Grams must be between 0.01 and 100000.");
            }

            PreparedRecipeBatch? preparedRecipeBatch = await _context.PreparedRecipeBatches
                .Include(batch => batch.Items)
                .FirstOrDefaultAsync(batch =>
                    batch.Id == PreparedRecipeBatchId &&
                    batch.OwnerId == _currentUser.UserId);

            if (preparedRecipeBatch is null)
            {
                return NotFound();
            }

            Ingredient? ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(ingredient =>
                    ingredient.Id == IngredientId &&
                    ingredient.OwnerId == _currentUser.UserId);

            if (ingredient is null)
            {
                ModelState.AddModelError(nameof(IngredientId), "Ingredient was not found.");
            }

            if (!ModelState.IsValid || ingredient is null)
            {
                PreparedRecipeBatch = preparedRecipeBatch;
                await LoadIngredientsAsync();
                return Page();
            }

            int nextPosition = preparedRecipeBatch.Items.Count == 0
                ? 1
                : preparedRecipeBatch.Items.Max(item => item.Position) + 1;

            PreparedRecipeBatchItem item = new()
            {
                PreparedRecipeBatchId = preparedRecipeBatch.Id,
                ItemType = PreparedRecipeBatchItemType.Ingredient,
                SourceIngredientId = ingredient.Id,
                NameSnapshot = ingredient.Name,
                Grams = Grams,
                CaloriesSnapshot = ingredient.CaloriesPer100g * Grams / 100,
                ProteinSnapshot = ingredient.ProteinPer100g * Grams / 100,
                FiberSnapshot = ingredient.FiberPer100g * Grams / 100,
                SugarSnapshot = ingredient.SugarPer100g * Grams / 100,
                SaltSnapshot = ingredient.SaltPer100g * Grams / 100,
                Position = nextPosition
            };

            _context.PreparedRecipeBatchItems.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = preparedRecipeBatch.Id });
        }

        private async Task LoadIngredientsAsync()
        {
            List<Ingredient> ingredients = await _context.Ingredients
                .Where(ingredient => ingredient.OwnerId == _currentUser.UserId)
                .OrderBy(ingredient => ingredient.Name)
                .ToListAsync();

            Ingredients = new SelectList(ingredients, "Id", "Name");
        }
    }
}