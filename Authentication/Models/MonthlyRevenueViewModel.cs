namespace Authentication.Models
{
    public class MonthlyRevenueViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public double TotalPrice { get; set; }
        public double TotalDiscount { get; set; }
        public double TotalRevenue { get; set; }
    }
}
