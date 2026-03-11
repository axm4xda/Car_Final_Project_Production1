using Car_Project.Models;

namespace Car_Project.ViewModels.Payment
{
    public class PaymentResultViewModel
    {
        public bool IsSuccess { get; set; }
        public string? TransactionId { get; set; }
        public int OrderId { get; set; }
        public string OrderCode => $"ORD-{OrderId:D6}";
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public DateTime PaidAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class PaymentHistoryItemViewModel
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string OrderCode => $"ORD-{OrderId:D6}";
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
