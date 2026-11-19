# TechNova - Modern Smartphone & Technology E-Commerce Platform

![TechNova Logo](https://img.shields.io/badge/TechNova-v1.0-blue)
![.NET Version](https://img.shields.io/badge/.NET-9.0-blue)
![License](https://img.shields.io/badge/License-MIT-green)

**TechNova** is a modern, full-featured e-commerce platform built with **ASP.NET Core MVC** and **.NET 9** that specializes in smartphones and latest technology products. It provides a seamless shopping experience for customers and comprehensive management tools for administrators.

---

## 🎯 Features Overview

### 🛍️ Customer Features
- **Product Browsing**: Browse latest smartphones and tech products with advanced filtering
- **Search & Filter**: Search by name, filter by brand, category, and price range
- **Product Comparison**: Compare multiple products side-by-side
- **Shopping Cart**: Add/remove items with real-time updates
- **Wishlist**: Save favorite products for later
- **Order Tracking**: Track your orders in real-time
- **User Reviews**: Read and write product reviews
- **User Accounts**: Create accounts, manage profiles, and view order history
- **Secure Checkout**: Safe payment processing with multiple payment methods
- **Responsive Design**: Works seamlessly on desktop and mobile devices

### 🏢 Admin Features
- **Dashboard**: Comprehensive overview of store performance
- **Product Management**: Add, edit, delete products with detailed specifications
- **Inventory Management**: Track stock levels and low-stock alerts
- **Category Management**: Organize products into categories
- **Brand Management**: Manage product brands
- **Order Management**: View and manage customer orders
- **Sales Reports**: Detailed sales analytics and reporting
- **User Management**: Manage customer accounts
- **Slider Management**: Control homepage promotional sliders
- **Invoice Generation**: Automated PDF invoice generation

---

## 📸 Screenshots

### Home Page - Featured Slider
![TechNova Home Page](./screenshots/home-page.png)
*Homepage with promotional slider showcasing latest smartphone models*

### Products Listing Page
![Products Catalog](./screenshots/products-page.png)
*Advanced product filtering and browsing with search functionality*

### Admin Dashboard
![Admin Dashboard](./screenshots/admin-dashboard.png)
*Comprehensive admin panel with sales analytics and performance metrics*

---

## 🛠️ Tech Stack

### Frontend
- **HTML5** - Semantic markup
- **CSS3** - Bootstrap 5.3.2 framework for responsive design
- **Bootstrap Icons** - Modern icon library
- **JavaScript** - jQuery for dynamic interactions
- **jQuery Validation** - Client-side form validation
- **Razor Pages/Views** - Dynamic content rendering

### Backend
- **.NET 9 (LTS)** - Latest .NET framework
- **ASP.NET Core MVC** - Web framework
- **Entity Framework Core** - ORM for database operations
- **MySQL** - Primary database
- **QuestPDF** - PDF generation for invoices

### Additional Libraries
- **Microsoft.AspNetCore.Identity** - User authentication & authorization
- **Stripe** - Payment processing
- **SMTP (Gmail)** - Email notifications

---

## 📋 System Requirements

- **Operating System**: Windows, Linux, or macOS
- **.NET Runtime**: .NET 9 or higher
- **Database**: MySQL 8.0+
- **Visual Studio**: 2022 or higher (Community Edition supported)
- **RAM**: Minimum 4GB
- **Disk Space**: Minimum 500MB

---

## ⚙️ Installation & Setup

### Prerequisites
1. Install [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Install [MySQL Server](https://dev.mysql.com/downloads/mysql/)
3. Install [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/) or [VS Code](https://code.visualstudio.com/)

### Step-by-Step Setup

#### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/TechNova.git
cd TechNova
```

#### 2. Configure Database Connection
Open `appsettings.json` and update the connection string:
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=localhost;Database=technovadb;User=root;Password=your_password;Port=3306"
  }
}
```

#### 3. Configure Email (SMTP)
Update SMTP settings in `appsettings.json`:
```json
{
  "Smtp": {
	"Host": "smtp.gmail.com",
	"Port": 587,
	"FromName": "TechNova",
	"Username": "your-email@gmail.com",
	"Password": "your-app-password"
  }
}
```

#### 4. Configure Stripe Payment
Add your Stripe keys to `appsettings.json`:
```json
{
  "Stripe": {
	"PublishableKey": "pk_test_...",
	"SecretKey": "sk_test_..."
  }
}
```

#### 5. Create Database & Apply Migrations
```bash
cd TechNova/TechNova
dotnet ef database update
```

#### 6. Run the Application
```bash
dotnet run --project TechNova/TechNova/TechNova.csproj
```

The application will be available at: `https://localhost:7101`

---

## 🗂️ Project Structure

```
TechNova/
├── Areas/
│   └── Admin/                    # Admin panel controllers and views
│       ├── Controllers/          # Admin management controllers
│       ├── Views/               # Admin dashboard and management pages
│       └── ...
├── Controllers/                  # Main application controllers
│   ├── HomeController.cs
│   ├── ProductsController.cs
│   ├── ShoppingCartController.cs
│   ├── CheckoutController.cs
│   └── ...
├── Models/                       # Data models
│   ├── Product.cs
│   ├── Order.cs
│   ├── User.cs
│   ├── Brand.cs
│   ├── Category.cs
│   └── ...
├── Views/                        # Razor views
│   ├── Home/
│   ├── Products/
│   ├── Checkout/
│   ├── Account/
│   ├── Shared/
│   │   ├── _Layout.cshtml       # Main layout template
│   │   └── ...
│   └── ...
├── Services/                     # Business logic services
│   ├── IShoppingCartService.cs
│   ├── IProductService.cs
│   ├── IOrderService.cs
│   ├── OrderInvoicePdfService.cs
│   └── ...
├── Data/                         # Database context & migrations
│   ├── ApplicationDbContext.cs
│   ├── Migrations/
│   └── DbInitializer.cs
├── wwwroot/                      # Static files
│   ├── css/                      # Stylesheets
│   ├── js/                       # JavaScript files
│   ├── images/                   # Product and category images
│   └── lib/                      # Third-party libraries
├── ViewComponents/               # Reusable view components
│   └── BrandsMenuViewComponent.cs
├── ViewModels/                   # View-specific models
│   ├── ProductListViewModel.cs
│   ├── CheckoutViewModel.cs
│   └── ...
├── appsettings.json             # Configuration settings
├── Program.cs                    # Application startup configuration
└── README.md                     # This file
```

---

## 🔐 Default Admin Credentials

After initial setup, you can create an admin account through the registration page or use:
- **Email**: admin@technova.com
- **Password**: Admin@123456

> **Important**: Change the default admin password immediately after first login!

---

## 📱 Key Pages & Routes

### Customer Pages
| Route | Description |
|-------|-------------|
| `/` | Home page with featured products |
| `/Products` | Browse all products |
| `/Products/Details/{id}` | Product details page |
| `/ShoppingCart` | Shopping cart |
| `/Checkout` | Checkout process |
| `/Account/Login` | User login |
| `/Account/Register` | User registration |
| `/Account/Profile` | User profile |
| `/Account/Orders` | Order history |
| `/Account/Wishlist` | Wishlist |
| `/Checkout/TrackOrder` | Order tracking |

### Admin Pages
| Route | Description |
|-------|-------------|
| `/Admin/Dashboard` | Admin dashboard |
| `/Admin/Products` | Product management |
| `/Admin/Categories` | Category management |
| `/Admin/Brands` | Brand management |
| `/Admin/Orders` | Order management |
| `/Admin/Users` | User management |
| `/Admin/Sliders` | Slider management |
| `/Admin/Dashboard/SalesReport` | Sales reports |

---

## 💳 Payment Integration

TechNova integrates with **Stripe** for secure payment processing:
- Credit/Debit Card payments
- Multiple currency support
- PCI DSS compliant
- Automated invoice generation

---

## 📧 Email Notifications

The system sends automatic emails for:
- Order confirmation
- Order status updates
- Password reset requests
- Account verification

---

## 🔄 Database Schema Highlights

### Key Tables
- **Products** - Product catalog
- **Categories** - Product categories
- **Brands** - Manufacturer brands
- **Orders** - Customer orders
- **OrderItems** - Order line items
- **AspNetUsers** - User accounts
- **ShoppingCartItems** - Shopping cart items
- **WishlistItems** - Wishlist items
- **Reviews** - Product reviews
- **Sliders** - Homepage slider images

---

## 🚀 Deployment

### Publishing for Production

```bash
# Build release version
dotnet build -c Release

# Publish to folder
dotnet publish -c Release -o ./publish

# Deploy to hosting platform (Azure, AWS, etc.)
```

### Environment Configuration
Update `appsettings.Production.json` with production settings before deployment.

---

## 🤝 Contributing

We welcome contributions! To contribute:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request

### Coding Standards
- Follow C# naming conventions (PascalCase for classes, camelCase for variables)
- Write meaningful comments for complex logic
- Include XML documentation for public methods
- Ensure code compiles without warnings

---

## 🐛 Bug Reporting & Issues

Found a bug? Please report it by:
1. Going to the [Issues](https://github.com/yourusername/TechNova/issues) page
2. Click "New Issue"
3. Provide detailed description and steps to reproduce
4. Add screenshots if applicable

---

## 📝 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 👥 Authors & Credits

- **Project Lead**: Your Name
- **Contributors**: [List contributors here]
- **Inspired by**: Modern e-commerce platforms

---

## 📞 Support & Contact

- **Email**: support@technova.com
- **Website**: https://www.technova.com
- **Documentation**: [Wiki](https://github.com/yourusername/TechNova/wiki)
- **Issues**: [GitHub Issues](https://github.com/yourusername/TechNova/issues)

---

## 🗺️ Roadmap

### Version 1.0 (Current)
- ✅ Core e-commerce functionality
- ✅ Product catalog
- ✅ Shopping cart & checkout
- ✅ Admin dashboard
- ✅ Order management

### Version 1.1 (Planned)
- 🔄 Mobile app (iOS/Android)
- 🔄 Advanced analytics
- 🔄 AI-powered recommendations
- 🔄 Multi-language support

### Version 2.0 (Future)
- 🔄 Marketplace features
- 🔄 Vendor management
- 🔄 Advanced inventory tracking
- 🔄 Integration with supply chain tools

---

## 📚 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Docs](https://docs.microsoft.com/en-us/ef/core/)
- [Bootstrap Documentation](https://getbootstrap.com/docs/)
- [Stripe Integration Guide](https://stripe.com/docs)

---

## ⭐ Show Your Support

If you find this project helpful, please consider:
- **Starring** the repository ⭐
- **Sharing** it with others 📢
- **Contributing** to the project 🤝
- **Reporting** issues and suggestions 💡

---

<div align="center">

**Made with ❤️ by the TechNova Team**

[⬆ Back to Top](#technova---modern-smartphone--technology-e-commerce-platform)

</div>
