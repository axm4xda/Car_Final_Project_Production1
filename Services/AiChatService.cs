using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Car_Project.Data;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public sealed class AiChatService : IAiChatService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<AiChatService> _logger;
        private readonly ApplicationDbContext _db;

        // Sabit sistem mesajı — avtomobil satış assistenti konteksti
        private const string SystemPromptBase =
            """
            Sən "Aurexo" avtomobil alış-satış platformasının AI köməkçisisən.
            Adın Aurexo Assistant-dır. İstifadəçilərə avtomobil axtarışı, qiymət müqayisəsi,
            kredit hesablaması, satış agentləri haqqında məlumat, texniki xüsusiyyətlər və
            ümumi avtomobil sualları ilə kömək edirsən.
            Cavablarını qısa, dəqiq və yardımsevər ver. Azərbaycan dilində cavab ver,
            lakin istifadəçi başqa dildə yazarsa həmin dildə cavab ver.
            Formatlamada markdown istifadə etmə, sadə mətn yaz.

            İstifadəçi hansı maşın yaxşıdır, hansını alım, müqayisə et, məsləhət ver kimi suallar soruşduqda,
            aşağıdakı real inventar datasına əsasən cavab ver. Yalnız platformada mövcud olan maşınları tövsiyə et.
            Büdcəyə, yanacaq növünə, vəziyyətə (yeni/işlənmiş), ilə və xüsusiyyətlərə görə ən uyğun variantları təklif et.
            Əgər istifadəçi konkret marka və ya model soruşarsa, inventarda olanları göstər.
            """;

        // Timeout for OpenAI API calls
        private static readonly TimeSpan ApiTimeout = TimeSpan.FromSeconds(30);

        public AiChatService(IConfiguration config, IHttpClientFactory httpFactory,
            ILogger<AiChatService> logger, ApplicationDbContext db)
        {
            _config      = config;
            _httpFactory  = httpFactory;
            _logger       = logger;
            _db           = db;
        }

        /// <summary>
        /// Verilənlər bazasından aktiv maşınların xülasəsini hazırlayır — AI konteksti üçün.
        /// </summary>
        public async Task<string> GetCarInventorySummaryAsync(CancellationToken ct = default)
        {
            try
            {
                var cars = await _db.Cars
                    .AsNoTracking()
                    .Where(c => c.IsApproved)
                    .Include(c => c.Brand)
                    .OrderByDescending(c => c.CreatedDate)
                    .Take(50) // Son 50 aktiv elan — token limitini aşmamaq üçün
                    .Select(c => new
                    {
                        c.Title,
                        Brand = c.Brand.Name,
                        c.Price,
                        c.Year,
                        c.Mileage,
                        c.FuelType,
                        c.Transmission,
                        c.Condition,
                        c.BodyStyle,
                        c.Color,
                        c.Cylinders,
                        c.DoorCount,
                        c.Id
                    })
                    .ToListAsync(ct);

                if (cars.Count == 0)
                    return "";

                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("=== PLATFORMADA MÖVCUD OLAN AVTOMOBILLƏR ===");
                sb.AppendLine($"Cəmi aktiv elan sayı: {cars.Count}");
                sb.AppendLine();

                // Marka üzrə qruplaşdır
                var brandGroups = cars.GroupBy(c => c.Brand).OrderBy(g => g.Key);
                foreach (var group in brandGroups)
                {
                    sb.AppendLine($"--- {group.Key} ({group.Count()} elan) ---");
                    foreach (var car in group)
                    {
                        sb.Append($"  - {car.Title} | {car.Year} il | {car.Price:N0} AZN");
                        sb.Append($" | {car.FuelType} | {car.Transmission} | {car.Condition}");
                        sb.Append($" | {car.Mileage:N0} km");
                        if (!string.IsNullOrEmpty(car.BodyStyle))
                            sb.Append($" | {car.BodyStyle}");
                        if (!string.IsNullOrEmpty(car.Color))
                            sb.Append($" | {car.Color}");
                        sb.Append($" | ID:{car.Id}");
                        sb.AppendLine();
                    }
                }

                // Qısa statistika
                sb.AppendLine();
                sb.AppendLine("=== QİYMƏT STATİSTİKASI ===");
                sb.AppendLine($"Ən ucuz: {cars.Min(c => c.Price):N0} AZN");
                sb.AppendLine($"Ən bahalı: {cars.Max(c => c.Price):N0} AZN");
                sb.AppendLine($"Orta qiymət: {cars.Average(c => c.Price):N0} AZN");
                sb.AppendLine($"Yeni avtomobillər: {cars.Count(c => c.Condition == Models.CarCondition.New)}");
                sb.AppendLine($"İşlənmiş avtomobillər: {cars.Count(c => c.Condition == Models.CarCondition.Used)}");

                var fuelGroups = cars.GroupBy(c => c.FuelType);
                foreach (var fg in fuelGroups)
                    sb.AppendLine($"  {fg.Key}: {fg.Count()} elan");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Avtomobil inventar xülasəsi hazırlana bilmədi.");
                return "";
            }
        }

        public async Task<string> GetResponseAsync(List<AiChatMessage> conversationHistory, CancellationToken ct = default)
        {
            // Null/boş tarixçə yoxlaması
            if (conversationHistory is null || conversationHistory.Count == 0)
            {
                return "Sual verməmisiniz. Zəhmət olmasa mesajınızı yazın.";
            }

            var apiKey = _config["OpenAI:ApiKey"];

            // API key yoxdursa mock cavab qaytar
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_OPENAI_API_KEY_HERE")
            {
                return await GetMockResponseAsync(conversationHistory.LastOrDefault()?.Content ?? "", ct);
            }

            try
            {
                return await CallOpenAiAsync(apiKey, conversationHistory, ct);
            }
            catch (OperationCanceledException)
            {
                throw; // CancellationToken üçün yenidən at — Hub handle edəcək
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "OpenAI API network error, falling back to mock response.");
                return await GetMockResponseAsync(conversationHistory.LastOrDefault()?.Content ?? "", ct);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "OpenAI API returned unexpected JSON format.");
                return "Üzr istəyirəm, AI servisdən gözlənilməz cavab alındı. Zəhmət olmasa yenidən cəhd edin.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI API call failed, falling back to mock response.");
                return await GetMockResponseAsync(conversationHistory.LastOrDefault()?.Content ?? "", ct);
            }
        }

        private async Task<string> CallOpenAiAsync(string apiKey, List<AiChatMessage> history, CancellationToken ct)
        {
            var model = _config["OpenAI:Model"] ?? "gpt-3.5-turbo";
            var maxTokens = int.TryParse(_config["OpenAI:MaxTokens"], out var mt) ? mt : 500;

            // Dinamik sistem mesajı: əsas prompt + real inventar datası
            var inventorySummary = await GetCarInventorySummaryAsync(ct);
            var fullSystemPrompt = SystemPromptBase + inventorySummary;

            // Mesaj siyahısını hazırlayın
            var messages = new List<object>
            {
                new { role = "system", content = fullSystemPrompt }
            };
            foreach (var msg in history)
            {
                // Yalnız icazə verilən rolları qəbul et
                var role = msg.Role switch
                {
                    "user" => "user",
                    "assistant" => "assistant",
                    "system" => "system",
                    _ => "user"
                };
                messages.Add(new { role, content = msg.Content ?? string.Empty });
            }

            var requestBody = new
            {
                model,
                messages,
                max_tokens    = maxTokens,
                temperature   = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var client = _httpFactory.CreateClient();
            client.Timeout = ApiTimeout;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await client.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json"),
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("OpenAI API returned {StatusCode}: {Body}",
                    (int)response.StatusCode, errorBody.Length > 500 ? errorBody[..500] : errorBody);

                // Rate limit (429) üçün xüsusi mesaj
                if ((int)response.StatusCode == 429)
                {
                    return "AI servis hal-hazırda çox yüklüdür. Zəhmət olmasa bir neçə saniyə sonra yenidən cəhd edin.";
                }

                response.EnsureSuccessStatusCode(); // Digər xətalar üçün exception at
            }

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseJson);

            // Cavab strukturunu yoxla
            if (!doc.RootElement.TryGetProperty("choices", out var choices)
                || choices.GetArrayLength() == 0)
            {
                _logger.LogWarning("OpenAI API returned empty choices array.");
                return "AI servisdən cavab alına bilmədi. Yenidən cəhd edin.";
            }

            var firstChoice = choices[0];
            if (!firstChoice.TryGetProperty("message", out var message)
                || !message.TryGetProperty("content", out var contentElement))
            {
                _logger.LogWarning("OpenAI API response missing message.content.");
                return "AI servisdən cavab alına bilmədi. Yenidən cəhd edin.";
            }

            var content = contentElement.GetString();
            return content?.Trim() ?? "Cavab alına bilmədi.";
        }

        /// <summary>
        /// OpenAI API key olmadıqda istifadə olunan mock cavab generatoru — real DB datasını istifadə edir.
        /// </summary>
        private async Task<string> GetMockResponseAsync(string userMessage, CancellationToken ct)
        {
            var msg = userMessage.ToLowerInvariant();

            if (msg.Contains("salam") || msg.Contains("hello") || msg.Contains("hi"))
                return "Salam! Mən Aurexo Assistant-am. Avtomobil axtarışı, qiymət müqayisəsi, məsləhət və ya hər hansı sualınızda sizə kömək edə bilərəm. Hansı maşın haqqında məlumat istəyirsiniz?";

            // Hansı maşın yaxşıdır / tövsiyə / məsləhət sualları
            if (msg.Contains("yaxşı") || msg.Contains("yaxsi") || msg.Contains("tövsiyə") || msg.Contains("tovsiye")
                || msg.Contains("məsləhət") || msg.Contains("meslehet") || msg.Contains("hansı") || msg.Contains("hansi")
                || msg.Contains("recommend") || msg.Contains("best") || msg.Contains("suggest")
                || msg.Contains("alım") || msg.Contains("alim") || msg.Contains("seçim") || msg.Contains("secim"))
            {
                return await BuildRecommendationResponseAsync(msg, ct);
            }

            // Müqayisə sualları
            if (msg.Contains("müqayisə") || msg.Contains("muqayise") || msg.Contains("compare") || msg.Contains("fərq")
                || msg.Contains("ferq") || msg.Contains("vs") || msg.Contains("yoxsa"))
            {
                return await BuildComparisonResponseAsync(msg, ct);
            }

            // Qiymət sualları — real datadan
            if (msg.Contains("qiymət") || msg.Contains("qiymet") || msg.Contains("price") || msg.Contains("nəçəyə")
                || msg.Contains("neçəyə") || msg.Contains("necəyə") || msg.Contains("ucuz") || msg.Contains("bahalı")
                || msg.Contains("büdcə") || msg.Contains("budce"))
            {
                return await BuildPriceResponseAsync(msg, ct);
            }

            // Marka / model axtarışı — real datadan
            if (await IsCarBrandQueryAsync(msg, ct))
            {
                return await BuildBrandResponseAsync(msg, ct);
            }

            if (msg.Contains("kredit") || msg.Contains("loan") || msg.Contains("calculator") || msg.Contains("hesabla"))
                return "Kredit hesablaması üçün saytımızdakı Calculator bölməsindən istifadə edə bilərsiniz. Orada aylıq ödəniş, faiz dərəcəsi və kredit müddətini hesablaya bilərsiniz.";

            if (msg.Contains("agent") || msg.Contains("dealer") || msg.Contains("satıcı") || msg.Contains("satici"))
                return "Satış agentlərimizi 'Sale Agents' bölməsindən görə bilərsiniz. Hər agentin əlaqə nömrəsi, ünvanı və müştəri rəyləri mövcuddur. Verified (təsdiqlənmiş) agentlərə üstünlük verməyiniz tövsiyə olunur.";

            if (msg.Contains("sat") || msg.Contains("sell") || msg.Contains("elan"))
                return "Avtomobilinizi satmaq üçün 'Add Listing' düyməsinə basıb elan yerləşdirə bilərsiniz. Elanınız admin tərəfindən təsdiqləndikdən sonra saytda görünəcək.";

            // Yanacaq növü sualları
            if (msg.Contains("elektrik") || msg.Contains("electric") || msg.Contains("hybrid") || msg.Contains("hibrid")
                || msg.Contains("dizel") || msg.Contains("diesel") || msg.Contains("benzin") || msg.Contains("petrol"))
            {
                return await BuildFuelTypeResponseAsync(msg, ct);
            }

            // Ümumi maşın sualı — inventar göstərin
            if (msg.Contains("maşın") || msg.Contains("masin") || msg.Contains("avto") || msg.Contains("car")
                || msg.Contains("nəyiniz var") || msg.Contains("neyiniz var") || msg.Contains("mövcud") || msg.Contains("movcud"))
            {
                return await BuildInventoryOverviewAsync(ct);
            }

            if (msg.Contains("necə") || msg.Contains("how") || msg.Contains("nə edim"))
                return "Daha dəqiq kömək etmək üçün sualınızı ətraflı izah edə bilərsiniz? Məsələn: 'Büdcəm 20000 AZN-dir, hansı maşını alım?', 'BMW ilə Mercedes hansı yaxşıdır?', 'Elektrik maşınları göstər' kimi suallar verə bilərsiniz.";

            return "Sizə kömək edə bilərəm! Məsələn bu sualları verə bilərsiniz:\n- Hansı maşın yaxşıdır?\n- Büdcəm 15000 AZN, nə alım?\n- BMW ilə Toyota müqayisə et\n- Ən ucuz maşınlar hansılardır?\n- Elektrik maşınları göstər\n- Hansı markalar var?";
        }

        /// <summary>
        /// Tövsiyə / məsləhət cavabı — büdcəyə və tələblərə görə real datadan maşın seçir.
        /// </summary>
        private async Task<string> BuildRecommendationResponseAsync(string msg, CancellationToken ct)
        {
            var query = _db.Cars.AsNoTracking()
                .Where(c => c.IsApproved)
                .Include(c => c.Brand)
                .AsQueryable();

            // Büdcə aşkarlama
            decimal? budget = ExtractBudget(msg);
            if (budget.HasValue)
            {
                query = query.Where(c => c.Price <= budget.Value);
            }

            // Vəziyyət filtrləri
            if (msg.Contains("yeni") || msg.Contains("new") || msg.Contains("sıfır") || msg.Contains("sifir"))
                query = query.Where(c => c.Condition == Models.CarCondition.New);
            else if (msg.Contains("işlənmiş") || msg.Contains("islenmiş") || msg.Contains("used") || msg.Contains("ikinci əl"))
                query = query.Where(c => c.Condition == Models.CarCondition.Used);

            // Yanacaq filtrləri
            if (msg.Contains("elektrik") || msg.Contains("electric"))
                query = query.Where(c => c.FuelType == Models.FuelType.Electric);
            else if (msg.Contains("hybrid") || msg.Contains("hibrid"))
                query = query.Where(c => c.FuelType == Models.FuelType.Hybrid);
            else if (msg.Contains("dizel") || msg.Contains("diesel"))
                query = query.Where(c => c.FuelType == Models.FuelType.Diesel);

            var cars = await query.OrderBy(c => c.Price).Take(5).ToListAsync(ct);

            if (cars.Count == 0)
            {
                return budget.HasValue
                    ? $"Təəssüf ki, {budget.Value:N0} AZN büdcəyə uyğun elan tapılmadı. Büdcənizi artırmağı və ya fərqli filtrləri sınamağı tövsiyə edirəm."
                    : "Hal-hazırda bu kriterlərə uyğun elan tapılmadı. Zəhmət olmasa fərqli parametrlərlə yenidən soruşun.";
            }

            var sb = new StringBuilder();
            sb.AppendLine(budget.HasValue
                ? $"Büdcənizə ({budget.Value:N0} AZN) uyğun ən yaxşı variantlar:"
                : "Sizin üçün ən yaxşı variantlar:");
            sb.AppendLine();

            foreach (var car in cars)
            {
                sb.AppendLine($"• {car.Brand.Name} {car.Title} ({car.Year}) — {car.Price:N0} AZN");
                sb.Append($"  {car.FuelType}, {car.Transmission}, {car.Condition}");
                if (car.Mileage > 0) sb.Append($", {car.Mileage:N0} km");
                if (!string.IsNullOrEmpty(car.Color)) sb.Append($", {car.Color}");
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("Daha ətraflı məlumat üçün 'List' bölməsindən avtomobillərə baxa bilərsiniz. Başqa sualınız varsa çəkinmədən soruşun!");
            return sb.ToString();
        }

        /// <summary>
        /// Müqayisə cavabı — iki markadan olan maşınları yan-yana göstərir.
        /// </summary>
        private async Task<string> BuildComparisonResponseAsync(string msg, CancellationToken ct)
        {
            var brands = await _db.Brands.AsNoTracking().ToListAsync(ct);
            var matchedBrands = brands
                .Where(b => msg.Contains(b.Name.ToLowerInvariant()))
                .Take(2)
                .ToList();

            if (matchedBrands.Count < 2)
            {
                // Ən populyar maşınları göstərin
                var topCars = await _db.Cars.AsNoTracking()
                    .Where(c => c.IsApproved)
                    .Include(c => c.Brand)
                    .OrderByDescending(c => c.CreatedDate)
                    .Take(5)
                    .ToListAsync(ct);

                if (topCars.Count == 0)
                    return "Müqayisə üçün iki marka adı yazın. Məsələn: 'BMW ilə Mercedes müqayisə et'. Hal-hazırda platformada elan mövcud deyil.";

                var sb2 = new StringBuilder();
                sb2.AppendLine("Müqayisə üçün iki marka adı yazın. Məsələn: 'BMW ilə Mercedes müqayisə et'.");
                sb2.AppendLine();
                sb2.AppendLine("Platformamızda mövcud olan markalar:");
                var brandNames = topCars.Select(c => c.Brand.Name).Distinct();
                foreach (var bn in brandNames)
                    sb2.AppendLine($"  • {bn}");
                return sb2.ToString();
            }

            var sb = new StringBuilder();
            sb.AppendLine($"{matchedBrands[0].Name} vs {matchedBrands[1].Name} müqayisəsi:");
            sb.AppendLine();

            foreach (var brand in matchedBrands)
            {
                var brandCars = await _db.Cars.AsNoTracking()
                    .Where(c => c.IsApproved && c.BrandId == brand.Id)
                    .OrderBy(c => c.Price)
                    .Take(3)
                    .ToListAsync(ct);

                sb.AppendLine($"--- {brand.Name} ({brandCars.Count} elan göstərilir) ---");
                if (brandCars.Count == 0)
                {
                    sb.AppendLine("  Hal-hazırda bu markadan elan yoxdur.");
                }
                else
                {
                    foreach (var car in brandCars)
                    {
                        sb.AppendLine($"  • {car.Title} ({car.Year}) — {car.Price:N0} AZN | {car.FuelType} | {car.Transmission} | {car.Mileage:N0} km");
                    }
                }
                sb.AppendLine();
            }

            sb.AppendLine("Daha ətraflı müqayisə üçün saytdakı 'Compare' bölməsindən istifadə edə bilərsiniz.");
            return sb.ToString();
        }

        /// <summary>
        /// Qiymət əsaslı cavab — ən ucuz / ən bahalı / büdcəyə uyğun maşınlar.
        /// </summary>
        private async Task<string> BuildPriceResponseAsync(string msg, CancellationToken ct)
        {
            var query = _db.Cars.AsNoTracking()
                .Where(c => c.IsApproved)
                .Include(c => c.Brand);

            decimal? budget = ExtractBudget(msg);

            List<Models.Car> cars;
            string title;

            if (msg.Contains("ucuz") || msg.Contains("cheap") || msg.Contains("əlverişli"))
            {
                cars = await query.OrderBy(c => c.Price).Take(5).ToListAsync(ct);
                title = "Ən uyğun qiymətli avtomobillər:";
            }
            else if (msg.Contains("bahalı") || msg.Contains("bahali") || msg.Contains("expensive") || msg.Contains("premium") || msg.Contains("lüks"))
            {
                cars = await query.OrderByDescending(c => c.Price).Take(5).ToListAsync(ct);
                title = "Ən premium avtomobillər:";
            }
            else if (budget.HasValue)
            {
                cars = await query.Where(c => c.Price <= budget.Value).OrderByDescending(c => c.Price).Take(5).ToListAsync(ct);
                title = $"{budget.Value:N0} AZN büdcəyə uyğun avtomobillər:";
            }
            else
            {
                // Orta qiymət statistikası göstər
                var allCars = await query.ToListAsync(ct);
                if (allCars.Count == 0)
                    return "Hal-hazırda platformada aktiv elan yoxdur.";

                return $"Platformamızdakı qiymət aralığı:\n" +
                       $"  Ən ucuz: {allCars.Min(c => c.Price):N0} AZN\n" +
                       $"  Ən bahalı: {allCars.Max(c => c.Price):N0} AZN\n" +
                       $"  Orta qiymət: {allCars.Average(c => c.Price):N0} AZN\n" +
                       $"  Cəmi {allCars.Count} elan mövcuddur.\n\n" +
                       "Büdcənizi yazın, sizə uyğun maşınları göstərim. Məsələn: 'Büdcəm 20000 AZN'";
            }

            if (cars.Count == 0)
                return budget.HasValue
                    ? $"{budget.Value:N0} AZN büdcəyə uyğun elan tapılmadı."
                    : "Bu kriterlərə uyğun elan tapılmadı.";

            var sb = new StringBuilder();
            sb.AppendLine(title);
            sb.AppendLine();
            foreach (var car in cars)
            {
                sb.AppendLine($"• {car.Brand.Name} {car.Title} ({car.Year}) — {car.Price:N0} AZN | {car.FuelType} | {car.Mileage:N0} km");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Marka əsaslı cavab — konkret markadan olan maşınları göstərir.
        /// </summary>
        private async Task<string> BuildBrandResponseAsync(string msg, CancellationToken ct)
        {
            var brands = await _db.Brands.AsNoTracking().ToListAsync(ct);
            var matchedBrand = brands.FirstOrDefault(b => msg.Contains(b.Name.ToLowerInvariant()));

            if (matchedBrand == null)
            {
                var allBrandNames = brands.Select(b => b.Name).ToList();
                return $"Platformamızda mövcud markalar: {string.Join(", ", allBrandNames)}.\n\nHansı marka haqqında məlumat istəyirsiniz?";
            }

            var cars = await _db.Cars.AsNoTracking()
                .Where(c => c.IsApproved && c.BrandId == matchedBrand.Id)
                .OrderBy(c => c.Price)
                .Take(8)
                .ToListAsync(ct);

            if (cars.Count == 0)
                return $"Hal-hazırda {matchedBrand.Name} markasından aktiv elan yoxdur.";

            var sb = new StringBuilder();
            sb.AppendLine($"{matchedBrand.Name} markasından mövcud elanlar ({cars.Count} göstərilir):");
            sb.AppendLine();
            foreach (var car in cars)
            {
                sb.AppendLine($"• {car.Title} ({car.Year}) — {car.Price:N0} AZN | {car.FuelType} | {car.Transmission} | {car.Mileage:N0} km");
            }
            sb.AppendLine();
            sb.AppendLine("Daha çox elan üçün 'List' bölməsindən marka filtrini seçə bilərsiniz.");
            return sb.ToString();
        }

        /// <summary>
        /// Yanacaq növü əsaslı cavab.
        /// </summary>
        private async Task<string> BuildFuelTypeResponseAsync(string msg, CancellationToken ct)
        {
            Models.FuelType? fuelType = null;
            string fuelName = "";

            if (msg.Contains("elektrik") || msg.Contains("electric"))
            { fuelType = Models.FuelType.Electric; fuelName = "Elektrik"; }
            else if (msg.Contains("hybrid") || msg.Contains("hibrid"))
            { fuelType = Models.FuelType.Hybrid; fuelName = "Hibrid"; }
            else if (msg.Contains("dizel") || msg.Contains("diesel"))
            { fuelType = Models.FuelType.Diesel; fuelName = "Dizel"; }
            else if (msg.Contains("benzin") || msg.Contains("petrol"))
            { fuelType = Models.FuelType.Petrol; fuelName = "Benzin"; }

            if (fuelType == null)
                return "Yanacaq növünü dəqiqləşdirin: benzin, dizel, elektrik və ya hibrid.";

            var cars = await _db.Cars.AsNoTracking()
                .Where(c => c.IsApproved && c.FuelType == fuelType.Value)
                .Include(c => c.Brand)
                .OrderBy(c => c.Price)
                .Take(6)
                .ToListAsync(ct);

            if (cars.Count == 0)
                return $"Hal-hazırda {fuelName} yanacaqlı avtomobil elanı mövcud deyil.";

            var sb = new StringBuilder();
            sb.AppendLine($"{fuelName} yanacaqlı avtomobillər ({cars.Count} göstərilir):");
            sb.AppendLine();
            foreach (var car in cars)
            {
                sb.AppendLine($"• {car.Brand.Name} {car.Title} ({car.Year}) — {car.Price:N0} AZN | {car.Transmission} | {car.Mileage:N0} km");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Ümumi inventar icmalı.
        /// </summary>
        private async Task<string> BuildInventoryOverviewAsync(CancellationToken ct)
        {
            var cars = await _db.Cars.AsNoTracking()
                .Where(c => c.IsApproved)
                .Include(c => c.Brand)
                .ToListAsync(ct);

            if (cars.Count == 0)
                return "Hal-hazırda platformada aktiv elan yoxdur.";

            var sb = new StringBuilder();
            sb.AppendLine($"Platformamızda cəmi {cars.Count} aktiv elan var:");
            sb.AppendLine();

            var brandGroups = cars.GroupBy(c => c.Brand.Name).OrderByDescending(g => g.Count());
            foreach (var group in brandGroups)
            {
                var minPrice = group.Min(c => c.Price);
                var maxPrice = group.Max(c => c.Price);
                sb.AppendLine($"• {group.Key}: {group.Count()} elan ({minPrice:N0} — {maxPrice:N0} AZN)");
            }

            sb.AppendLine();
            sb.AppendLine($"Qiymət aralığı: {cars.Min(c => c.Price):N0} — {cars.Max(c => c.Price):N0} AZN");
            sb.AppendLine();
            sb.AppendLine("Hansı marka və ya büdcə aralığı sizi maraqlandırır?");
            return sb.ToString();
        }

        /// <summary>
        /// Mesajda marka adının olub-olmadığını yoxlayır.
        /// </summary>
        private async Task<bool> IsCarBrandQueryAsync(string msg, CancellationToken ct)
        {
            if (msg.Contains("marka") || msg.Contains("brand"))
                return true;

            var brands = await _db.Brands.AsNoTracking().Select(b => b.Name).ToListAsync(ct);
            return brands.Any(b => msg.Contains(b.ToLowerInvariant()));
        }

        /// <summary>
        /// Mesajdan büdcə rəqəmini çıxarır. Məs: "büdcəm 20000", "15000 azn", "10k" kimi.
        /// </summary>
        private static decimal? ExtractBudget(string msg)
        {
            // "XXk" formatı (10k, 20k, 15k ...)
            var kMatch = System.Text.RegularExpressions.Regex.Match(msg, @"(\d+)\s*k\b");
            if (kMatch.Success && decimal.TryParse(kMatch.Groups[1].Value, out var kVal))
                return kVal * 1000;

            // Adi rəqəm formatı (5000, 20000, 150000 ...)
            var numMatch = System.Text.RegularExpressions.Regex.Match(msg, @"(\d{4,7})");
            if (numMatch.Success && decimal.TryParse(numMatch.Groups[1].Value, out var numVal))
                return numVal;

            return null;
        }
    }
}
