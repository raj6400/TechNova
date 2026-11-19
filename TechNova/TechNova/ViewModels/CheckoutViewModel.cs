using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TechNova.Models;

namespace TechNova.ViewModels
{
    public class CheckoutViewModel
    {
        [BindNever]
        public ShoppingCartViewModel Cart { get; set; } = new();

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(256)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        [Display(Name = "Street Address")]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Apartment, Suite, etc.")]
        public string? Address2 { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "State / Province")]
        public string? State { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        [StringLength(50)]
        public string Country { get; set; } = "Pakistan";

        [StringLength(500)]
        [Display(Name = "Order Notes")]
        [DataType(DataType.MultilineText)]
        public string? OrderNotes { get; set; }

        [Required]
        [EnumDataType(typeof(PaymentMethod))]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        [BindNever]
        public bool IsAuthenticated { get; set; }
    }
}
