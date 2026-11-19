using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;
using TechNova.Services;
using System.Diagnostics;

namespace TechNova.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            IProductService productService,
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _productService = productService;
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                Sliders = await _context.Sliders
                    .AsNoTracking()
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Id)
                    .Take(5)
                    .ToListAsync(),

                FeaturedProducts = await _productService.GetFeaturedProductsAsync(8),
                NewArrivals = await _productService.GetNewArrivalsAsync(8),
                Bestsellers = await _productService.GetBestsellersAsync(8),
                Brands = await _productService.GetBrandsAsync(6)
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }

    public class HomeViewModel
    {
        public List<Slider> Sliders { get; set; } = new List<Slider>();

        public List<Product> FeaturedProducts { get; set; } = new List<Product>();

        public List<Product> NewArrivals { get; set; } = new List<Product>();

        public List<Product> Bestsellers { get; set; } = new List<Product>();

        public List<Brand> Brands { get; set; } = new List<Brand>();
    }
}