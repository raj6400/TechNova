using System.ComponentModel.DataAnnotations;

namespace TechNova.Models
{
    public class ContactInfo
    {
        public int Id { get; set; }

        [Required]
        public string CompanyName { get; set; } = "TechNova";

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? WhatsApp { get; set; }

        public string? MapAddress { get; set; }

        public string? WorkingHours { get; set; }

        public string? Facebook { get; set; }

        public string? Instagram { get; set; }

        public string? Twitter { get; set; }

        public string? LinkedIn { get; set; }

        public string? Description { get; set; }
    }
}