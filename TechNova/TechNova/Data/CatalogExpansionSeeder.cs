
using Microsoft.EntityFrameworkCore;
using TechNova.Models;

namespace TechNova.Data;

internal static class CatalogExpansionSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // ==========================================
        // 1. CATEGORIES
        // ==========================================

        var categories = new Dictionary<string,
            (string Description, string ImageUrl, int DisplayOrder)>
            (StringComparer.OrdinalIgnoreCase)
        {
            ["Laptops"] = ("Thin-and-light notebooks, professional workstations, and gaming laptops.", "/images/Products/11_pro.webp", 5),
            ["Smartphones"] = ("Flagship and everyday smartphones with premium displays, cameras, and performance.", "/images/Products/11_pro.jpg", 5),
            ["Desktops"] = ("Powerful desktop computers for home, office, and creative work.", "/images/Products/18_pro.webp", 6),
            ["Monitors"] = ("High-resolution displays for productivity, gaming, and content creation.", "/images/Products/26.webp", 7),
            ["Audio"] = ("Headphones, earbuds, speakers, and audio accessories.", "/images/Products/duo.jpg", 8),
            ["Televisions"] = ("Smart TVs and home-theater displays for immersive entertainment.", "/images/Products/18_pro_max.webp", 9),
            ["Smartwatches"] = ("Connected watches for health, fitness, communication, and everyday use.", "/images/Products/11_pro_xl.webp", 10),
            ["Cameras"] = ("Digital cameras and creator equipment for photos and video.", "/images/Products/26_ultra.webp", 11),
            ["Accessories"] = ("Productivity essentials, peripherals, and everyday tech add-ons.", "/images/Products/26.webp", 12),
            ["Gaming"] = ("Gaming consoles, controllers, and performance accessories.", "/images/Products/26_+.webp", 13)
        };

        var existingCategoryNames = await context.Categories
            .Select(category => category.Name)
            .ToListAsync();

        var categoriesToAdd = categories
            .Where(item => !existingCategoryNames.Contains(
                item.Key, StringComparer.OrdinalIgnoreCase))
            .Select(item => new Category
            {
                Name = item.Key,
                Description = item.Value.Description,
                ImageUrl = item.Value.ImageUrl,
                DisplayOrder = item.Value.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.Now
            })
            .ToList();

        if (categoriesToAdd.Count > 0)
        {
            await context.Categories.AddRangeAsync(categoriesToAdd);
            await context.SaveChangesAsync();
        }

        // ==========================================
        // 2. BRANDS
        // ==========================================

        var brands = new Dictionary<string,
            (string Description, string Country, string Website)>
            (StringComparer.OrdinalIgnoreCase)
        {
            ["Apple"] = ("Premium smartphones, laptops, wearables, and accessories designed for a seamless ecosystem.", "USA", "https://www.apple.com/in"),
            ["Dell"] = ("Business laptops, desktops, monitors, and professional computing solutions.", "USA", "https://www.dell.com"),
            ["Lenovo"] = ("ThinkPad business computers, Yoga convertibles, and Legion gaming hardware.", "China", "https://www.lenovo.com"),
            ["HP"] = ("Reliable personal computers, printers, and accessories for work and home.", "USA", "https://www.hp.com"),
            ["ASUS"] = ("Innovative laptops, displays, components, and gaming devices.", "Taiwan", "https://www.asus.com"),
            ["Acer"] = ("Affordable laptops, monitors, and gaming computers for every lifestyle.", "Taiwan", "https://www.acer.com"),
            ["Sony"] = ("Premium cameras, headphones, televisions, and entertainment products.", "Japan", "https://www.sony.co.in"),
            ["LG"] = ("OLED televisions, monitors, appliances, and smart electronics.", "South Korea", "https://www.lg.com/in"),
            ["JBL"] = ("Wireless speakers and headphones with powerful, portable sound.", "USA", "https://in.jbl.com"),
            ["Canon"] = ("Digital cameras, lenses, and imaging solutions for creators.", "Japan", "https://in.canon"),
            ["Logitech"] = ("Keyboards, mice, webcams, and accessories for work and play.", "Switzerland", "https://www.logitech.com"),
            ["Nothing"] = ("Distinctive smartphones and audio products with a clean design language.", "United Kingdom", "https://in.nothing.tech"),
            ["Nintendo"] = ("Family-friendly consoles and games for shared entertainment.", "Japan", "https://www.nintendo.com"),
            ["Samsung"] = ("Galaxy smartphones, QLED televisions, monitors, and consumer electronics for modern living.", "South Korea", "https://www.samsung.com/in"),
            ["Microsoft"] = ("Surface devices, Xbox gaming consoles, and productivity software for work and entertainment.", "USA", "https://www.microsoft.com/en-in")
        };

        var existingBrandNames = await context.Brands
            .Select(brand => brand.Name)
            .ToListAsync();

        var brandsToAdd = brands
            .Where(item => !existingBrandNames.Contains(
                item.Key, StringComparer.OrdinalIgnoreCase))
            .Select(item => new Brand
            {
                Name = item.Key,
                Description = item.Value.Description,
                Country = item.Value.Country,
                WebsiteUrl = item.Value.Website,
                IsActive = true,
                CreatedAt = DateTime.Now
            })
            .ToList();

        if (brandsToAdd.Count > 0)
        {
            await context.Brands.AddRangeAsync(brandsToAdd);
            await context.SaveChangesAsync();
        }

        // ==========================================
        // 3. GET CATEGORY IDS (DUPLICATE SAFE)
        // ==========================================

        var existingCategories = await context.Categories
            .ToListAsync();

        var categoryIds = existingCategories
            .GroupBy(
                category => category.Name,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First().Id,
                StringComparer.OrdinalIgnoreCase);

        // ==========================================
        // 4. GET BRAND IDS (DUPLICATE SAFE)
        // ==========================================

        var existingBrands = await context.Brands
            .ToListAsync();

        var brandIds = existingBrands
            .GroupBy(
                brand => brand.Name,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First().Id,
                StringComparer.OrdinalIgnoreCase);

        // ==========================================
        // 5. PRODUCTS
        // ==========================================

        var products = new[]
        {
            new Product
            {
                Name = "Apple MacBook Air M3 15-inch",
                Model = "MC9A4HN/A",
                Description = "The 15-inch MacBook Air combines the silent power of the Apple M3 chip with an expansive Liquid Retina display, all-day battery life, and a lightweight aluminium design.",
                ShortDescription = "M3 chip, 15.3-inch Liquid Retina display, 16GB RAM, 512GB SSD",
                OriginalPrice = 149900,
                SalePrice = 139990,
                StockQuantity = 18,
                MainImageUrl = "/images/Products/11_pro.webp",
                CategoryId = categoryIds["Laptops"],
                BrandId = brandIds["Apple"],
                IsFeatured = true,
                IsNewArrival = true
            },

            new Product
            {
                Name = "Dell Inspiron 14 Plus",
                Model = "7440",
                Description = "A versatile productivity laptop with a sharp 2.5K display, Intel Core Ultra performance, fast SSD storage, and a comfortable backlit keyboard.",
                ShortDescription = "Core Ultra 7, 16GB RAM, 1TB SSD, 14-inch 2.5K display",
                OriginalPrice = 99990,
                SalePrice = 89990,
                StockQuantity = 22,
                MainImageUrl = "/images/Products/18_pro.webp",
                CategoryId = categoryIds["Laptops"],
                BrandId = brandIds["Dell"],
                IsBestseller = true
            },

            new Product
            {
                Name = "Lenovo Legion 5i Gaming Laptop",
                Model = "83DG00",
                Description = "Built for competitive gaming and demanding creative work with a high-refresh-rate display, dedicated RTX graphics, and advanced cooling.",
                ShortDescription = "Core i7, RTX 4060, 16GB RAM, 1TB SSD, 165Hz display",
                OriginalPrice = 139990,
                SalePrice = 124990,
                StockQuantity = 14,
                MainImageUrl = "/images/Products/26.webp",
                CategoryId = categoryIds["Laptops"],
                BrandId = brandIds["Lenovo"],
                IsFeatured = true
            },

            new Product
            {
                Name = "HP Pavilion Plus 14",
                Model = "14-ew",
                Description = "A premium everyday notebook with an OLED display, modern Intel processor, fast Wi-Fi 6E, and a compact design for work on the move.",
                ShortDescription = "Core Ultra 5, 16GB RAM, 512GB SSD, 14-inch OLED display",
                OriginalPrice = 89990,
                SalePrice = 79990,
                StockQuantity = 20,
                MainImageUrl = "/images/Products/26_+.webp",
                CategoryId = categoryIds["Laptops"],
                BrandId = brandIds["HP"],
                IsNewArrival = true
            },

            new Product
            {
                Name = "ASUS ROG Zephyrus G14",
                Model = "GA403",
                Description = "A compact premium gaming laptop with powerful Ryzen performance, GeForce RTX graphics, and a colour-accurate high-resolution display.",
                ShortDescription = "Ryzen 9, RTX 4070, 32GB RAM, 1TB SSD, 120Hz OLED",
                OriginalPrice = 199990,
                SalePrice = 179990,
                StockQuantity = 8,
                MainImageUrl = "/images/Products/26_ultra.webp",
                CategoryId = categoryIds["Laptops"],
                BrandId = brandIds["ASUS"]
            },

            new Product
            {
                Name = "Nothing Phone (2a) Plus",
                Model = "A142",
                Description = "A distinctive mid-range smartphone with a smooth AMOLED display, capable dual cameras, clean Nothing OS software, and fast charging.",
                ShortDescription = "Dimensity 7350 Pro, 120Hz AMOLED, 50MP dual camera, 5G",
                OriginalPrice = 31999,
                SalePrice = 27999,
                StockQuantity = 35,
                MainImageUrl = "/images/Products/11_pro_fold.webp",
                CategoryId = categoryIds["Smartphones"],
                BrandId = brandIds["Nothing"],
                IsNewArrival = true
            },

            new Product
            {
                Name = "Sony WH-1000XM5 Wireless Headphones",
                Model = "WH1000XM5",
                Description = "Industry-leading noise cancellation, rich high-resolution audio, and all-day comfort make these headphones ideal for travel and focused work.",
                ShortDescription = "Adaptive noise cancellation, 30-hour battery, multipoint Bluetooth",
                OriginalPrice = 34990,
                SalePrice = 29990,
                StockQuantity = 26,
                MainImageUrl = "/images/Products/duo.jpg",
                CategoryId = categoryIds["Audio"],
                BrandId = brandIds["Sony"],
                IsBestseller = true
            },

            new Product
            {
                Name = "LG 55-inch OLED evo 4K Smart TV",
                Model = "OLED55C4",
                Description = "Experience cinematic contrast and vivid colour with a self-lit OLED panel, AI-powered picture processing, Dolby Vision, and a low-latency gaming mode.",
                ShortDescription = "4K OLED evo, Dolby Vision, webOS, 120Hz gaming",
                OriginalPrice = 169990,
                SalePrice = 139990,
                StockQuantity = 10,
                MainImageUrl = "/images/Products/18_pro_max.webp",
                CategoryId = categoryIds["Televisions"],
                BrandId = brandIds["LG"],
                IsFeatured = true
            },

            new Product
            {
                Name = "Apple Watch Series 10 GPS",
                Model = "MWWE3HN/A",
                Description = "A slim, powerful smartwatch with advanced health insights, fitness tracking, safety features, and a bright wide-angle display.",
                ShortDescription = "46mm aluminium case, health tracking, GPS, fast charging",
                OriginalPrice = 46900,
                SalePrice = 42900,
                StockQuantity = 28,
                MainImageUrl = "/images/Products/11_pro_xl.webp",
                CategoryId = categoryIds["Smartwatches"],
                BrandId = brandIds["Apple"]
            },

            new Product
            {
                Name = "Canon EOS R50 Mirrorless Camera",
                Model = "EOSR50",
                Description = "Capture sharp photos and smooth 4K video with a compact mirrorless camera featuring fast autofocus, a flip-out screen, and creator-friendly controls.",
                ShortDescription = "24.2MP APS-C sensor, 4K video, Dual Pixel autofocus",
                OriginalPrice = 74990,
                SalePrice = 67990,
                StockQuantity = 12,
                MainImageUrl = "/images/Products/26_utlra.webp",
                CategoryId = categoryIds["Cameras"],
                BrandId = brandIds["Canon"],
                IsNewArrival = true
            },

            new Product
            {
                Name = "Logitech MX Keys S Combo",
                Model = "920-011598",
                Description = "A refined wireless keyboard and mouse combo for multi-device productivity, featuring quiet typing, smart backlighting, and ergonomic control.",
                ShortDescription = "Wireless keyboard and mouse, multi-device, USB-C charging",
                OriginalPrice = 15995,
                SalePrice = 12995,
                StockQuantity = 42,
                MainImageUrl = "/images/Products/26.png",
                CategoryId = categoryIds["Accessories"],
                BrandId = brandIds["Logitech"],
                IsBestseller = true
            },

            new Product
            {
                Name = "Nintendo Switch OLED Console",
                Model = "HEG-001",
                Description = "Enjoy console-quality gaming at home or on the go with a vibrant OLED screen, flexible play modes, and a library of family-friendly games.",
                ShortDescription = "7-inch OLED display, docked and handheld modes, 64GB storage",
                OriginalPrice = 39999,
                SalePrice = 34999,
                StockQuantity = 16,
                MainImageUrl = "/images/Products/26.avif",
                CategoryId = categoryIds["Gaming"],
                BrandId = brandIds["Nintendo"],
                IsFeatured = true
            }
        };

        // ==========================================
        // 6. PREVENT DUPLICATE PRODUCTS
        // ==========================================

        var existingProductNames = await context.Products
            .Select(product => product.Name)
            .ToListAsync();

        var productsToAdd = products
            .Where(product => !existingProductNames.Contains(
                product.Name,
                StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (productsToAdd.Count > 0)
        {
            await context.Products.AddRangeAsync(productsToAdd);
            await context.SaveChangesAsync();
        }
    }
}
