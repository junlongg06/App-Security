using App_Security_Assignment.Models;
using App_Security_Assignment.Data;
using App_Security_Assignment.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace App_Security_Assignment.Controllers
{
    [Authorize] // Ensures only authenticated users can access Home
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            // 🔹 Ensure Session Exists
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("❌ Session Expired! Redirecting to Login.");
                return RedirectToAction("Login", "Account");
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.FirstName = customer.FirstName;
            ViewBag.LastName = customer.LastName;
            ViewBag.Email = customer.Email;
            ViewBag.MobileNo = customer.MobileNo;
            ViewBag.BillingAddress = customer.BillingAddress;
            ViewBag.ShippingAddress = customer.ShippingAddress;
            ViewBag.EncryptedCreditCard = customer.EncryptedCreditCard;
            ViewBag.EncryptedPassword = customer.PasswordHash;  // ✅ Keep encrypted password for display
            ViewBag.ProfilePicture = string.IsNullOrEmpty(customer.ProfilePicture)
                ? Url.Content("~/images/default-profile.jpg")
                : Url.Content("~/uploads/" + Path.GetFileName(customer.ProfilePicture));

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                _logger.LogInformation($"✅ {user.Email} logged out.");

                // Audit Log: User Logged Out
                _context.AuditLogs.Add(new AuditLog
                {
                    Email = user.Email,
                    Action = "Logged Out",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
