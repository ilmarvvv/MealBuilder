namespace MealBuilder.Web.Models
{
    public class PreparedRecipeBatchSummary
    {
        public PreparedRecipeBatch PreparedRecipeBatch { get; set; } = new();

        public decimal UsedServings { get; set; }

        public decimal RemainingServings => PreparedRecipeBatch.TotalServings - UsedServings;
    }
}
