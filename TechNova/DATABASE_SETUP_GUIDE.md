# TechNova Database Configuration Summary

## ✅ Completed Configuration

### 1. Updated Connection String in appsettings.json
- **File**: `TechNova/appsettings.json`
- **Old Connection**: SQL Server (localdb)\MSSQLLocalDB
- **New Connection**: MySQL on localhost
- **Connection String**: `Server=localhost;Database=technovadb;User=root;Password=;Port=3306`

### 2. Verified NuGet Packages
- ✅ **Microsoft.EntityFrameworkCore.Tools** (v9.0.1) - Already installed
- ✅ **Pomelo.EntityFrameworkCore.MySql** (v9.0.0) - Already installed
- ✅ **Microsoft.AspNetCore.Identity.EntityFrameworkCore** (v9.0.1) - Already installed

### 3. DbContext Configuration
- **Class**: `ApplicationDbContext` (in `TechNova/Data/ApplicationDbContext.cs`)
- **Namespace**: `MobileShop.Data`
- **Base Class**: `IdentityDbContext<ApplicationUser>`
- **Program.cs Configuration**: MySQL is configured via `options.UseMySql()`

## ⚠️ Next Steps Required

### To Complete the AddMigration Command:

**Before running the migration, you need:**

1. **Start MySQL Server** (phpMyAdmin)
   - Ensure MySQL is running on localhost:3306
   - If password is required, update the connection string with the correct password

2. **Create the Database** (if not exists)
   ```sql
   CREATE DATABASE technovadb;
   ```

3. **Run the Migration Command**
   ```powershell
   cd D:\TechNova\TechNova\TechNova
   dotnet ef migrations add InitialCreate
   ```

### Optional: Use SQL Server Instead (If MySQL has issues)

If you prefer SQL Server instead of MySQL, change the connection string back to:
```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=technovadb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

And update Program.cs line 13-15 from `.UseMySql()` to `.UseSqlServer()`

## 📝 Entity Framework Entities

The project includes the following entities:
- Categories
- Brands
- Products
- ProductImages
- ProductSpecifications
- Reviews
- Orders
- OrderItems
- ShoppingCartItems
- WishlistItems
- ApplicationUser (Identity)
- IdentityRole

## 🔗 Reference
- DbContext: `TechNova/Data/ApplicationDbContext.cs`
- Program Configuration: `TechNova/Program.cs` (lines 12-15)
- Migrations output directory: `TechNova/Migrations/` (will be created after first migration)
