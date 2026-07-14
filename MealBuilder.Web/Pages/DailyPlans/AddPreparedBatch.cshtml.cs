using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MealBuilder.Web.Pages.DailyPlans
{
    public class AddPreparedBatchModel : PageModel
    {
        private readonly AppDbContext _context;

        private readonly CurrentUserAccessor _currentUser;

        public AddPreparedBatchModel(AppDbContext context, CurrentUserAccessor currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public DailyPlan DailyPlan { get; set; } = new();

        [BindProperty]
        public DateOnly? DailyPlanDate { get; set; }

        public SelectList PreparedRecipeBatches { get; set; } = null!;

        [BindProperty]
        public DailyPlanItem DailyPlanItem { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id, DateOnly? date)
        {
            DailyPlan? dailyPlan;

            if (id.HasValue)
            {
                dailyPlan = await _context.DailyPlans
                    .FirstOrDefaultAsync(dailyPlan =>
                        dailyPlan.Id == id.Value &&
                        dailyPlan.OwnerId == _currentUser.UserId);

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
                    .FirstOrDefaultAsync(dailyPlan =>
                        dailyPlan.Date == date.Value &&
                        dailyPlan.OwnerId == _currentUser.UserId);

                if (dailyPlan is null)
                {
                    dailyPlan = new DailyPlan
                    {
                        OwnerId = _currentUser.UserId,
                        Date = date.Value,
                        Name = $"Daily Plan {date.Value}"
                    };
                }
            }

            DailyPlan = dailyPlan;
            DailyPlanDate = dailyPlan.Date;
            DailyPlanItem.DailyPlanId = dailyPlan.Id;
            DailyPlanItem.ItemType = DailyPlanItemType.PreparedRecipeBatch;

            await LoadPreparedRecipeBatchesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("DailyPlanItem.DailyPlan");
            ModelState.Remove("DailyPlanItem.Recipe");
            ModelState.Remove("DailyPlanItem.Ingredient");
            ModelState.Remove("DailyPlanItem.PreparedRecipeBatch");

            DailyPlanItem.ItemType = DailyPlanItemType.PreparedRecipeBatch;
            DailyPlanItem.RecipeId = null;
            DailyPlanItem.IngredientId = null;
            DailyPlanItem.Grams = null;

            if (DailyPlanItem.PreparedRecipeBatchId is null)
            {
                ModelState.AddModelError(
                    "DailyPlanItem.PreparedRecipeBatchId",
                    "Prepared batch is required.");
            }

            if (DailyPlanItem.ServingsCount is null ||
                DailyPlanItem.ServingsCount <= 0)
            {
                ModelState.AddModelError(
                    "DailyPlanItem.ServingsCount",
                    "Servings count must be greater than 0.");
            }

            if (DailyPlanItem.DailyPlanId <= 0 && !DailyPlanDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(DailyPlanDate),
                    "Daily plan date is required.");
            }

            if (DailyPlanItem.PreparedRecipeBatchId is not null &&
                DailyPlanItem.ServingsCount is not null)
            {
                PreparedRecipeBatch? preparedRecipeBatch =
                    await _context.PreparedRecipeBatches
                        .Include(preparedRecipeBatch =>
                            preparedRecipeBatch.DailyPlanItems)
                        .FirstOrDefaultAsync(preparedRecipeBatch =>
                            preparedRecipeBatch.Id == DailyPlanItem.PreparedRecipeBatchId &&
                            preparedRecipeBatch.Recipe != null &&
                            preparedRecipeBatch.Recipe.OwnerId == _currentUser.UserId);

                if (preparedRecipeBatch is null)
                {
                    ModelState.AddModelError(
                        "DailyPlanItem.PreparedRecipeBatchId",
                        "Prepared batch was not found.");
                }
                else
                {
                    decimal allocatedServings =
                        preparedRecipeBatch.DailyPlanItems
                            .Where(dailyPlanItem =>
                                dailyPlanItem.ServingsCount is not null)
                            .Sum(dailyPlanItem =>
                                dailyPlanItem.ServingsCount!.Value);

                    decimal unallocatedServings =
                        preparedRecipeBatch.TotalServings - allocatedServings;

                    if (DailyPlanItem.ServingsCount > unallocatedServings)
                    {
                        ModelState.AddModelError(
                            "DailyPlanItem.ServingsCount",
                            $"Only {unallocatedServings} unallocated servings are available for this prepared batch.");
                    }
                }
            }

            DailyPlan? dailyPlan = null;

            if (DailyPlanItem.DailyPlanId > 0)
            {
                dailyPlan = await _context.DailyPlans
                    .FirstOrDefaultAsync(dailyPlan =>
                        dailyPlan.Id == DailyPlanItem.DailyPlanId &&
                        dailyPlan.OwnerId == _currentUser.UserId);

                if (dailyPlan is null)
                {
                    return NotFound();
                }
            }
            else if (DailyPlanDate.HasValue)
            {
                dailyPlan = await _context.DailyPlans
                    .FirstOrDefaultAsync(dailyPlan =>
                        dailyPlan.Date == DailyPlanDate.Value &&
                        dailyPlan.OwnerId == _currentUser.UserId);
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
                        OwnerId = _currentUser.UserId,
                        Date = DailyPlanDate.Value,
                        Name = $"Daily Plan {DailyPlanDate.Value}"
                    };
                }

                DailyPlan = dailyPlan;

                await LoadPreparedRecipeBatchesAsync();
                return Page();
            }

            if (dailyPlan is null)
            {
                dailyPlan = new DailyPlan
                {
                    OwnerId = _currentUser.UserId,
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

        private async Task LoadPreparedRecipeBatchesAsync()
        {
            List<PreparedRecipeBatch> preparedRecipeBatches = await _context.PreparedRecipeBatches
                .Where(preparedRecipeBatch =>
                    preparedRecipeBatch.Recipe != null &&
                    preparedRecipeBatch.Recipe.OwnerId == _currentUser.UserId)
                .Include(preparedRecipeBatch => preparedRecipeBatch.DailyPlanItems)
                .OrderBy(preparedRecipeBatch => preparedRecipeBatch.RecipeNameSnapshot)
                .ThenByDescending(preparedRecipeBatch => preparedRecipeBatch.CookedDate)
                .ToListAsync();

            var availablePreparedRecipeBatches = preparedRecipeBatches
                .Select(preparedRecipeBatch =>
                {
                    decimal allocatedServings = preparedRecipeBatch.DailyPlanItems
                        .Where(dailyPlanItem => dailyPlanItem.ServingsCount is not null)
                        .Sum(dailyPlanItem => dailyPlanItem.ServingsCount!.Value);

                    decimal unallocatedServings =
                        preparedRecipeBatch.TotalServings - allocatedServings;

                    return new
                    {
                        preparedRecipeBatch.Id,

                        Label =
                            $"{preparedRecipeBatch.RecipeNameSnapshot} | " +
                            $"cooked {preparedRecipeBatch.CookedDate} | " +
                            $"unallocated {unallocatedServings} servings",

                        UnallocatedServings = unallocatedServings
                    };
                })
                .Where(preparedRecipeBatch =>
                    preparedRecipeBatch.UnallocatedServings > 0)
                .ToList();

            PreparedRecipeBatches = new SelectList(
                availablePreparedRecipeBatches,
                "Id",
                "Label");
        }
    }
}
