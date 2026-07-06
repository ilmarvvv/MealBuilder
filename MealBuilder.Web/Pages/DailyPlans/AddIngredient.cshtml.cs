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

        [BindProperty]
        public DateOnly? DailyPlanDate { get; set; }

        public SelectList Ingredients { get; set; } = null!;

        [BindProperty]
        public DailyPlanItem DailyPlanItem { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id, DateOnly? date)
        {
            DailyPlan? dailyPlan;

            if (id.HasValue)
            {
                dailyPlan = await _context.DailyPlans.FindAsync(id.Value);

                if (dailyPlan is null)
                {
                    return NotFound();
                }
            }
            else
            {
                if (!date.HasValue)
                {
                    return BadRequest();
                }

                dailyPlan = await _context.DailyPlans
                    .FirstOrDefaultAsync(dailyPlan => dailyPlan.Date == date.Value);

                if (dailyPlan is null)
                {
                    dailyPlan = new DailyPlan
                    {
                        Date = date.Value,
                        Name = $"Daily Plan {date.Value}"
                    };
                }
            }

            DailyPlan = dailyPlan;
            DailyPlanDate = dailyPlan.Date;
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
            ModelState.Remove("DailyPlanItem.PreparedRecipeBatch");

            DailyPlanItem.ItemType = DailyPlanItemType.Ingredient;
            DailyPlanItem.RecipeId = null;
            DailyPlanItem.PreparedRecipeBatchId = null;
            DailyPlanItem.ServingsCount = null;

            if (DailyPlanItem.IngredientId is null)
            {
                ModelState.AddModelError(
                    "DailyPlanItem.IngredientId",
                    "Ingredient is required.");
            }

            if (DailyPlanItem.Grams is null || DailyPlanItem.Grams <= 0)
            {
                ModelState.AddModelError(
                    "DailyPlanItem.Grams",
                    "Grams must be greater than 0.");
            }

            if (DailyPlanItem.DailyPlanId <= 0 && !DailyPlanDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(DailyPlanDate),
                    "Daily plan date is required.");
            }

            DailyPlan? dailyPlan = null;

            if (DailyPlanItem.DailyPlanId > 0)
            {
                dailyPlan = await _context.DailyPlans
                    .FindAsync(DailyPlanItem.DailyPlanId);

                if (dailyPlan is null)
                {
                    return NotFound();
                }
            }
            else if (DailyPlanDate.HasValue)
            {
                dailyPlan = await _context.DailyPlans
                    .FirstOrDefaultAsync(dailyPlan =>
                        dailyPlan.Date == DailyPlanDate.Value);
            }

            if (!ModelState.IsValid)
            {
                if (dailyPlan is null)
                {
                    if (!DailyPlanDate.HasValue)
                    {
                        return BadRequest();
                    }

                    dailyPlan = new DailyPlan
                    {
                        Date = DailyPlanDate.Value,
                        Name = $"Daily Plan {DailyPlanDate.Value}"
                    };
                }

                DailyPlan = dailyPlan;

                await LoadIngredientsAsync();
                return Page();
            }

            if (dailyPlan is null)
            {
                dailyPlan = new DailyPlan
                {
                    Date = DailyPlanDate!.Value,
                    Name = $"Daily Plan {DailyPlanDate.Value}"
                };

                _context.DailyPlans.Add(dailyPlan);
            }

            DailyPlanItem.DailyPlan = dailyPlan;

            _context.DailyPlanItems.Add(DailyPlanItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = dailyPlan.Id });
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
