namespace Car_Project.Models
{
    public class LoanCalculation : BaseEntity
    {
        public decimal CarPrice { get; set; }
        public decimal DownPayment { get; set; }
        public decimal InterestRate { get; set; }
        public int LoanTermMonths { get; set; }
        public decimal MonthlyPayment { get; set; }
        public decimal TotalInterestPayment { get; set; }
        public decimal TotalLoanAmount { get; set; }
    }
}
