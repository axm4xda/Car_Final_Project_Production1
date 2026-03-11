namespace Car_Project.Services.Abstractions
{
    public interface IEmailService
    {
        /// <summary>Shop ödənişi uğurlu olduqda istifadəçiyə invoice maili göndərir.</summary>
        Task SendPaymentConfirmationAsync(
            string toEmail,
            string orderCode,
            string? transactionId,
            decimal amount,
            string paymentMethod,
            string? cardLastFour,
            DateTime paidAt,
            IEnumerable<(string ProductName, int Quantity, decimal UnitPrice)> items);

        /// <summary>VIP/ödənişli elan üçün ödəniş uğurlu olduqda istifadəçiyə mail göndərir.</summary>
        Task SendVipListingPaymentAsync(
            string toEmail,
            string ownerName,
            string carTitle,
            decimal amount,
            string? transactionId,
            DateTime paidAt);

        /// <summary>Qeydiyyat uğurlu olduqda istifadəçiyə xoş gəldin maili göndərir.</summary>
        Task SendWelcomeAsync(
            string toEmail,
            string fullName);

        /// <summary>Şifrə dəyişdirildikdə istifadəçiyə təhlükəsizlik bildirişi maili göndərir.</summary>
        Task SendPasswordChangedAsync(
            string toEmail,
            string fullName);

        /// <summary>Admin elanı təsdiq və ya rədd etdikdə elan sahibinə mail göndərir.</summary>
        Task SendCarListingStatusAsync(
            string toEmail,
            string ownerName,
            string carTitle,
            bool isApproved,
            string? adminNote);

        /// <summary>Admin satış müraciətini təsdiq və ya rədd etdikdə müraciət sahibinə mail göndərir.</summary>
        Task SendSellRequestStatusAsync(
            string toEmail,
            string ownerName,
            string carTitle,
            bool isApproved,
            string? adminNote);

        /// <summary>Şifrə sıfırlama linki göndərir.</summary>
        Task SendPasswordResetAsync(
            string toEmail,
            string fullName,
            string resetLink);
    }
}
