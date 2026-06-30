using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.DailyPlans
{
    public class AddIngredientModel : PageModel
    {
        private readonly AppDbContext _context;

        public AddIngredientModel(AppDbContext context)
        {
            _context = context;
        }

        public DailyPlan DailyPlan { get; set; } = new();

        public SelectList Ingredients { get; set; } = null!;

        [BindProperty]
        public DailyPlanItem DailyPlanItem { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            DailyPlan? dailyPlan = await _context.DailyPlans.FindAsync(id);

            if (dailyPlan is null)
            {
                return NotFound();
            }

            DailyPlan = dailyPlan;
            DailyPlanItem.DailyPlanId = dailyPlan.Id;
            DailyPlanItem.ItemType = DailyPlanItemType.Ingredient;

            await LoadIngredientsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("DailyPlanItem.DailyPlan");
            ModelState.Remove("DailyPlanItem.Recipe");
            ModelState.Remove("DailyPlanItem.Ingredient");

            DailyPlanItem.ItemType = DailyPlanItemType.Ingredient;
            DailyPlanItem.RecipeId = null;
            DailyPlanItem.ServingsCount = null;

            if (DailyPlanItem.IngredientId is null)
            {
                ModelState.AddModelError("DailyPlanItem.IngredientId", "Ingredient is required.");
            }

            if (DailyPlanItem.Grams is null || DailyPlanItem.Grams <= 0)
            {
                ModelState.AddModelError("DailyPlanItem.Grams", "Grams must be greater than 0.");
            }

            if (!ModelState.IsValid)
            {
                DailyPlan? dailyPlan = await _context.DailyPlans.FindAsync(DailyPlanItem.DailyPlanId);

                if (dailyPlan is not null)
                {
                    DailyPlan = dailyPlan;
                }

                await LoadIngredientsAsync();
                return Page();
            }

            _context.DailyPlanItems.Add(DailyPlanItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = DailyPlanItem.DailyPlanId });
        }

        private async Task LoadIngredientsAsync()
        {
            List<Ingredient> ingredients = await _context.Ingredients
                .OrderBy(ingredient => ingredient.Name)
                .ToListAsync();

            Ingredients = new SelectList(ingredients, "Id", "Name");
        }
    }
}
