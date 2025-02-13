using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace App_Security_Assignment.Models.ViewModels
{
    public class RegisterViewModel : IValidatableObject
    {
        [Required, MaxLength(50)]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "First Name must contain only letters.")]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "Last Name must contain only letters.")]
        public string LastName { get; set; }

        [Required, RegularExpression(@"^\d{8}$", ErrorMessage = "Mobile No must be exactly 8 digits.")]
        public string MobileNo { get; set; }

        [Required, MaxLength(200)]
        public string BillingAddress { get; set; }

        public string ShippingAddress { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
        public string Password { get; set; }

        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Credit Card Number must be exactly 16 digits.")]
        public string CreditCardNo { get; set; }

        public IFormFile ProfilePicture { get; set; }

        // Additional validation logic (server-side password validation)
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsValidPassword(Password))
            {
                yield return new ValidationResult("Password must include uppercase, lowercase, number, and special character.",
                    new[] { "Password" });
            }
        }

        private bool IsValidPassword(string password)
        {
            if (password.Length < 12) return false;
            return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{12,}$");
        }
    }
}
