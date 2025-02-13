using System.ComponentModel.DataAnnotations;

namespace App_Security_Assignment.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, RegularExpression(@"^\d{8}$", ErrorMessage = "Mobile No must be exactly 8 digits.")]
        public string MobileNo { get; set; }

        [Required, MaxLength(200)]
        public string BillingAddress { get; set; }

        public string ShippingAddress { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string EncryptedCreditCard { get; set; }

        public string ProfilePicture { get; set; }
    }
}
