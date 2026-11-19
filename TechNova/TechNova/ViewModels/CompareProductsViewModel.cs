using TechNova.Models;

namespace TechNova.ViewModels
{
    public class CompareProductsViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<string> SpecificationNames { get; set; } = new();
    }
}
