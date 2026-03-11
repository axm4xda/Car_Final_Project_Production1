using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Car_Project.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Car_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService,
            ILogger<AccountController> logger)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
            _roleManager   = roleManager;
            _emailService  = emailService;
            _logger        = logger;
        }

        private bool IsAjax() =>
            Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        private string GetReturnUrl(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action("Index", "Home")!;

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            var redirect = GetReturnUrl(returnUrl);

            // AgreeToTerms checkbox-u browser göndərmir (unchecked), manual yoxla
            if (!model.AgreeToTerms)
            {
                var msg = "İstifadəçi Şərtlərini qəbul etməlisiniz.";
                if (IsAjax()) return Json(new { success = false, message = msg });
                TempData["AuthError"] = msg;
                TempData["OpenModal"] = "SignUpModal";
                return Redirect(redirect);
            }

            // Rol yoxla - yalnız Agent və User icazəli
            var allowedRoles = new[] { "Agent", "User" };
            if (string.IsNullOrEmpty(model.Role) || !allowedRoles.Contains(model.Role))
                model.Role = "User";

            // AgreeToTerms xətasını ModelState-dən çıxar, çünki yuxarıda yoxladıq
            ModelState.Remove(nameof(model.AgreeToTerms));

            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Məlumatları düzgün daxil edin.";
                if (IsAjax()) return Json(new { success = false, message = firstError });
                TempData["AuthError"] = firstError;
                TempData["OpenModal"] = "SignUpModal";
                return Redirect(redirect);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                var msg = "Bu email artıq qeydə alınmışdır.";
                if (IsAjax()) return Json(new { success = false, message = msg });
                TempData["AuthError"] = msg;
                TempData["OpenModal"] = "SignUpModal";
                return Redirect(redirect);
            }

            var user = new AppUser
            {
                FullName    = model.FullName,
                Email       = model.Email,
                UserName    = model.Email,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                await _signInManager.SignInAsync(user, isPersistent: false);

                // ── Xoş gəldin maili göndər ──────────────────────────────────
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    try
                    {
                        await _emailService.SendWelcomeAsync(user.Email, user.FullName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Welcome email failed for {Email}", user.Email);
                        /* email failure should not block registration */
                    }
                }

                var successMsg = $"Xoş gəldiniz, {user.FullName}! Qeydiyyat uğurlu oldu.";
                if (IsAjax()) return Json(new { success = true, message = successMsg, redirectUrl = redirect });
                TempData["AuthSuccess"] = successMsg;
                return Redirect(redirect);
            }

            // Identity xətalarını Azərbaycan dilinə çevir
            var errors = string.Join(" ", result.Errors.Select(e => TranslateIdentityError(e)));
            if (IsAjax()) return Json(new { success = false, message = errors });
            TempData["AuthError"] = errors;
            TempData["OpenModal"] = "SignUpModal";
            return Redirect(redirect);
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            var redirect = GetReturnUrl(returnUrl);

            if (!ModelState.IsValid)
            {
                var msg = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                if (IsAjax()) return Json(new { success = false, message = msg });
                TempData["AuthError"] = msg;
                TempData["OpenModal"] = "LoginModal";
                return Redirect(redirect);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var successMsg = $"Xoş gəldiniz, {user?.FullName ?? model.Email}!";
                if (IsAjax()) return Json(new { success = true, message = successMsg, redirectUrl = redirect });
                TempData["AuthSuccess"] = successMsg;
                return Redirect(redirect);
            }

            string errorMsg;
            if (result.IsLockedOut)
            {
                errorMsg = "Hesabınız müvəqqəti olaraq bloklanıb. Bir az sonra yenidən cəhd edin.";
            }
            else
            {
                errorMsg = "Email və ya şifrə yanlışdır.";
            }

            if (IsAjax()) return Json(new { success = false, message = errorMsg });
            TempData["AuthError"] = errorMsg;
            TempData["OpenModal"] = "LoginModal";
            return Redirect(redirect);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["AuthSuccess"] = "Uğurla çıxış etdiniz.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRole = roles.FirstOrDefault() ?? "User";

            return View(user);
        }

        // ========== SuperAdmin: İstifadəçi İdarəetmə ==========

        // GET: /Account/ManageUsers
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();

            var userList = new List<UserWithRoleViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserWithRoleViewModel
                {
                    Id        = user.Id,
                    FullName  = user.FullName,
                    Email     = user.Email ?? "",
                    Role      = roles.FirstOrDefault() ?? "User",
                    CreatedDate = user.CreatedDate
                });
            }

            return View(userList);
        }

        // POST: /Account/ChangeUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            var allowedRoles = new[] { "Admin", "Agent", "User" };
            if (!allowedRoles.Contains(newRole))
            {
                TempData["AuthError"] = "Yanlış rol seçimi.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["AuthError"] = "İstifadəçi tapılmadı.";
                return RedirectToAction(nameof(ManageUsers));
            }

            // SuperAdmin rolunu dəyişmək olmaz
            if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                TempData["AuthError"] = "SuperAdmin-in rolunu dəyişmək mümkün deyil.";
                return RedirectToAction(nameof(ManageUsers));
            }

            // Köhnə rolları sil
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Yeni rol təyin et
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["AuthSuccess"] = $"{user.FullName} istifadəçisinə \"{newRole}\" rolu təyin edildi.";
            return RedirectToAction(nameof(ManageUsers));
        }

        // POST: /Account/RemoveUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RemoveUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["AuthError"] = "İstifadəçi tapılmadı.";
                return RedirectToAction(nameof(ManageUsers));
            }

            // SuperAdmin-i silmək olmaz
            if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                TempData["AuthError"] = "SuperAdmin hesabını silmək mümkün deyil.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["AuthSuccess"] = $"{user.FullName} istifadəçisi silindi.";
            }
            else
            {
                TempData["AuthError"] = "İstifadəçini silmək mümkün olmadı.";
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        /// <summary>
        /// Identity xəta mesajlarını Azərbaycan dilinə çevirir.
        /// </summary>
        private static string TranslateIdentityError(IdentityError error)
        {
            return error.Code switch
            {
                "DuplicateEmail"          => "Bu email artıq qeydiyyatdan keçib.",
                "DuplicateUserName"       => "Bu istifadəçi adı artıq mövcuddur.",
                "InvalidEmail"            => "Düzgün email formatı daxil edin.",
                "InvalidUserName"         => "İstifadəçi adı düzgün deyil.",
                "PasswordTooShort"        => "Şifrə minimum 8 simvol olmalıdır.",
                "PasswordRequiresDigit"   => "Şifrədə ən azı bir rəqəm olmalıdır.",
                "PasswordRequiresLower"   => "Şifrədə ən azı bir kiçik hərf olmalıdır.",
                "PasswordRequiresUpper"   => "Şifrədə ən azı bir böyük hərf olmalıdır.",
                "PasswordRequiresNonAlphanumeric" => "Şifrədə ən azı bir xüsusi simvol olmalıdır.",
                "PasswordRequiresUniqueChars"     => "Şifrədə daha çox fərqli simvol olmalıdır.",
                _                         => error.Description
            };
        }

        // GET: /Account/GoogleLogin
        [HttpGet]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                GoogleDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // GET: /Account/GoogleCallback
        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string? returnUrl = null, string? remoteError = null)
        {
            var redirect = GetReturnUrl(returnUrl);

            if (remoteError != null)
            {
                TempData["AuthError"] = $"Google xətası: {remoteError}";
                return Redirect(redirect);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["AuthError"] = "Google məlumatları alına bilmədi. Yenidən cəhd edin.";
                return Redirect(redirect);
            }

            // Mövcud xarici login ilə giriş cəhdi
            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                TempData["AuthSuccess"] = "Google hesabı ilə uğurla daxil oldunuz!";
                return Redirect(redirect);
            }

            string errorMsg;
            if (signInResult.IsLockedOut)
            {
                errorMsg = "Hesabınız müvəqqəti olaraq bloklanıb. Bir az sonra yenidən cəhd edin.";
            }
            else
            {
                errorMsg = "Google ilə giriş uğursuz oldu.";
            }

            TempData["AuthError"] = errorMsg;
            return Redirect(redirect);
        }

        // ── Forgot Password ───────────────────────────────────────────────────

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            // Always return generic success to prevent email enumeration
            var successMsg = "Əgər bu email mövcuddusa, şifrə sıfırlama linki göndərildi.";

            if (string.IsNullOrWhiteSpace(email))
            {
                if (IsAjax()) return Json(new { success = false, message = "Email daxil edin." });
                TempData["AuthError"] = "Email daxil edin.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                if (IsAjax()) return Json(new { success = true, message = successMsg });
                TempData["AuthSuccess"] = successMsg;
                return RedirectToAction("Index", "Home");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account",
                new { email = user.Email, token },
                protocol: Request.Scheme)!;

            try
            {
                await _emailService.SendPasswordResetAsync(user.Email!, user.FullName, resetLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset email failed for {Email}", user.Email);
                /* email failure should not reveal info */
            }

            if (IsAjax()) return Json(new { success = true, message = successMsg });
            TempData["AuthSuccess"] = successMsg;
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/ResetPassword?email=...&token=...
        [HttpGet]
        public IActionResult ResetPassword(string? email, string? token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Index", "Home");

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string token, string password, string confirmPassword)
        {
            ViewBag.Email = email;
            ViewBag.Token = token;

            if (string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Yeni şifrəni daxil edin.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Şifrələr uyğun gəlmir.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Error = "Yanlış link. Yenidən cəhd edin.";
                return View();
            }

            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (result.Succeeded)
            {
                try
                {
                    await _emailService.SendPasswordChangedAsync(user.Email!, user.FullName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Password changed notification email failed for {Email}", user.Email);
                    /* don't block on email failure */
                }

                TempData["AuthSuccess"] = "Şifrəniz uğurla yeniləndi! İndi daxil ola bilərsiniz.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = string.Join(" ", result.Errors.Select(e => TranslateIdentityError(e)));
            return View();
        }
    }
}
