# TechNova Features Documentation

Complete guide to all features and functionality in TechNova.

---

## Table of Contents

1. [Customer Features](#customer-features)
2. [Admin Features](#admin-features)
3. [Payment & Checkout](#payment--checkout)
4. [Search & Filtering](#search--filtering)
5. [User Management](#user-management)
6. [Reporting & Analytics](#reporting--analytics)
7. [Additional Features](#additional-features)

---

## Customer Features

### 🏠 Homepage

**Features**:
- Dynamic promotional slider (managed by admin)
- Featured products section
- Popular brands carousel
- Quick access to categories
- Search bar with autocomplete
- Shopping cart quick access
- User login/register buttons

**URL**: `/`
**Access**: All users

---

### 📱 Product Browsing

#### Product Catalog
- Browse all smartphones and technology products
- View 12 products per page with pagination
- Sort by: Newest, Price (Low-High), Price (High-Low), Rating
- Full product details with images
- Stock availability status
- Price display with currency (₹ for INR)

**URL**: `/Products`
**Access**: All users

#### Product Details Page
- Multiple high-quality product images
- Detailed product specifications
- Brand and category information
- Price and discount information
- Stock status and quantity selector
- Customer reviews and ratings
- Add to Cart button
- Add to Wishlist button
- Compare button

**URL**: `/Products/Details/{id}`
**Access**: All users
**Example**: `/Products/Details/1`

#### Featured Sections
- New Products banner
- Best-Selling Products
- Related Products (on details page)

---

### 🔍 Search & Filter

#### Search Functionality
- **Search Bar**: Search by product name
- **Real-time Suggestions**: Autocomplete as you type
- **Search Breadcrumbs**: Show current search term

#### Filtering Options
- **Category Filter**: 
  - Smartphones
  - Accessories
  - Tablets
  - Smartwatches
  - Laptops

- **Brand Filter**:
  - Apple
  - Samsung
  - Google
  - OnePlus
  - And more...

- **Price Range Filter**:
  - Min Price: Custom input
  - Max Price: Custom input
  - Apply Filters button
  - Clear Filters button

#### Sorting
- Newest First
- Price: Low to High
- Price: High to Low
- Highest Rated

**URL**: `/Products?search=term&categoryId=1&brandId=2&minPrice=10000&maxPrice=100000`

---

### 🛒 Shopping Cart

#### Cart Features
- Add components from product pages
- Update quantities
- Remove items
- View cart subtotal
- View tax calculation
- View total with tax
- Continue Shopping button
- Proceed to Checkout button

#### Cart Updates
- Real-time cart count update
- Cart persists across sessions (user login)
- Display cart items count in navbar
- Empty cart message when no items

**URL**: `/ShoppingCart`
**Access**: All users

#### Cart Display
| Column | Details |
|--------|---------|
| Product Image | Thumbnail preview |
| Product Name | Clickable link to details |
| Brand | Product brand name |
| Price/Unit | Unit price |
| Quantity | Editable quantity |
| Subtotal | Price × Quantity |
| Actions | Remove button |

---

### ❤️ Wishlist

#### Wishlist Management
- Add products to wishlist from details page
- View all wishlist items
- Remove items from wishlist
- Move to cart from wishlist
- Persistent wishlist (saved in database)

#### Wishlist Features
- Display product image, name, price
- Stock status indicator
- "Add to Cart" button
- "Remove" button
- Email wishlist option (planned)

**URL**: `/Account/Wishlist`
**Access**: Authenticated users only

---

### 🛍️ Checkout Process

#### Checkout Steps

**Step 1: Shipping Address**
- First Name / Last Name
- Email Address
- Phone Number
- Street Address
- City / Province
- Postal Code
- Country

**Step 2: Billing Address**
- Option to use shipping address
- Or enter different billing address

**Step 3: Payment Information**
- Payment Method Selection:
  - Credit Card (Stripe)
  - Debit Card (Stripe)
  - Other payment methods (planned)
- Card Details:
  - Card Number (16 digits)
  - Cardholder Name
  - Expiry Date (MM/YY)
  - CVV (3 digits)

**Step 4: Order Review**
- Review items in cart
- Shipping address confirmation
- Billing address confirmation
- Order total summary
- Place Order button

#### Order Processing
- Real-time payment processing via Stripe
- Secure SSL encryption
- Automatic order confirmation email
- Order number generation
- Inventory update

**URL**: `/Checkout`
**Access**: Authenticated users with items in cart

---

### 📦 Order Management

#### Order History
- View all previous orders
- Order details:
  - Order Number
  - Order Date
  - Total Amount
  - Order Status
  - Shipping Address

#### Order Status Tracking
- Pending
- Processing
- Shipped
- Delivered
- Cancelled

#### Order Details View
- Itemized order items
- Product images
- Product names, prices, quantities
- Subtotal, tax, shipping, total
- Billing & shipping addresses
- Order timeline
- Download Invoice button

**URL**: `/Account/Orders`
**Access**: Authenticated users

---

### 🚚 Order Tracking

#### Real-Time Tracking
- Enter Order Number and Email
- View current order status
- Track shipping progress
- Estimated delivery date
- Last update timestamp

#### Tracking States
- Order Received
- Being Processed
- Shipped
- Out for Delivery
- Delivered

**URL**: `/Checkout/TrackOrder`
**Access**: All users

---

### ⭐ Product Reviews

#### Leave Review
- Rating (1-5 stars)
- Review Title
- Review Text
- Submit Review button
- Must be authenticated
- Can only review purchased products (planned)

#### View Reviews
- Display all reviews for product
- Sort by: Newest, Highest Rating, Lowest Rating
- Show reviewer name (first name + last initial)
- Show review date
- Display review title and text
- Show star rating with color coding
- Review count display

#### Review Moderation
- Admin review approval (planned)
- Block inappropriate content

**URL**: `/Products/Details/{id}#reviews`
**Access**: All users can view, logged-in can post

---

### 📊 Product Comparison

#### Add to Compare
- Add up to 5 products to comparison
- Compare button on product page
- Compare bar slides in from bottom

#### Comparison Features
- Side-by-side product comparison
- View all specifications
- Compare prices
- Compare ratings
- Pro/Con comparison (planned)
- Export comparison (planned)

#### Comparison Bar
- Shows selected product count (e.g., "2 items selected")
- "Proceed to Compare" button
- "Clear" button

**URL**: `/Products/Compare`
**Access**: All users

---

### 👤 User Account

#### Profile Management
- View profile information
- Edit personal details:
  - First Name
  - Last Name
  - Email Address
  - Phone Number
  - Profile Picture

#### Password Management
- Change Password
- Reset Password (via email)
- Password verification
- Show/Hide password toggle

#### Account Settings
- Email notifications preferences
- Newsletter subscription
- Privacy settings

**URL**: `/Account/Profile`
**Access**: Authenticated users

---

### 🔐 Authentication

#### User Registration
- Email-based registration
- Password requirements:
  - Minimum 8 characters
  - At least 1 uppercase letter
  - At least 1 lowercase letter
  - At least 1 digit
  - At least 1 special character

#### User Login
- Email and password login
- "Remember Me" option
- Forgot password link
- Account lockout after 5 failed attempts (planned)

#### Password Reset
- Email-based password reset
- Secure password reset token
- Token expiration (24 hours)
- Email notification

**URLs**: 
- Register: `/Account/Register`
- Login: `/Account/Login`
- Forgot Password: `/Account/ForgotPassword`

---

## Admin Features

### 📊 Dashboard

#### Dashboard Overview
- **Total Orders**: Count of all orders
- **Paid Revenue**: Total revenue from completed orders
- **Products**: Total product count
- **Customers**: Total registered customers
- **Pending Orders**: Orders awaiting processing
- **Low Stock Products**: Count of products at low inventory

#### Charts & Analytics
- **Monthly Sales Chart**: Line chart showing sales & orders over time
- **Top Products Chart**: Pie chart of best-selling products
- **Recent Orders Table**: Show latest 10 orders
- **Top-Selling Products**: List of highest revenue products

**URL**: `/Admin`
**Access**: Admin users only

---

### 📦 Product Management

#### Product Listing
- All products in searchable, sortable table
- Quick filters for:
  - Active/Inactive status
  - Category
  - Brand
  - Price range

#### Create Product
- Product Name
- Description (Rich text editor)
- Brand Selection
- Category Selection
- Price
- Discount Price
- Stock Quantity
- Multiple product images upload
- Product specifications (key-value pairs)
- Meta Description
- Is Active toggle

#### Edit Product
- Modify all product details
- Update images
- Change specifications
- Update pricing

#### Delete Product
- Soft delete (archive)
- Option to restore archived products
- Cascade delete related data

#### Bulk Operations
- Bulk activate/deactivate
- Bulk delete (planned)
- Export to CSV (planned)

**URLs**: 
- List: `/Admin/Products`
- Create: `/Admin/Products/Create`
- Edit: `/Admin/Products/Edit/{id}`

---

### 🏷️ Category Management

#### Category Listing
- All categories in table
- Search by name
- Status indicator (Active/Inactive)
- Product count per category
- Edit and Delete buttons

#### Create Category
- Category Name
- Description
- Display Order (sort order)
- Category Image Upload
- Is Active toggle

#### Edit Category
- Modify category details
- Update image
- Change display order

#### Category Hierarchy (Planned)
- Parent/Child categories
- Subcategory support
- Breadcrumb navigation

**URLs**:
- List: `/Admin/Categories`
- Create: `/Admin/Categories/Create`
- Edit: `/Admin/Categories/Edit/{id}`

---

### 🏢 Brand Management

#### Brand Listing
- All brands in table
- Brand logo display
- Brand name and description
- Country of origin
- Website URL
- Product count per brand
- Status indicator

#### Create Brand
- Brand Name
- Description
- Logo Upload
- Website URL
- Country Selection
- Is Active toggle

#### Edit Brand
- Modify brand information
- Update logo
- Change website URL

**URLs**:
- List: `/Admin/Brands`
- Create: `/Admin/Brands/Create`
- Edit: `/Admin/Brands/Edit/{id}`

---

### 🎨 Slider Management

#### Homepage Slider
- Create promotional banners
- Multiple images/sliders
- Custom button text
- Button link URL
- Display order management
- Active/Inactive status

#### Slider Features
- Auto-play functionality
- Navigation arrows
- Dot indicators
- Responsive design
- Image upload

#### Create Slider
- Banner Image Upload
- Heading Text
- Description Text
- Button Text ("Shop Now", etc.)
- Button Link URL
- Display Order
- Is Active toggle

**URLs**:
- List: `/Admin/Sliders`
- Create: `/Admin/Sliders/Create`
- Edit: `/Admin/Sliders/Edit/{id}`

---

### 📋 Order Management

#### Order Listing
- All orders in searchable table
- Filter by:
  - Order Status
  - Date Range
  - Amount Range
  - Customer

#### Order Details
- Order number and date
- Customer information
- Billing & shipping address
- Order items (products, quantities, prices)
- Order total (subtotal, tax, shipping total)
- Order status
- Payment method
- Payment status

#### Order Actions
- Update order status
- Add order notes
- Generate Invoice (PDF)
- Send order update email
- Cancel order (if applicable)

#### Order Status Management
- Mark as Processing
- Mark as Shipped
- Mark as Delivered
- Mark as Cancelled
- Generate shipping label (planned)

**URLs**:
- List: `/Admin/Orders`
- Details: `/Admin/Orders/Details/{id}`
- Invoice: `/Admin/Orders/Invoice/{id}`

---

### 👥 Customer Management

#### Customer Listing
- All registered customers in table
- Customer name
- Email address
- Account status (Active/Inactive)
- Total orders count
- Total spent
- Registration date
- Last activity date

#### Customer Actions
- View customer details
- Deactivate/Activate account
- View customer orders
- View customer address
- Send email to customer (planned)

#### Customer Deactivation
- Prevents customer login
- Preserves order history
- Can be reactivated anytime

**URLs**:
- List: `/Admin/Users`
- Details: `/Admin/Users/Details/{id}`

---

### 📊 Reports & Analytics

#### Sales Report
- Date range selection
- Monthly sales data
- Order count by month
- Revenue by month
- Total sales summary
- Line chart visualization
- Export to Excel (planned)

#### Inventory Report
- Products with low stock alert
- Products out of stock
- Stock level by category
- Stock turnover analysis
- Reorder suggestions (planned)

#### Top Products Report
- Best-selling products
- Revenue contribution per product
- Sales count per product
- Profit margins (planned)
- Trending products

#### Customer Report (Planned)
- Top customers by spending
- New customers
- Inactive customers
- Customer lifetime value

**URLs**:
- Sales: `/Admin/Dashboard/SalesReport`
- Inventory: `/Admin/Dashboard/InventoryReport`
- Top Products: `/Admin/Dashboard/TopProducts`

---

### 🧾 Invoice Management

#### Invoice Generation
- Auto-generated PDF invoices
- Professional invoice layout
- Company details
- Customer details
- Order items with prices
- Tax calculation
- Order total
- Terms & conditions

#### Invoice Features
- Email invoice to customer
- Download invoice as PDF
- Print invoice
- Invoice history

**URL**: `/Admin/Orders/Invoice/{id}`

---

## Payment & Checkout

### 💳 Stripe Integration

#### Supported Payment Methods
- Visa
- Mastercard
- American Express
- Discover

#### Payment Features
- Secure payment processing
- PCI DSS compliant
- Real-time payment verification
- Automatic invoice generation
- Failed payment handling
- Payment retry mechanism (planned)

#### Payment Security
- SSL/TLS encryption
- Stripe tokenization
- No credit card storage on server
- Fraud detection

---

### 🛡️ Security

#### Data Protection
- Password hashing (bcrypt)
- SQL injection prevention (EF Core parameterized queries)
- CSRF token validation on all forms
- Secure session management
- HTTPS/SSL enforcement

#### User Privacy
- Encrypted password storage
- Secure password reset tokens
- Email verification
- Personal data protection

---

## Search & Filtering

### 🔎 Search Features

#### Text Search
- Search by product name
- Search by brand name
- Search by category
- Real-time autocomplete suggestions
- Search history (planned)

#### Advanced Filters
- **Price Range**: Min and Max price inputs
- **Category**: Multi-select categories
- **Brand**: Multi-select brands
- **Availability**: In stock / Out of stock
- **Rating**: Filter by minimum rating (planned)

#### Sort Options
- Newest First
- Best Sellers
- Price: Low to High
- Price: High to Low
- Highest Rated
- Most Reviewed

---

## User Management

### 👤 User Accounts

#### Account Types
- **Customer**: Regular user purchasing products
- **Admin**: Full access to admin panel

#### User Information Stored
- Email (unique)
- Password (hashed)
- First Name
- Last Name
- Phone Number
- Addresses (multiple)
- Profile Picture
- Registration Date
- Last Login Date
- Account Status

#### Address Management
- Multiple addresses per user
- Default shipping address
- Default billing address
- Address book
- Edit/Delete addresses

---

## Reporting & Analytics

### 📈 Analytics Dashboard

#### Key Metrics
- Total Revenue
- Orders Count
- Average Order Value
- Customer Count
- Cart Abandonment Rate (planned)
- Conversion Rate (planned)

#### Time-Based Reports
- Daily Sales
- Weekly Sales
- Monthly Sales
- Yearly Sales

#### Product Analytics
- Top Selling Products
- Low Stock Products
- Products by Category
- Products by Brand

#### Customer Analytics
- New Customers
- Repeat Customers
- Customer Retention (planned)
- Customer Lifetime Value (planned)

---

## Additional Features

### 📧 Email Notifications

#### Notification Types
- **Order Confirmation**: Sent after order placement
- **Order Status Update**: When order status changes
- **Shipping Notification**: When order ships
- **Delivery Confirmation**: When order delivered
- **Password Reset**: For password reset requests
- **Account Verification**: Email confirmation

#### Email Template
- Company branding
- Order details
- Order number and date
- Track order link
- Customer support contact

### 🔔 In-App Notifications

#### Notification Types
- Product added to cart successfully
- Item removed from cart
- Wishlist updates
- Order status changes
- Admin alerts (low stock, new orders)

#### Toast Notifications
- Success messages (green)
- Error messages (red)
- Warning messages (yellow)
- Info messages (blue)
- Auto-dismiss after 5 seconds

---

### 🛠️ System Features

#### Database Features
- Automatic timestamp tracking (CreatedAt, UpdatedAt)
- Soft delete support for data retention
- Foreign key relationships
- Data validation at database level
- Proper indexing for performance

#### Performance Features
- Database query optimization
- Pagination for large datasets
- Caching (planned)
- CDN integration (planned)
- Image optimization

#### Testing Features
- Unit tests (planned)
- Integration tests (planned)
- End-to-end tests (planned)
- Load testing (planned)

---

<div align="center">

**TechNova Features Complete Guide**

For questions, see [README.md](README.md) or [GitHub Discussions](https://github.com/yourusername/TechNova/discussions)

[⬆ Back to Top](#technova-features-documentation)

</div>
