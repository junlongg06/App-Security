using System;
using System.ComponentModel.DataAnnotations;

namespace App_Security_Assignment.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Action { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
