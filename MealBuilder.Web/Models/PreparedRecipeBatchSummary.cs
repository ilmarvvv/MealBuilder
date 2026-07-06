namespace MealBuilder.Web.Models
{
    public class PreparedRecipeBatchSummary
    {
        public PreparedRecipeBatch PreparedRecipeBatch { get; set; } = new();

        public decimal AllocatedServings { get; set; }

        public decimal ServingsPerDay =>
            PreparedRecipeBatch.TotalServings /
            PreparedRecipeBatch.PlannedDays;

        public decimal UnallocatedServings =>
            PreparedRecipeBatch.TotalServings -
            AllocatedServings;
    }
}
