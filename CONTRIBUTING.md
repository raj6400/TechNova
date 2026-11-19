# Contributing to TechNova

Thank you for your interest in contributing to **TechNova**! We appreciate your efforts to help improve this project. This document provides guidelines and instructions for contributing.

---

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [Getting Started](#getting-started)
3. [Development Setup](#development-setup)
4. [Making Changes](#making-changes)
5. [Commit Guidelines](#commit-guidelines)
6. [Pull Request Process](#pull-request-process)
7. [Coding Standards](#coding-standards)
8. [Testing](#testing)
9. [Documentation](#documentation)
10. [Questions & Support](#questions--support)

---

## Code of Conduct

We are committed to providing a welcoming and inspiring community for all. Please read and adhere to our Code of Conduct:

- Be respectful and inclusive
- Welcome diverse perspectives
- Focus on constructive feedback
- Report violations to: support@technova.com

---

## Getting Started

### Prerequisites

- .NET 9 SDK or higher
- Visual Studio 2022 or VS Code
- MySQL 8.0+
- Git

### Fork & Clone

1. **Fork** the repository on GitHub
2. **Clone** your fork locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/TechNova.git
   cd TechNova
   ```
3. **Add upstream** remote:
   ```bash
   git remote add upstream https://github.com/ORIGINAL-OWNER/TechNova.git
   ```

---

## Development Setup

### 1. Install Dependencies

```bash
cd TechNova/TechNova
dotnet restore
```

### 2. Configure Local Database

Update `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=localhost;Database=technova_dev;User=root;Password=devpassword;Port=3306"
  }
}
```

### 3. Create Database

```bash
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
```

Access the app at `https://localhost:7101`

---

## Making Changes

### Create a Feature Branch

```bash
# Update your fork
git fetch upstream
git checkout upstream/master

# Create a new branch
git checkout -b feature/your-feature-name
```

### Branch Naming Convention

- `feature/` - New features (e.g., `feature/add-wishlist`)
- `fix/` - Bug fixes (e.g., `fix/cart-calculation`)
- `docs/` - Documentation updates
- `refactor/` - Code refactoring
- `test/` - Test additions
- `chore/` - Maintenance tasks

### Make Your Changes

1. Make focused, atomic commits
2. Follow the coding standards (see below)
3. Add tests for new functionality
4. Update documentation as needed

---

## Commit Guidelines

### Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation changes
- **style**: Code style changes (formatting, missing semicolons, etc.)
- **refactor**: Code refactoring without feature changes
- **perf**: Performance improvements
- **test**: Adding or updating tests
- **chore**: Build process, dependency updates, etc.

### Examples

```
feat(products): add product comparison feature

Allows customers to compare up to 5 products side-by-side

Closes #123
```

```
fix(cart): resolve quantity update bug

Fixed issue where cart didn't update when changing item quantity

Fixes #456
```

### Best Practices

- ✅ Use imperative mood ("add feature" not "added feature")
- ✅ Write concise subjects (50 chars max)
- ✅ Reference issue numbers when applicable
- ✅ Explain why, not just what

---

## Pull Request Process

### Before Submitting

1. **Ensure your code is clean**:
   ```bash
   dotnet build
   dotnet test
   ```

2. **Update your fork**:
   ```bash
   git fetch upstream
   git rebase upstream/master
   ```

3. **Push your changes**:
   ```bash
   git push origin feature/your-feature-name
   ```

### Create Pull Request

1. Go to **GitHub** → Your Fork
2. Click **"Compare & pull request"**
3. Fill in the PR template:

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Related Issues
Closes #(issue number)

## Testing
Describe how you tested the changes

## Screenshots (if applicable)
Add screenshots for UI changes

## Checklist
- [ ] Code follows style guidelines
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] No new warnings generated
- [ ] Changes work locally
```

### PR Review Process

- Address review comments promptly
- Push updates to the same branch
- Respond to feedback professionally
- Be patient with the review process

---

## Coding Standards

### C# Style Guide

#### Naming Conventions

```csharp
// Classes, Interfaces, Methods - PascalCase
public class ProductService { }
public interface IOrderService { }
public void CalculateTotal() { }

// Properties - PascalCase
public string ProductName { get; set; }

// Fields/Variables - camelCase
private string _connectionString;
private int itemCount = 0;

// Constants - UPPER_SNAKE_CASE
private const string API_BASE_URL = "https://api.example.com";
```

#### Code Organization

```csharp
// 1. Using statements
using System;
using System.Collections.Generic;

namespace TechNova.Services
{
	// 2. Class declaration
	public class ProductService
	{
		// 3. Constants
		private const string CACHE_KEY = "Products";

		// 4. Fields
		private readonly IRepository _repository;

		// 5. Properties
		public int TotalProducts { get; set; }

		// 6. Constructors
		public ProductService(IRepository repository)
		{
			_repository = repository;
		}

		// 7. Public methods
		public async Task<Product> GetByIdAsync(int id)
		{
			return await _repository.GetAsync(id);
		}

		// 8. Private methods
		private bool ValidateProduct(Product product)
		{
			return product != null;
		}
	}
}
```

#### Documentation

```csharp
/// <summary>
/// Calculates the total price of items in the shopping cart
/// </summary>
/// <param name="cartItems">Collection of shopping cart items</param>
/// <returns>Total price as decimal</returns>
/// <exception cref="ArgumentNullException">Thrown when cartItems is null</exception>
public decimal CalculateTotal(IEnumerable<CartItem> cartItems)
{
	if (cartItems == null)
		throw new ArgumentNullException(nameof(cartItems));

	return cartItems.Sum(x => x.Price * x.Quantity);
}
```

#### Async/Await

```csharp
// ✅ Good - Use Async methods for I/O operations
public async Task<IActionResult> GetProducts()
{
	var products = await _service.GetProductsAsync();
	return View(products);
}

// ❌ Bad - Don't use .Result or .Wait()
public IActionResult GetProducts()
{
	var products = _service.GetProductsAsync().Result;
	return View(products);
}
```

#### LINQ

```csharp
// ✅ Method syntax - Clean and readable
var activeProducts = products
	.Where(p => p.IsActive)
	.OrderByDescending(p => p.CreatedDate)
	.Take(10)
	.ToList();

// ✅ Query syntax - for complex queries
var result = from p in products
			 where p.CategoryId == categoryId
			 orderby p.Price
			 select new { p.Name, p.Price };
```

### Razor View Guidelines

```razor
@* Always use @* *@ comments *@
@model ProductViewModel

<div class="container">
	@if (Model.Products.Any())
	{
		<div class="row">
			@foreach (var product in Model.Products)
			{
				<div class="col-md-4">
					<h5>@product.Name</h5>
					<p>@product.Description</p>
				</div>
			}
		</div>
	}
	else
	{
		<p class="text-muted">No products found</p>
	}
</div>
```

### Database Guidelines

```csharp
// ✅ Use Entity Framework Core queries
var products = await _context.Products
	.Where(p => p.CategoryId == categoryId)
	.Include(p => p.Reviews)
	.ToListAsync();

// ✅ Use async methods for database access
public async Task<Product> GetProductAsync(int id)
{
	return await _context.Products.FindAsync(id);
}
```

---

## Testing

### Unit Tests

```csharp
[TestClass]
public class ShoppingCartServiceTests
{
	private ShoppingCartService _service;
	private Mock<IRepository> _mockRepository;

	[TestInitialize]
	public void Setup()
	{
		_mockRepository = new Mock<IRepository>();
		_service = new ShoppingCartService(_mockRepository.Object);
	}

	[TestMethod]
	public async Task AddToCart_WithValidProduct_ReturnsSuccess()
	{
		// Arrange
		var productId = 1;

		// Act
		var result = await _service.AddToCartAsync(productId);

		// Assert
		Assert.IsTrue(result.IsSuccess);
	}
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test TechNova.Tests

# Run with coverage report
dotnet test /p:CollectCoverage=true
```

---

## Documentation

### Code Comments

```csharp
// Use for explaining "why", not "what"
// ✅ Good
// We use Redis cache to reduce database load on high-traffic periods
var cached = await _cacheService.GetAsync(key);

// ❌ Bad
// Get from cache
var cached = await _cacheService.GetAsync(key);
```

### XML Documentation

Always document public APIs:

```csharp
/// <summary>
/// Processes a customer order and generates an invoice
/// </summary>
/// <param name="orderId">The unique order identifier</param>
/// <returns>A task representing the asynchronous operation</returns>
/// <exception cref="OrderNotFoundException">Thrown when order not found</exception>
/// <exception cref="InvalidOperationException">Thrown when order is already processed</exception>
public async Task ProcessOrderAsync(int orderId)
{
	// Implementation
}
```

### Updating README

If your change affects usage or setup:
1. Update relevant sections in `README.md`
2. Update `CHANGELOG.md` with your changes
3. Add migration guide if breaking changes

---

## Questions & Support

### Getting Help

- **Documentation**: Check the [Wiki](https://github.com/yourusername/TechNova/wiki)
- **Issues**: Search [existing issues](https://github.com/yourusername/TechNova/issues)
- **Discussions**: Join our [community discussions](https://github.com/yourusername/TechNova/discussions)
- **Email**: support@technova.com

### Asking Questions

1. Include relevant code snippets
2. Provide error messages and logs
3. Describe expected vs actual behavior
4. Specify your environment (.NET version, OS, etc.)

---

## Review Checklist

Before submitting your PR, ensure:

- [ ] ✅ Code builds without errors
- [ ] ✅ All tests pass
- [ ] ✅ No new compiler warnings
- [ ] ✅ Code follows style guidelines
- [ ] ✅ XML documentation added for public members
- [ ] ✅ Git history is clean
- [ ] ✅ Commit messages are descriptive
- [ ] ✅ Updated README if necessary
- [ ] ✅ No sensitive data committed
- [ ] ✅ PR description is clear

---

## License

By contributing to TechNova, you agree that your contributions will be licensed under its MIT License.

---

<div align="center">

**Thank you for contributing to TechNova! 🎉**

Your contributions make this project better for everyone.

[⬆ Back to Top](#contributing-to-technova)

</div>
