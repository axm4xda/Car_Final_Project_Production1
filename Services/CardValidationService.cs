using System.Text.RegularExpressions;

namespace Car_Project.Services
{
    /// <summary>
    /// Credit card validation service using Luhn algorithm and field validation.
    /// Designed to be replaceable with a real payment gateway (e.g., Stripe) later.
    /// </summary>
    public class CardValidationService : ICardValidationService
    {
        /// <summary>
        /// Validates a credit card number using the Luhn algorithm.
        /// </summary>
        public bool IsValidCardNumber(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            // Remove spaces and dashes
            var cleaned = cardNumber.Replace(" ", "").Replace("-", "");

            // Must be digits only, 13–19 characters
            if (!Regex.IsMatch(cleaned, @"^\d{13,19}$"))
                return false;

            return PassesLuhnCheck(cleaned);
        }

        /// <summary>
        /// Validates expiration date in MM/YY format and checks it's not expired.
        /// </summary>
        public bool IsValidExpiry(string? expiry)
        {
            if (string.IsNullOrWhiteSpace(expiry))
                return false;

            // Accept MM/YY or MM/YYYY
            var match = Regex.Match(expiry.Trim(), @"^(0[1-9]|1[0-2])\/(\d{2}|\d{4})$");
            if (!match.Success)
                return false;

            var month = int.Parse(match.Groups[1].Value);
            var yearStr = match.Groups[2].Value;
            var year = yearStr.Length == 2
                ? 2000 + int.Parse(yearStr)
                : int.Parse(yearStr);

            // Card is valid through the end of the expiry month
            var expiryDate = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
            return expiryDate >= DateTime.UtcNow;
        }

        /// <summary>
        /// Validates CVV (3 or 4 digits).
        /// </summary>
        public bool IsValidCvv(string? cvv)
        {
            if (string.IsNullOrWhiteSpace(cvv))
                return false;

            return Regex.IsMatch(cvv.Trim(), @"^\d{3,4}$");
        }

        /// <summary>
        /// Validates cardholder name (non-empty, 2+ characters, letters/spaces only).
        /// </summary>
        public bool IsValidCardHolderName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var trimmed = name.Trim();
            return trimmed.Length >= 2 && Regex.IsMatch(trimmed, @"^[a-zA-ZÀ-ÿşŞğĞıİöÖüÜçÇəƏ\s\.\-']+$");
        }

        /// <summary>
        /// Performs full card validation; returns a list of error messages.
        /// </summary>
        public IList<string> ValidateCard(string? cardNumber, string? expiry, string? cvv, string? cardHolderName)
        {
            var errors = new List<string>();

            if (!IsValidCardHolderName(cardHolderName))
                errors.Add("Kart sahibinin adı düzgün deyil.");

            if (!IsValidCardNumber(cardNumber))
                errors.Add("Kart nömrəsi etibarsızdır.");

            if (!IsValidExpiry(expiry))
                errors.Add("Son istifadə tarixi etibarsız və ya vaxtı keçib.");

            if (!IsValidCvv(cvv))
                errors.Add("CVV kodu düzgün deyil (3 və ya 4 rəqəm olmalıdır).");

            return errors;
        }

        /// <summary>
        /// Luhn algorithm implementation.
        /// </summary>
        private static bool PassesLuhnCheck(string number)
        {
            int sum = 0;
            bool alternate = false;

            for (int i = number.Length - 1; i >= 0; i--)
            {
                int digit = number[i] - '0';

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }
    }

    public interface ICardValidationService
    {
        bool IsValidCardNumber(string? cardNumber);
        bool IsValidExpiry(string? expiry);
        bool IsValidCvv(string? cvv);
        bool IsValidCardHolderName(string? name);
        IList<string> ValidateCard(string? cardNumber, string? expiry, string? cvv, string? cardHolderName);
    }
}
