using System.Text.Json;

namespace Car_Project.Services
{
    /// <summary>
    /// Google reCAPTCHA v2/v3 server-side verification service.
    /// </summary>
    public class RecaptchaService : IRecaptchaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecaptchaService> _logger;

        public RecaptchaService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<RecaptchaService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public string SiteKey => _configuration["Recaptcha:SiteKey"] ?? "";

        public bool IsEnabled =>
            !string.IsNullOrWhiteSpace(_configuration["Recaptcha:SiteKey"]) &&
            !string.IsNullOrWhiteSpace(_configuration["Recaptcha:SecretKey"]);

        public async Task<bool> VerifyAsync(string? recaptchaResponse)
        {
            if (!IsEnabled)
            {
                _logger.LogWarning("reCAPTCHA is not configured. Skipping verification.");
                return true; // Allow if not configured
            }

            if (string.IsNullOrWhiteSpace(recaptchaResponse))
                return false;

            var secretKey = _configuration["Recaptcha:SecretKey"]!;

            try
            {
                var client = _httpClientFactory.CreateClient();
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", secretKey),
                    new KeyValuePair<string, string>("response", recaptchaResponse)
                });

                var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<RecaptchaVerifyResponse>(json);

                if (result == null)
                    return false;

                // For reCAPTCHA v3, also check score
                if (result.Score.HasValue && result.Score.Value < 0.5m)
                {
                    _logger.LogWarning("reCAPTCHA v3 score too low: {Score}", result.Score.Value);
                    return false;
                }

                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "reCAPTCHA verification failed with exception.");
                return false;
            }
        }
    }

    public interface IRecaptchaService
    {
        string SiteKey { get; }
        bool IsEnabled { get; }
        Task<bool> VerifyAsync(string? recaptchaResponse);
    }

    internal class RecaptchaVerifyResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("score")]
        public decimal? Score { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("action")]
        public string? Action { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("challenge_ts")]
        public string? ChallengeTs { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("hostname")]
        public string? Hostname { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}
