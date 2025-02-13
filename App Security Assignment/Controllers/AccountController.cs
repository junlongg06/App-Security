using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using App_Security_Assignment.Data;
using App_Security_Assignment.Models;
using App_Security_Assignment.Models.ViewModels;
using App_Security_Assignment.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace App_Security_Assignment.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;

        public AccountController(ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            ILogger<AccountController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _logger = logger;
        }

        private string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return Regex.Replace(input, "[\r\n\t]", " "); // Remove newline and tab characters
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (await _userManager.IsLockedOutAsync(user))
                {
                    _logger.LogWarning("❌ User {Email} is locked out.", SanitizeInput(model.Email));
                    TempData["LockoutMessage"] = "Your account is locked due to multiple failed login attempts. Try again later.";
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("✅ User {Email} logged in successfully.", SanitizeInput(model.Email));

                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email);
                    if (customer != null)
                    {
                        HttpContext.Session.SetString("UserEmail", customer.Email);
                        HttpContext.Session.SetString("FirstName", customer.FirstName);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    var attemptsLeft = _userManager.Options.Lockout.MaxFailedAccessAttempts - await _userManager.GetAccessFailedCountAsync(user);
                    TempData["AttemptsLeft"] = attemptsLeft;

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("❌ User {Email} locked out.", SanitizeInput(model.Email));
                        TempData["LockoutMessage"] = "Your account has been locked due to too many failed attempts.";
                    }
                    else
                    {
                        _logger.LogWarning("❌ Invalid login attempt for {Email}. {AttemptsLeft} attempts left.", SanitizeInput(model.Email), attemptsLeft);
                        ModelState.AddModelError("", "Invalid login attempt.");
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            _logger.LogInformation("🚀 Register method called! Received Email: {Email}", SanitizeInput(model.Email));

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("❌ ModelState is invalid. Errors:");

                foreach (var key in ModelState.Keys)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        _logger.LogWarning("❌ {Key}: {ErrorMessage}", key, error.ErrorMessage);
                    }
                }
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("❌ Email already exists.");
                ModelState.AddModelError("Email", "Email already in use.");
                return View(model);
            }

            string encryptedCreditCard = EncryptionHelper.Encrypt(model.CreditCardNo);

            string profilePicPath = null;
            if (model.ProfilePicture != null)
            {
                if (Path.GetExtension(model.ProfilePicture.FileName).ToLower() != ".jpg")
                {
                    _logger.LogWarning("❌ Invalid file type.");
                    ModelState.AddModelError("ProfilePicture", "Only .JPG images are allowed.");
                    return View(model);
                }

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                profilePicPath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + ".jpg");
                using (var fileStream = new FileStream(profilePicPath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(fileStream);
                }
                _logger.LogInformation("✅ Profile Picture Saved!");
            }

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("✅ User created successfully. Adding customer to DB...");

                string encryptedPassword = EncryptionHelper.Encrypt(model.Password);

                var customer = new Customer
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    MobileNo = model.MobileNo,
                    BillingAddress = model.BillingAddress,
                    ShippingAddress = model.ShippingAddress,
                    Email = model.Email,
                    PasswordHash = encryptedPassword,
                    EncryptedCreditCard = encryptedCreditCard,
                    ProfilePicture = profilePicPath
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Customer data saved to database!");

                return RedirectToAction("Login", "Account");
            }

            _logger.LogError("❌ User creation failed.");
            foreach (var error in result.Errors)
            {
                _logger.LogError("❌ Identity Error: {ErrorDescription}", error.Description);
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }
}
