# TechNova

**TechNova** is an ASP.NET Core MVC e-commerce application for smartphones and technology products. It includes a customer storefront (catalog, cart, wishlist, comparison, checkout and order tracking) and an admin area for managing products, orders, customers, content and reports.

The application targets **.NET 9**, uses **Entity Framework Core with MySQL**, **ASP.NET Core Identity** for authentication, **Stripe Checkout** or cash on delivery for payments, SMTP for email notifications, and QuestPDF for order invoices.

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-0F6CBD?logo=.net&logoColor=white)](https://learn.microsoft.com/aspnet/core/mvc/overview)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-9.0-7C3AED?logo=.net&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![MySQL](https://img.shields.io/badge/MySQL-8%2B-00758F?logo=mysql&logoColor=white)](https://dev.mysql.com/doc/)
[![Stripe](https://img.shields.io/badge/Payments-Stripe-635BFF?logo=stripe&logoColor=white)](https://stripe.com/docs)
[![Bootstrap 5.3.2](https://img.shields.io/badge/Bootstrap-5.3.2-7952B3?logo=bootstrap&logoColor=white)](https://getbootstrap.com/docs/5.3/)

## Contents

- [Project Overview](#project-overview)
- [Key Features](#key-features)
- [Screenshots](#screenshots)
- [Technology Stack](#technology-stack)
- [System Requirements](#system-requirements)
- [Prerequisites](#prerequisites)
- [Installation and Setup](#installation-and-setup)
- [Project Folder Structure](#project-folder-structure)
- [Database Setup and Schema Overview](#database-setup-and-schema-overview)
- [Application Routes](#application-routes)
- [Authentication and Authorization](#authentication-and-authorization)
- [Payment Integration](#payment-integration)
- [Email Notifications](#email-notifications)
- [Admin Panel Information](#admin-panel-information)
- [Build and Deployment](#build-and-deployment)
- [Troubleshooting](#troubleshooting)
- [Contributors](#contributors)
- [License](#license)

## Project Overview

| Aspect | Details |
|---|---|
| Application type | MVC web application with a separate Admin area |
| Target framework | `net9.0` (`TechNova/TechNova/TechNova.csproj`) |
| Solution | `TechNova/TechNova.slnx` |
| Database | MySQL, accessed with Pomelo.EntityFrameworkCore.MySql |
| Identity | ASP.NET Core Identity with `Admin`, `Customer` and `Manager` roles |
| Checkout | Stripe Checkout (card) and Cash on Delivery |
| Currency | Pakistani rupees (`PKR`; displayed as `RS` in the UI) |
| Invoices | PDF generation with QuestPDF under the Community license |

Startup is handled by `Program.cs`. It configures MySQL, Identity, authorization policies, session-based shopping carts, application services, SMTP email and MVC routing. `DbInitializer.SeedAsync` applies pending migrations and seeds roles, an administrator account and the initial catalog at application startup.

## Key Features

### Customer Features

- **Home page** with up to five active promotional slides, featured products, new arrivals, bestsellers and brands.
- **Product catalog** with pagination (12 products per page), category and brand filters, price range filters, and sorting by newest, price or name.
- **Search** by product name, description or brand name, with a JSON autocomplete endpoint.
- **Product details** with specifications, brand/category information, stock status and related products.
- **Product comparison** of two to four products side by side; selections are kept in browser local storage.
- **Shopping cart** stored per session, with quantity updates, 18% tax and shipping of `RS 500` for subtotals up to `RS 50,000` (free above that). The cart is associated with the user account at login.
- **Wishlist** for authenticated users, with add/remove actions.
- **Reviews**: authenticated users can submit ratings and reviews on product pages.
- **Checkout** with shipping and billing details, stock validation, transactional stock reduction and order creation.
- **Order history and details**, PDF invoice download, and order tracking by order number and email address.
- **Contact form** that stores visitor messages for admin review.
- **Account management**: registration, login with remember-me, logout, profile editing, change password, and email-based password reset.

### Admin Features

- **Dashboard** with total orders, revenue, products, customers, pending orders, low-stock products, recent orders, top products and monthly sales.
- **Product management**: list, create, edit, delete, inspect details, add/remove specifications, and import/export catalog data using Excel.
- **Category and brand management** with create, edit and delete actions.
- **Slider management** for homepage promotions, including active/inactive toggling.
- **Order management**: filter/search orders, view details, update status, cancel orders, and view or download invoices.
- **User management**: search customers, view details, toggle account status and change role membership.
- **Reports**: sales, inventory and top-product reports, plus a management overview page.
- **Contact management**: edit store contact information, read messages, mark messages as read and delete messages.

## Screenshots

### Homepage

![TechNova homepage with promotional slider and featured products](https://github.com/user-attachments/assets/766dbc13-24fd-4796-a8ff-77617da60e84)

*Customer homepage showing the TechNova brand, promotional slider and product sections.*

### Products listing

![TechNova product listing page with filtering and search](https://github.com/user-attachments/assets/a0cf135c-074d-4d56-97ce-00ed13982626)

*Catalog page with product search, filters, sorting and pagination.*

### Admin dashboard

![TechNova admin dashboard with sales metrics](https://github.com/user-attachments/assets/7fc9601d-fffa-400e-93fe-8a79064762aa)

*Admin dashboard with order, revenue, product and inventory metrics.*

## Technology Stack

| Layer | Technology | Notes |
|---|---|---|
| Framework | .NET 9, ASP.NET Core MVC | `net9.0` target; controllers, Razor views and areas |
| ORM / data | Entity Framework Core 9, Pomelo.EntityFrameworkCore.MySql 9.0.0 | MySQL provider and EF design-time tools 9.0.1 |
| Authentication | Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.1 | Identity users, roles and EF stores |
| Payments | Stripe.net 43.0.0 | Server-side Stripe Checkout sessions |
| PDF | QuestPDF 2024.12.3 | Community license configured in `Program.cs` |
| Excel | ClosedXML 0.102.1 | Admin product import/export |
| Mapping | AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1 | Registered in `Program.cs` |
| Email | `System.Net.Mail` via `SmtpEmailSender` | SMTP over SSL, port 587 by default |
| Frontend | Razor views, Bootstrap 5.3.2, Bootstrap Icons 1.11.1, jQuery 3.7.1 | Bootstrap and jQuery are loaded from CDNs in `_Layout.cshtml` |
| Static assets | Custom CSS/JavaScript under `wwwroot` | Includes cart, compare and admin UI behavior |

Google and Facebook authentication packages are referenced in the project file, but no external login provider is configured in `Program.cs`.

## System Requirements

- **SDK/runtime:** .NET 9 SDK for development; .NET 9 runtime for hosting.
- **Database:** MySQL 8 or a compatible MariaDB server reachable from the application.
- **Client:** A modern browser with JavaScript enabled. CDN access is needed for Bootstrap, Bootstrap Icons and jQuery.
- **Optional services:** SMTP account for email notifications and a Stripe account for card payments.
- **Hardware:** No special hardware is required; 4 GB RAM and 500 MB of free disk space are a reasonable minimum for local development.

## Prerequisites

1. Install the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) and verify it:

   ```bash
   dotnet --version
   ```

2. Install [MySQL Server](https://dev.mysql.com/downloads/mysql/) and confirm it is running on port `3306` (or note the port you use).
3. Install [Git](https://git-scm.com/downloads).
4. Optionally install [Visual Studio 2022](https://visualstudio.microsoft.com/) with the ASP.NET workload, [VS Code](https://code.visualstudio.com/) with C# Dev Kit, or another .NET IDE.
5. To run EF commands, install the matching EF tool if needed:

   ```bash
   dotnet tool install --global dotnet-ef --version 9.0.1
   ```

## Installation and Setup

All commands below are run from the repository root unless stated otherwise.

### Clone the Repository

```bash
git clone https://github.com/raj6400/TechNova.git
cd TechNova
```

The solution is `TechNova/TechNova.slnx`; the web project and its configuration files are in `TechNova/TechNova/`.

Restore the NuGet packages:

```bash
dotnet restore TechNova/TechNova/TechNova.csproj
```

### Configure the Database

Edit `TechNova/TechNova/appsettings.json`, or add the same keys to `TechNova/TechNova/appsettings.Development.json` for local overrides:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=technovadb;User=<YOUR_MYSQL_USER>;Password=<YOUR_MYSQL_PASSWORD>;Port=3306"
  }
}
```

Create the database if it does not exist:

```bash
mysql -u <YOUR_MYSQL_USER> -p -e "CREATE DATABASE IF NOT EXISTS technovadb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
```

Keep real passwords out of source control. The project has a `UserSecretsId`, so local secrets can be set from the project directory, for example:

```bash
cd TechNova/TechNova
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=technovadb;User=<YOUR_MYSQL_USER>;Password=<YOUR_MYSQL_PASSWORD>;Port=3306"
cd ../..
```

### Configure Email (SMTP)

In the same configuration file, set the SMTP credentials used by `SmtpEmailSender`:

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "FromName": "TechNova",
    "Username": "<YOUR_SMTP_USERNAME>",
    "Password": "<YOUR_SMTP_APP_PASSWORD>"
  }
}
```

`Smtp:From` is optional and falls back to `Smtp:Username`. For Gmail, use an app password rather than the account password. Email is optional for local development, but order confirmations, status updates and password-reset messages cannot be sent without these settings.

### Configure Stripe

Card payments require a Stripe secret key:

```json
{
  "Stripe": {
    "SecretKey": "sk_test_<YOUR_STRIPE_SECRET_KEY>",
    "PublishableKey": "pk_test_<YOUR_STRIPE_PUBLISHABLE_KEY>"
  }
}
```

Only `Stripe:SecretKey` is read by the current checkout code. If it is empty, card payment is disabled and shoppers are directed to Cash on Delivery. Use test keys for development and store production keys outside source control.

### Apply Database Migrations

Run the committed migrations from the repository root:

```bash
dotnet ef database update --project TechNova/TechNova/TechNova.csproj
```

Startup also calls `Database.MigrateAsync()`, so pending migrations are applied when the application starts. The seeder then creates the `Admin`, `Customer` and `Manager` roles, an administrator account and sample catalog data. Change the seeded administrator password immediately after the first login; its initial value is defined in `TechNova/TechNova/Data/DbInitializer.cs` and is intentionally not published here.

### Run the Application

```bash
dotnet run --project TechNova/TechNova/TechNova.csproj
```

By default the HTTPS launch profile uses `https://localhost:7101` and HTTP uses `http://localhost:5059`, as defined in `TechNova/TechNova/Properties/launchSettings.json`. Accept the development certificate prompt on first run if required.

## Project Folder Structure

```text
TechNova/
├── README.md                         # This document
└── TechNova/                         # Solution and application root
    ├── TechNova.slnx                 # Solution file
    └── TechNova/                     # Web project (.csproj lives here)
        ├── Areas/Admin/
        │   ├── Controllers/          # Admin dashboard, catalog, orders, users, sliders, contact
        │   └── Views/                # Admin Razor views and _AdminLayout.cshtml
        ├── Controllers/              # Home, Products, ShoppingCart, Checkout, Account, Contact
        ├── Data/
        │   ├── ApplicationDbContext.cs
        │   ├── DbInitializer.cs
        │   ├── CatalogExpansionSeeder.cs
        │   └── Migrations/           # EF Core migrations and model snapshot
        ├── Models/                   # Product, Order, Category, Brand, Slider, Identity user, etc.
        ├── Services/                 # Cart, product, order, report, file, email and PDF services
        ├── ViewComponents/           # Brands menu component
        ├── ViewModels/               # View-specific models
        ├── Views/                    # Customer-facing Razor views and Shared layout
        ├── wwwroot/
        │   ├── css/                  # Site, admin and contact styles
        │   ├── images/               # Product, profile and slider images
        │   ├── js/                   # Site, compare and admin scripts
        │   └── lib/                  # Third-party static libraries and licenses
        ├── appsettings.json          # Default configuration
        ├── appsettings.Development.json
        ├── Program.cs                # Application startup and service registration
        └── Properties/launchSettings.json
```

## Database Setup and Schema Overview

The context is `TechNova.Data.ApplicationDbContext`, which derives from `IdentityDbContext<ApplicationUser>`. It uses `UseMySql` with automatic server-version detection.

| Area | Tables / entities |
|---|---|
| Catalog | `Categories`, `Brands`, `Products`, `ProductImages`, `ProductSpecifications`, `Reviews` |
| Commerce | `Orders`, `OrderItems`, `ShoppingCartItems`, `WishlistItems` |
| Content | `Sliders`, `ContactInfos`, `ContactMessages` |
| Identity | Standard ASP.NET Identity tables including `AspNetUsers`, `AspNetRoles` and `AspNetUserRoles` |

Notable configuration includes restricted delete behavior from products to categories and brands, a unique index on `Order.OrderNumber`, and indexes on product name/flags, order status/date, reviews and shopping cart IDs.

Migrations, in order:

1. `20261002181345_InitialCreate`
2. `20261102082928_AddCheckoutDetails`
3. `20261119151733_AddSliders`
4. `20261120185026_AddContactModule`

Seed data covers categories, brands, products and the three roles. Catalog seeding is expanded by `CatalogExpansionSeeder`.

## Application Routes

Routes follow ASP.NET Core MVC conventions: the default route is `/{controller}/{action}/{id?}`, and the Admin area uses `/{area}/{controller}/{action}/{id?}` with `Dashboard/Index` as its defaults.

### Customer and shared routes

| Route | Purpose |
|---|---|
| `/` | Home page |
| `/Home/Privacy` | Privacy page |
| `/Products` | Product catalog with filters, sorting and paging |
| `/Products/Search?term=<text>` | JSON search suggestions (minimum two characters) |
| `/Products/Details/{id}` | Product details, reviews and wishlist state |
| `/Products/Compare?ids=1&ids=2` | Compare two to four products |
| `/ShoppingCart` | Cart with quantities and totals |
| `/Checkout` | Checkout form |
| `/Checkout/CompleteStripe?sessionId=<id>` | Stripe payment return and verification |
| `/Checkout/Confirmation` | Order confirmation |
| `/Checkout/TrackOrder` | Track an order by order number and email |
| `/Account/Login`, `/Account/Register` | Authentication |
| `/Account/Profile` | User profile |
| `/Account/Orders`, `/Account/OrderDetails/{id}` | Order history and details |
| `/Account/DownloadInvoice/{id}` | Download an order invoice as PDF |
| `/Account/Wishlist` | Saved products |
| `/Account/ForgotPassword`, `/Account/ResetPassword` | Password reset flow |
| `/Contact` | Contact form |

### Admin routes

All routes below are under the `Admin` area and require the `Admin` role.

| Route | Purpose |
|---|---|
| `/Admin` or `/Admin/Dashboard` | Dashboard overview |
| `/Admin/Dashboard/SalesReport`, `/Admin/Dashboard/InventoryReport`, `/Admin/Dashboard/TopProducts` | Reports |
| `/Admin/Management/Products`, `/Admin/Management/Categories`, `/Admin/Management/Brands`, `/Admin/Management/Orders`, `/Admin/Management/Users`, `/Admin/Management/Reports` | Management landing pages |
| `/Admin/Products` | Product list; `Create`, `Edit/{id}`, `Details/{id}`, `Delete/{id}` for management |
| `/Admin/Categories`, `/Admin/Brands` | Category and brand management |
| `/Admin/Slider` | Homepage slider management (controller name is singular) |
| `/Admin/Orders` | Order list; `Details/{id}`, `UpdateStatus/{id}`, `CancelOrder/{id}`, `Invoice/{id}`, `InvoicePdf/{id}` |
| `/Admin/Users` | Customer list; `Details/{id}`, `ToggleStatus/{id}`, `UpdateRole/{id}` |
| `/Admin/ContactAdmin` | Contact settings and `Messages`; mark read or delete messages |

## Authentication and Authorization

- ASP.NET Core Identity stores users and roles in the same MySQL database through `ApplicationDbContext`.
- Registration creates a user, assigns the `Customer` role, and signs the user in. Login supports remember-me and migrates the session cart to the user account.
- The configured password policy requires at least six characters plus uppercase, lowercase and digit characters. A special character is not required, email confirmation is not required, and lockout on failed login is disabled.
- `Program.cs` defines `AdminOnly` and `CustomerOnly` policies. Every Admin area controller applies the `AdminOnly` policy (the contact admin controller uses the equivalent `Admin` role requirement).
- Password reset uses Identity tokens and sends the reset link by email. No external login flow is wired up in startup, despite the Google/Facebook packages in the project file.

## Payment Integration

Checkout supports two payment methods:

| Method | Implementation |
|---|---|
| Card | Stripe Checkout session with `PaymentMethodTypes = card`, currency `pkr`, server-side session verification and refund handling if stock changes after payment |
| Cash on Delivery | Order is created immediately inside a database transaction after stock validation |

The backend reads `Stripe:SecretKey` only. `Stripe:PublishableKey` exists in configuration but is not used by the current server-side checkout flow. Card payment is unavailable when the secret key is empty. The application does not store card details; payment is handled on Stripe's hosted checkout page. There is no multi-currency or additional gateway support in the current implementation.

## Email Notifications

`SmtpEmailSender` sends HTML email over SMTP with SSL:

- **Order confirmation/invoice** after a successful order, containing items, totals and shipping details.
- **Order status update** to a registered customer when an admin changes an order's status.
- **Password reset** with an Identity reset token.

Delivery requires `Smtp:Username` and `Smtp:Password`; `Smtp:From` defaults to the username. Email failures are logged and do not cancel an already completed order. There is no account-verification or newsletter email flow.

## Admin Panel Information

The Admin area uses its own layout (`Areas/Admin/Views/Shared/_AdminLayout.cshtml`) and is restricted to users in the `Admin` role. From it, an administrator can monitor dashboard metrics, manage the product catalog and homepage sliders, process orders and invoices, manage customer accounts and roles, review sales/inventory/top-product reports, and maintain contact information and messages. Catalog data can also be exported to and imported from Excel.

The startup seeder creates the initial administrator and roles. Treat the seeded password as a development-only value and replace it before sharing or deploying the application.

## Build and Deployment

Build and publish with the .NET 9 SDK:

```bash
dotnet restore TechNova/TechNova/TechNova.csproj
dotnet build TechNova/TechNova/TechNova.csproj -c Release
dotnet publish TechNova/TechNova/TechNova.csproj -c Release -o ./publish
```

Run the published application with `dotnet ./publish/TechNova.dll` from the repository root.

Before deploying:

1. Install the .NET 9 runtime on the target host.
2. Provide production configuration through `appsettings.Production.json`, environment variables (for example `ConnectionStrings__DefaultConnection`, `Stripe__SecretKey`, `Smtp__Username`, `Smtp__Password`) or a secrets manager. Do not commit real credentials.
3. Ensure MySQL is reachable and apply migrations with `dotnet ef database update` against the production connection string; back up the database first.
4. Serve the application over HTTPS and replace the seeded administrator password.

There are no Docker, CI/CD or cloud-specific deployment files in this repository.

## Troubleshooting

| Symptom | What to check |
|---|---|
| `NETSDK1045: The current .NET SDK does not support targeting .NET 9.0` | Install the .NET 9 SDK. Check installed versions with `dotnet --list-sdks`. |
| Database connection fails | Confirm MySQL is running, the host/port in `ConnectionStrings:DefaultConnection` is correct, and the user has access to `technovadb`. |
| `dotnet ef` is not recognized | Install the tool with `dotnet tool install --global dotnet-ef --version 9.0.1`. |
| Migration errors | Verify the connection string and schema state. Use `dotnet ef migrations list --project TechNova/TechNova/TechNova.csproj`; the app also applies pending migrations on startup. |
| Port `7101` is already in use | On Windows, run `netstat -ano | findstr :7101`; on macOS/Linux, run `lsof -i :7101`. Stop the process or run with `dotnet run --urls "https://localhost:7102"`. |
| NuGet restore fails | Check network access and the configured package sources; try `dotnet nuget locals all --clear` and restore again. |
| Card payments are unavailable | Set `Stripe:SecretKey`. The publishable key is not used by the current backend. |
| Emails are not delivered or orders show an email warning | Configure `Smtp:Username` and `Smtp:Password` (and optionally `Smtp:From`); use a Gmail app password where applicable. |
| Cannot sign in to the admin area | The seeded administrator is created by `DbInitializer.cs` on first run. Use its configured credentials locally, change the password immediately, and ensure the user has the `Admin` role. |

## Contributors

- Repository owner and maintainer: [@raj6400](https://github.com/raj6400).
- No other contributor names or contributions are declared in the repository.

Contributions are welcome. Fork the repository, create a focused branch (`feature/...`, `fix/...`, `docs/...`), use descriptive commits, run `dotnet build` before opening a pull request, and update this README when setup or behavior changes. Please keep feedback constructive and never include credentials in commits. No test project is present in the repository, so `dotnet test` is not currently applicable.

## License

No project-level `LICENSE` file is present in this repository. Do not assume an MIT or other license; contact the repository owner for reuse permissions. Third-party libraries bundled under `TechNova/TechNova/wwwroot/lib` retain their own license files.
