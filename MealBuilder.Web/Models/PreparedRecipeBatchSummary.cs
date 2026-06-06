namespace MealBuilder.Web.Models
{
    public class PreparedRecipeBatchSummary
    {
        public PreparedRecipeBatch PreparedRecipeBatch { get; set; } = new();

        public decimal UsedServings { get; set; }

        public decimal ServingsPerDay => PreparedRecipeBatch.TotalServings / PreparedRecipeBatch.PlannedDays;

        public decimal RemainingServings => PreparedRecipeBatch.TotalServings - UsedServings;
    }
}
