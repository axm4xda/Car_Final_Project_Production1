using Car_Project.Services.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Car_Project.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        private static readonly HashSet<string> _placeholderUsers = new(StringComparer.OrdinalIgnoreCase)
        {
            "your-email@gmail.com", "your-real-email@gmail.com", "example@gmail.com", ""
        };

        private static readonly HashSet<string> _placeholderPasswords = new(StringComparer.OrdinalIgnoreCase)
        {
            "your-app-password", "your-real-app-password", "your-password", "app-password", ""
        };

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // ── Shared helpers ────────────────────────────────────────────────────

        private (string host, int port, string user, string pass, string fromName, string fromEmail) GetSmtpSettings()
        {
            var host      = _config["EmailSettings:SmtpHost"]     ?? "smtp.gmail.com";
            var port      = int.TryParse(_config["EmailSettings:SmtpPort"], out var p) ? p : 587;
            var user      = (_config["EmailSettings:SmtpUser"]     ?? "").Trim();
            var pass      = (_config["EmailSettings:SmtpPassword"] ?? "").Trim();
            var fromName  = _config["EmailSettings:FromName"]      ?? "Aurexo";
            var fromEmail = (_config["EmailSettings:FromEmail"]    ?? user).Trim();
            return (host, port, user, pass, fromName, fromEmail);
        }

        private bool IsConfigured(string user, string pass) =>
            !_placeholderUsers.Contains(user) && !_placeholderPasswords.Contains(pass);

        private async Task SendAsync(string toEmail, string subject, string htmlBody, string logLabel)
        {
            var (host, port, user, pass, fromName, fromEmail) = GetSmtpSettings();

            if (!IsConfigured(user, pass))
            {
                _logger.LogWarning(
                    "[Email] Skipped ({Label}): placeholder credentials. Would have sent to {Email}.",
                    logLabel, toEmail);
                return;
            }

            _logger.LogInformation(
                "[Email] Attempting ({Label}) → {Email} via {Host}:{Port} from {User}.",
                logLabel, toEmail, host, port, user);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body    = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                var socketOptions = port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                await client.ConnectAsync(host, port, socketOptions);
                _logger.LogInformation("[Email] Connected to {Host}:{Port}.", host, port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Email] Connection failed ({Label}) → {Host}:{Port}.", logLabel, host, port);
                throw;
            }

            try
            {
                await client.AuthenticateAsync(user, pass);
                _logger.LogInformation("[Email] Authenticated as {User}.", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Email] Authentication failed ({Label}) as {User}.", logLabel, user);
                await client.DisconnectAsync(true);
                throw;
            }

            try
            {
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("[Email] Sent ({Label}) → {Email}.", logLabel, toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Email] Send failed ({Label}) → {Email}.", logLabel, toEmail);
                try { await client.DisconnectAsync(true); } catch { }
                throw;
            }
        }

        private static string HeaderBlock(string emoji, string title, string subtitle) => $@"
    <div style='background:linear-gradient(135deg,#FF5722,#e64a19);padding:32px 24px;text-align:center;'>
        <h1 style='margin:0;color:#fff;font-size:24px;font-weight:700;'>{emoji} {title}</h1>
        <p style='margin:8px 0 0;color:rgba(255,255,255,0.85);font-size:14px;'>{subtitle}</p>
    </div>";

        private static string FooterBlock(int year) => $@"
    <div style='background:#f9fafb;padding:20px 24px;text-align:center;border-top:1px solid #e5e7eb;'>
        <p style='margin:0;font-size:12px;color:#9ca3af;'>Bu email avtomatik göndərilib. Suallarınız üçün bizimlə əlaqə saxlayın.</p>
        <p style='margin:8px 0 0;font-size:12px;color:#9ca3af;'>&copy; {year} Aurexo. All Rights Reserved.</p>
    </div>";

        private static string WrapEmail(string header, string body) => $@"
<!DOCTYPE html><html><head><meta charset='UTF-8'></head>
<body style='margin:0;padding:0;background:#f9fafb;font-family:Arial,Helvetica,sans-serif;'>
<div style='max-width:600px;margin:30px auto;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);'>
{header}
<div style='padding:32px 24px;'>{body}</div>
{FooterBlock(DateTime.UtcNow.Year)}
</div></body></html>";

        // ── 1. Shop ödəniş təsdiqi ────────────────────────────────────────────

        public async Task SendPaymentConfirmationAsync(
            string toEmail,
            string orderCode,
            string? transactionId,
            decimal amount,
            string paymentMethod,
            string? cardLastFour,
            DateTime paidAt,
            IEnumerable<(string ProductName, int Quantity, decimal UnitPrice)> items)
        {
            var itemList  = items.ToList();
            var itemsHtml = string.Join("", itemList.Select(i =>
                $@"<tr>
                    <td style='padding:10px 16px;border-bottom:1px solid #f3f4f6;font-size:14px;color:#374151;'>{System.Net.WebUtility.HtmlEncode(i.ProductName)}</td>
                    <td style='padding:10px 16px;border-bottom:1px solid #f3f4f6;font-size:14px;color:#374151;text-align:center;'>{i.Quantity}</td>
                    <td style='padding:10px 16px;border-bottom:1px solid #f3f4f6;font-size:14px;color:#374151;text-align:right;'>${i.UnitPrice * i.Quantity:F2}</td>
                </tr>"));

            var cardInfo = !string.IsNullOrEmpty(cardLastFour) ? $"**** **** **** {cardLastFour}" : "N/A";

            var body = $@"
<div style='background:#f9fafb;border-radius:12px;padding:20px;margin-bottom:24px;'>
    <table style='width:100%;border-collapse:collapse;'>
        <tr><td style='padding:6px 0;font-size:14px;color:#6b7280;'>Sifariş kodu</td><td style='padding:6px 0;font-size:14px;font-weight:700;color:#1f2937;text-align:right;'>{orderCode}</td></tr>
        <tr><td style='padding:6px 0;font-size:14px;color:#6b7280;'>Tranzaksiya ID</td><td style='padding:6px 0;font-size:14px;font-weight:600;color:#1f2937;text-align:right;font-family:monospace;'>{transactionId ?? "N/A"}</td></tr>
        <tr><td style='padding:6px 0;font-size:14px;color:#6b7280;'>Ödəniş tarixi</td><td style='padding:6px 0;font-size:14px;color:#1f2937;text-align:right;'>{paidAt:dd MMM yyyy HH:mm} UTC</td></tr>
        <tr><td style='padding:6px 0;font-size:14px;color:#6b7280;'>Ödəniş üsulu</td><td style='padding:6px 0;font-size:14px;color:#1f2937;text-align:right;'>{paymentMethod}</td></tr>
        <tr><td style='padding:6px 0;font-size:14px;color:#6b7280;'>Kart</td><td style='padding:6px 0;font-size:14px;color:#1f2937;text-align:right;font-family:monospace;'>{cardInfo}</td></tr>
    </table>
</div>
<h3 style='margin:0 0 12px;font-size:16px;color:#1f2937;'>Alınan məhsullar</h3>
<table style='width:100%;border-collapse:collapse;margin-bottom:20px;'>
    <thead><tr style='background:#f3f4f6;'>
        <th style='padding:10px 16px;text-align:left;font-size:12px;font-weight:600;color:#6b7280;text-transform:uppercase;'>Məhsul</th>
        <th style='padding:10px 16px;text-align:center;font-size:12px;font-weight:600;color:#6b7280;text-transform:uppercase;'>Miqdar</th>
        <th style='padding:10px 16px;text-align:right;font-size:12px;font-weight:600;color:#6b7280;text-transform:uppercase;'>Cəm</th>
    </tr></thead>
    <tbody>{itemsHtml}</tbody>
</table>
<div style='background:#fff5f2;border-radius:12px;padding:16px 20px;text-align:right;'>
    <span style='font-size:14px;color:#6b7280;'>Ödənilən məbləğ:</span>
    <span style='font-size:22px;font-weight:800;color:#FF5722;margin-left:12px;'>${amount:F2}</span>
</div>";

            var html = WrapEmail(HeaderBlock("✅", "Ödəniş Uğurlu!", "Sifarişiniz təsdiqləndi"), body);
            await SendAsync(toEmail, $"Ödəniş Təsdiqi – {orderCode}", html, "ShopPayment");
        }

        // ── 2. VIP / ödənişli elan ödəniş təsdiqi ────────────────────────────

        public async Task SendVipListingPaymentAsync(
            string toEmail,
            string ownerName,
            string carTitle,
            decimal amount,
            string? transactionId,
            DateTime paidAt)
        {
            var body = $@"
<p style='font-size:15px;color:#374151;margin:0 0 20px;'>Salam, <strong>{System.Net.WebUtility.HtmlEncode(ownerName)}</strong>!</p>
<p style='font-size:14px;color:#6b7280;margin:0 0 24px;'>
    <strong style='color:#FF5722;'>VIP</strong> elan ödənişiniz uğurla tamamlandı. 
    Elanınız admin təsdiqindən sonra siyahının <strong>başında</strong> ⭐ işarəsi ilə göstəriləcək.
</p>
<div style='background:#f9fafb;border-radius:12px;padding:20px;margin-bottom:24px;'>
    <table style='width:100%;border-collapse:collapse;'>
        <tr><td style='padding:6px 0;font-size:14px;color:#6b7280;'>Avtomobil</td><td style='padding:6px 0;font-size:14px;font-weight:700;color:#1f2937;text-align:right;'>{System.Net.WebUtility.HtmlEncode(carTitle)}</td></tr>
        <tr><td style='padding:6px 0;font-size:14px;color:#6b7280;'>Tranzaksiya ID</td><td style='padding:6px 0;font-size:14px;font-family:monospace;color:#1f2937;text-align:right;'>{transactionId ?? "N/A"}</td></tr>
        <tr><td style='padding:6px 0;font-size:14px;color:#6b7280;'>Ödəniş tarixi</td><td style='padding:6px 0;font-size:14px;color:#1f2937;text-align:right;'>{paidAt:dd MMM yyyy HH:mm} UTC</td></tr>
    </table>
</div>
<div style='background:#fff5f2;border-radius:12px;padding:16px 20px;text-align:right;'>
    <span style='font-size:14px;color:#6b7280;'>Ödənilən məbləğ:</span>
    <span style='font-size:22px;font-weight:800;color:#FF5722;margin-left:12px;'>${amount:F2}</span>
</div>";

            var html = WrapEmail(HeaderBlock("⭐", "VIP Elan Ödənişi Təsdiqləndi!", "Elanınız tezliklə yayımlanacaq"), body);
            await SendAsync(toEmail, $"VIP Elan Ödənişi – {carTitle}", html, "VipPayment");
        }

        // ── 3. Xoş gəldin maili ──────────────────────────────────────────────

        public async Task SendWelcomeAsync(string toEmail, string fullName)
        {
            var body = $@"
<p style='font-size:15px;color:#374151;margin:0 0 16px;'>Salam, <strong>{System.Net.WebUtility.HtmlEncode(fullName)}</strong>! 🎉</p>
<p style='font-size:14px;color:#6b7280;margin:0 0 16px;'>
    <strong>Aurexo</strong> ailəsinə xoş gəldiniz! Hesabınız uğurla yaradıldı.
</p>
<p style='font-size:14px;color:#6b7280;margin:0 0 24px;'>
    İndi avtomobil elanlarına baxmaq, satışa çıxarmaq və ya VIP elan əlavə etmək üçün
    platformadan istifadə edə bilərsiniz.
</p>
<div style='text-align:center;margin-top:8px;'>
    <a href='/' style='display:inline-block;background:#FF5722;color:#fff;padding:12px 32px;
       border-radius:8px;text-decoration:none;font-weight:700;font-size:15px;'>
        Platforma keçin →
    </a>
</div>";

            var html = WrapEmail(HeaderBlock("👋", "Xoş Gəldiniz!", "Aurexo-ya üzv olduğunuz üçün təşəkkür edirik"), body);
            await SendAsync(toEmail, "Aurexo-ya xoş gəldiniz!", html, "Welcome");
        }

        // ── 4. Şifrə dəyişikliyi bildirişi ───────────────────────────────────

        public async Task SendPasswordChangedAsync(string toEmail, string fullName)
        {
            var body = $@"
<p style='font-size:15px;color:#374151;margin:0 0 16px;'>Salam, <strong>{System.Net.WebUtility.HtmlEncode(fullName)}</strong>!</p>
<p style='font-size:14px;color:#6b7280;margin:0 0 16px;'>
    Hesabınızın şifrəsi <strong>{DateTime.UtcNow:dd MMM yyyy HH:mm} UTC</strong> tarixində uğurla dəyişdirildi.
</p>
<div style='background:#fff5f2;border:1px solid #FFCCBC;border-radius:10px;padding:16px 20px;margin-bottom:24px;'>
    <p style='margin:0;font-size:13px;color:#BF360C;'>
        ⚠️ Əgər bu dəyişikliyi siz etməmisinizsə, dərhal bizimlə əlaqə saxlayın
        və hesabınızı qoruyun.
    </p>
</div>";

            var html = WrapEmail(HeaderBlock("🔐", "Şifrəniz Dəyişdirildi", "Təhlükəsizlik bildirişi"), body);
            await SendAsync(toEmail, "Şifrəniz dəyişdirildi – Aurexo", html, "PasswordChanged");
        }

        // ── 5. Elan statusu (Admin Approve / Reject) ─────────────────────────

        public async Task SendCarListingStatusAsync(
            string toEmail,
            string ownerName,
            string carTitle,
            bool isApproved,
            string? adminNote)
        {
            string emoji, title, subtitle, statusHtml;
            var encodedTitle = System.Net.WebUtility.HtmlEncode(carTitle);

            if (isApproved)
            {
                emoji    = "✅";
                title    = "Elanınız Təsdiqləndi!";
                subtitle = "Artıq List səhifəsində yayımlanır";
                statusHtml =
                    $"<p style='font-size:14px;color:#6b7280;margin:0 0 16px;'>" +
                    $"<strong style='color:#16a34a;'>&quot;{encodedTitle}&quot;</strong> adlı elanınız " +
                    $"admin tərəfindən <strong style='color:#16a34a;'>təsdiqləndi</strong> və " +
                    $"indi List səhifəsində istifadəçilərə göstərilir.</p>";
            }
            else
            {
                emoji    = "❌";
                title    = "Elanınız Rədd Edildi";
                subtitle = "Daha ətraflı məlumat üçün aşağıya baxın";
                statusHtml =
                    $"<p style='font-size:14px;color:#6b7280;margin:0 0 16px;'>" +
                    $"<strong style='color:#dc2626;'>&quot;{encodedTitle}&quot;</strong> adlı elanınız " +
                    $"admin tərəfindən <strong style='color:#dc2626;'>rədd edildi</strong>.</p>";
            }

            var noteHtml = !string.IsNullOrWhiteSpace(adminNote)
                ? $"<div style='background:#f9fafb;border-radius:10px;padding:16px 20px;margin-top:16px;'>" +
                  $"<p style='margin:0 0 6px;font-size:12px;font-weight:600;color:#6b7280;text-transform:uppercase;'>Admin Qeydi</p>" +
                  $"<p style='margin:0;font-size:14px;color:#374151;'>{System.Net.WebUtility.HtmlEncode(adminNote)}</p></div>"
                : "";

            var body =
                $"<p style='font-size:15px;color:#374151;margin:0 0 16px;'>Salam, " +
                $"<strong>{System.Net.WebUtility.HtmlEncode(ownerName)}</strong>!</p>" +
                statusHtml + noteHtml;

            var html = WrapEmail(HeaderBlock(emoji, title, subtitle), body);
            var subject = isApproved
                ? $"Elanınız Təsdiqləndi – {carTitle}"
                : $"Elanınız Rədd Edildi – {carTitle}";

            await SendAsync(toEmail, subject, html, isApproved ? "CarApproved" : "CarRejected");
        }

        // ── 6. Satış müraciəti statusu (Admin Approve / Reject) ───────────────

        public async Task SendSellRequestStatusAsync(
            string toEmail,
            string ownerName,
            string carTitle,
            bool isApproved,
            string? adminNote)
        {
            string emoji, title, subtitle, statusHtml;
            var encodedTitle = System.Net.WebUtility.HtmlEncode(carTitle);

            if (isApproved)
            {
                emoji    = "✅";
                title    = "Satış Müraciətiniz Qəbul Edildi!";
                subtitle = "Avtomobiliniz satış siyahısına əlavə edildi";
                statusHtml =
                    $"<p style='font-size:14px;color:#6b7280;margin:0 0 16px;'>" +
                    $"<strong style='color:#16a34a;'>&quot;{encodedTitle}&quot;</strong> üçün göndərdiyiniz " +
                    $"satış müraciəti admin tərəfindən <strong style='color:#16a34a;'>qəbul edildi</strong>. " +
                    $"Avtomobiliniz indi platformada yayımlanır.</p>";
            }
            else
            {
                emoji    = "❌";
                title    = "Satış Müraciətiniz Rədd Edildi";
                subtitle = "Daha ətraflı məlumat üçün aşağıya baxın";
                statusHtml =
                    $"<p style='font-size:14px;color:#6b7280;margin:0 0 16px;'>" +
                    $"<strong style='color:#dc2626;'>&quot;{encodedTitle}&quot;</strong> üçün göndərdiyiniz " +
                    $"satış müraciəti admin tərəfindən <strong style='color:#dc2626;'>rədd edildi</strong>.</p>";
            }

            var noteHtml = !string.IsNullOrWhiteSpace(adminNote)
                ? $"<div style='background:#f9fafb;border-radius:10px;padding:16px 20px;margin-top:16px;'>" +
                  $"<p style='margin:0 0 6px;font-size:12px;font-weight:600;color:#6b7280;text-transform:uppercase;'>Admin Qeydi</p>" +
                  $"<p style='margin:0;font-size:14px;color:#374151;'>{System.Net.WebUtility.HtmlEncode(adminNote)}</p></div>"
                : "";

            var body =
                $"<p style='font-size:15px;color:#374151;margin:0 0 16px;'>Salam, " +
                $"<strong>{System.Net.WebUtility.HtmlEncode(ownerName)}</strong>!</p>" +
                statusHtml + noteHtml;

            var html = WrapEmail(HeaderBlock(emoji, title, subtitle), body);
            var subject = isApproved
                ? $"Satış Müraciətiniz Qəbul Edildi – {carTitle}"
                : $"Satış Müraciətiniz Rədd Edildi – {carTitle}";

            await SendAsync(toEmail, subject, html, isApproved ? "SellRequestApproved" : "SellRequestRejected");
        }

        // ── 7. Şifrə sıfırlama linki ─────────────────────────────────────────

        public async Task SendPasswordResetAsync(string toEmail, string fullName, string resetLink)
        {
            var body = $@"
<p style='font-size:15px;color:#374151;margin:0 0 16px;'>Salam, <strong>{System.Net.WebUtility.HtmlEncode(fullName)}</strong>!</p>
<p style='font-size:14px;color:#6b7280;margin:0 0 24px;'>
    Şifrə sıfırlama tələbi aldıq. Şifrənizi yeniləmək üçün aşağıdakı düyməyə klikləyin:
</p>
<div style='text-align:center;margin-bottom:24px;'>
    <a href='{System.Net.WebUtility.HtmlEncode(resetLink)}' style='display:inline-block;background:#FF5722;color:#fff;padding:14px 36px;
       border-radius:10px;text-decoration:none;font-weight:700;font-size:15px;'>
        Şifrəni Sıfırla →
    </a>
</div>
<p style='font-size:13px;color:#9ca3af;margin:0 0 16px;'>Bu link 1 saat ərzində etibarlıdır.</p>
<div style='background:#fff5f2;border:1px solid #FFCCBC;border-radius:10px;padding:16px 20px;margin-bottom:24px;'>
    <p style='margin:0;font-size:13px;color:#BF360C;'>
        ⚠️ Əgər bu tələbi siz göndərməmisinizsə, bu emaili nəzərə almayın — hesabınız təhlükəsizdir.
    </p>
</div>
<p style='font-size:12px;color:#9ca3af;margin:0;'>Link işləmirsə, bu URL-i brauzerə kopyalayın:<br/>
<span style='word-break:break-all;color:#6b7280;'>{System.Net.WebUtility.HtmlEncode(resetLink)}</span></p>";

            var html = WrapEmail(HeaderBlock("🔑", "Şifrə Sıfırlama", "Şifrənizi yeniləyin"), body);
            await SendAsync(toEmail, "Şifrə Sıfırlama – Aurexo", html, "PasswordReset");
        }
    }
}
