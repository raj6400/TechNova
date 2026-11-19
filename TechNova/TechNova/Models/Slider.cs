using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace TechNova.Models
{
    // Database table name: Sliders
    [Table("Sliders")]
    public class Slider
    {
        [Key]
        public int Id { get; set; }

        [StringLength(150)]
        [Display(Name = "Title")]
        public string? Title { get; set; }

        [StringLength(300)]
        [Display(Name = "Sub Title")]
        public string? SubTitle { get; set; }

        // Saved image path, example: /images/sliders/xxxx.jpg
        [StringLength(255)]
        public string ImagePath { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Button Text")]
        public string? ButtonText { get; set; }

        [StringLength(255)]
        [Display(Name = "Button Link")]
        public string? ButtonLink { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Only used for upload, NOT stored in database
        [NotMapped]
        [Display(Name = "Slider Image")]
        public IFormFile? ImageFile { get; set; }
    }
}
