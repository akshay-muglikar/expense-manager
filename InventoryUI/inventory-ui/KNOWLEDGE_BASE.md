# Inventory Management Application - Knowledge Base

## 1. Overview

### What the Application Does
The Inventory Management Application is a comprehensive business management solution designed for retail stores, shops, and small to medium businesses. It streamlines inventory tracking, sales processing, vendor relationships, customer management, and financial operations through an intuitive web interface.

### Key Use Cases
- **Retail Stores**: Track product inventory, generate bills, manage supplier relationships
- **Service Centers**: Manage parts inventory, track customer purchases, handle vendor expenses  
- **Small Businesses**: Complete business operations including inventory, billing, and financial tracking
- **Multi-location Businesses**: Centralized inventory and customer management

### Value Proposition
- **Efficiency**: Streamlined workflows for common business operations
- **Accuracy**: Barcode scanning and automated calculations reduce errors
- **Insights**: Real-time inventory tracking and financial summaries
- **Mobility**: Responsive design works on desktop and mobile devices
- **Integration**: WhatsApp integration for customer communication

---

## 2. Core Modules & Features

### 📦 Inventory Management
**Purpose**: Track and manage product stock levels, pricing, and product information

**Key Features**:
- Add new inventory items with detailed specifications
- Update existing item information (name, price, quantity, category)
- Track stock levels with real-time quantity updates
- Barcode support for quick item identification
- Category-wise organization
- Car/vehicle-specific parts categorization
- Search and filter inventory items

**Components**: `addinventory.component`, `inventory.component`

### 🧾 Billing & Invoicing
**Purpose**: Create, manage, and track sales transactions

**Key Features**:
- **Bill Creation**: Generate new bills with customer information
- **Item Selection**: Add items via barcode scanning, search, or dropdown selection  
- **Pricing Control**: Modify item prices and quantities per bill
- **Payment Tracking**: Support for Cash, Card, and UPI payments
- **Discounts & Advances**: Apply discounts and track advance payments
- **Bill History**: View all historical bills with date filtering
- **Print & Download**: Generate PDF invoices for customers
- **WhatsApp Integration**: Send bill details directly to customer WhatsApp
- **Bill Editing**: Modify existing bills and update items

**Components**: `bill-v2.component`, `billhistory.component`

### 🏭 Vendor Account Management
**Purpose**: Manage supplier relationships and track vendor-related transactions

**Key Features**:
- **Vendor Registration**: Add new vendors with contact information
- **Vendor Cards View**: Visual card-based vendor listing
- **Account Summary**: Track vendor balances, credits, and debits
- **Expense Tracking**: Record vendor payments and purchases
- **Transaction History**: Detailed vendor account statements
- **Search & Filter**: Find vendors quickly by name or mobile

**Components**: `vendor.component`
**Models**: `VendorModel`, `VendorAccountModel`

### 👥 Customer Management
**Purpose**: Track customer information and purchase history

**Key Features**:
- **Customer Database**: Maintain customer contact information
- **Purchase History**: View all bills for specific customers
- **Quick Billing**: Create new bills with pre-filled customer data
- **Account Summary**: Track customer purchase patterns
- **Search Functionality**: Find customers by name or mobile number

**Components**: `customer.component`
**Models**: `CustomerModel`, `CustomerAccountModel`

### 💰 Expense Tracking
**Purpose**: Monitor business expenses and financial outflows

**Key Features**:
- **Expense Entry**: Record various business expenses
- **Category Management**: Organize expenses by type
- **Date Filtering**: View expenses by custom date ranges
- **Vendor Integration**: Link expenses to specific vendors
- **Financial Reporting**: Track spending patterns and totals

**Components**: `expense.component`

---

## 3. Common User Queries & How to Answer Them

### Inventory Related Questions

**Q: "How do I add a new item to inventory?" / "How to add new products?" / "Add inventory items"**

**A:** To add a new inventory item:
1. Navigate to the Bills section and click "Add new Inventory" button
2. Fill in the required fields: Item name, Category, Car/Vehicle type, Quantity, Description, Price
3. Optionally add a barcode for quick scanning
4. Click "Save" to add the item to your inventory
5. The item will immediately be available for billing

**Q: "How do I update item prices?" / "Change product price" / "Modify inventory"**

**A:** To update existing inventory items:
1. Go to the Inventory section from the sidebar menu
2. Find the item you want to update using search or browsing
3. Click on the item to edit details
4. Modify price, quantity, or other details as needed
5. Save changes to update the inventory

**Q: "How can I search for items?" / "Find products quickly"**

**A:** You can search for items in several ways:
- Use the search bar in billing to find items by name or car type
- Scan barcodes using the barcode scanner input field
- Browse by category in the inventory section
- Use the item dropdown when creating bills

### Billing Related Questions

**Q: "How do I create a new bill?" / "Generate invoice" / "Make a sale"**

**A:** To create a new bill:
1. Click on "Bills" in the sidebar navigation
2. Enter customer name and mobile number
3. Add items by:
   - Searching and clicking on item cards
   - Using the barcode scanner
   - Selecting from the dropdown in the items table
4. Adjust quantities and prices if needed
5. Select payment mode (Cash/Card/UPI)
6. Apply discount or advance if applicable
7. Click "Save Bill" to generate the invoice

**Q: "How to send bills to customers?" / "WhatsApp integration" / "Share invoice"**

**A:** To send bills via WhatsApp:
1. Create and save the bill first
2. Click the WhatsApp icon in the bill actions
3. The system will generate a message with bill details and download link
4. This opens WhatsApp with pre-filled message ready to send

**Q: "How do I print or download bills?" / "Generate PDF invoice"**

**A:** To print or download bills:
- **Download PDF**: Click the download button after saving the bill
- **Print**: Use the print button to open print dialog
- **From History**: Access any previous bill from Bill History and use print/download options

### Vendor Related Questions

**Q: "How do I add a new vendor?" / "Register supplier" / "Add vendor account"**

**A:** To add a new vendor:
1. Go to "Vendors" section from sidebar
2. In the vendor form on the left, enter:
   - Vendor name
   - Mobile number
3. Click "Add Vendor" button
4. The vendor will appear in the cards section and be available for expense tracking

**Q: "How to track vendor expenses?" / "Record vendor payments" / "Add vendor transaction"**

**A:** To add vendor expenses:
1. Select a vendor from the vendor cards
2. Switch to the "Account" tab in the right panel
3. Click "Add Expense" button
4. Fill in:
   - Description of the expense
   - Amount
   - Expense type (Credit/Debit)
   - Payment mode
   - Date
5. Click "Add Transaction" to record the expense

### Customer Related Questions

**Q: "How do I view customer history?" / "Check customer bills" / "Customer account"**

**A:** To view customer information:
1. Navigate to "Customers" section
2. Search for the customer using name or mobile number
3. Click on the customer card to view details
4. The system shows:
   - Total bills count
   - Total purchase amount
   - Last purchase date
   - Complete bill history in a table

**Q: "How to create a bill for existing customer?" / "Quick billing"**

**A:** To create a bill for existing customer:
1. From the customer section, click "Create Bill" button next to customer name
2. The system will redirect to billing with customer details pre-filled
3. Add items and complete the billing process as usual

---

## 4. Step-by-Step Guides

### Adding/Updating Inventory Items

#### Adding a New Item:
1. **Access Form**: Navigate to Bills → Click "Add new Inventory"
2. **Fill Details**:
   - **Item Name**: Enter descriptive product name
   - **Category**: Select or enter product category
   - **Car**: Specify vehicle type (if applicable)
   - **Quantity**: Enter initial stock quantity
   - **Description**: Add detailed product description
   - **Price**: Set selling price
   - **Barcode**: (Optional) Add barcode for scanning
3. **Validation**: Ensure all required fields are filled
4. **Save**: Click "Save" to add item to inventory
5. **Confirmation**: Item appears immediately in item selection

#### Updating Existing Items:
1. **Navigate**: Go to Inventory section
2. **Search**: Use search to find specific item
3. **Select**: Click on item to edit
4. **Modify**: Update any field (price, quantity, description)
5. **Save Changes**: Confirm updates
6. **Verification**: Changes reflect immediately in billing

### Generating/Updating/Downloading Invoices

#### Creating New Invoice:
1. **Start Bill**: Access Bills section from sidebar
2. **Customer Info**:
   - Enter customer name (required)
   - Add mobile number for WhatsApp integration
3. **Add Items**:
   - **Option A**: Search items and click item cards
   - **Option B**: Scan barcode in scanner field
   - **Option C**: Use dropdown in items table
4. **Configure Items**:
   - Adjust quantity for each item
   - Modify price if needed (defaults to inventory price)
   - Remove unwanted items using delete button
5. **Payment Details**:
   - Select payment mode (Cash/Card/UPI)
   - Add discount amount if applicable
   - Record advance payment if any
6. **Review**: Check grand total calculation
7. **Save**: Click "Save Bill" to generate invoice
8. **Confirmation**: System shows success message with bill ID

#### Updating Existing Invoice:
1. **Access**: Go to Bill History section
2. **Find Bill**: Use date filters or search to find bill
3. **Edit**: Click edit button (pencil icon) on desired bill
4. **Modify**: Make necessary changes to items, quantities, or customer info
5. **Update**: Click "Update Bill" to save changes
6. **Verification**: Updated bill reflects changes immediately

#### Downloading and Sharing:
1. **Download PDF**:
   - Save the bill first
   - Click download button
   - PDF generates automatically for printing/email
2. **Print Direct**:
   - Use print button for immediate printing
   - Standard browser print dialog opens
3. **WhatsApp Sharing**:
   - Click WhatsApp icon after saving bill
   - Pre-formatted message opens in WhatsApp
   - Message includes bill summary and download link

### Creating Vendor Account & Managing Expenses

#### Adding New Vendor:
1. **Navigate**: Go to Vendors section
2. **Form Entry**:
   - Enter vendor/supplier name
   - Add mobile number for contact
3. **Submit**: Click "Add Vendor"
4. **Verification**: New vendor appears in cards grid
5. **Ready**: Vendor available for expense tracking

#### Managing Vendor Expenses:
1. **Select Vendor**: Click on vendor card from grid
2. **Account Tab**: Switch to "Account" tab in right panel
3. **Add Expense**:
   - Click "Add Expense" button
   - **Description**: Enter expense details (e.g., "Parts purchase", "Service payment")
   - **Amount**: Enter transaction amount
   - **Type**: Select Credit (money given to vendor) or Debit (money received from vendor)
   - **Payment Mode**: Choose Cash, Card, or UPI
   - **Date**: Set transaction date (defaults to today)
4. **Save**: Click "Add Transaction"
5. **Verification**: Transaction appears in account history
6. **Balance**: Account summary updates automatically

#### Vendor Account Summary:
- **Total Balance**: Net amount (Credits - Debits)
- **Transaction History**: Chronological list of all transactions
- **Quick Stats**: Total credits, debits, and current balance
- **Search/Filter**: Find specific transactions by date or description

### Expense Tracking

#### Adding New Expense:
1. **Navigate**: Access Expense section from sidebar
2. **Form Fields**:
   - **Description**: Enter expense details
   - **Amount**: Specify expense amount
   - **Category**: Select expense category
   - **Date**: Choose transaction date
   - **Payment Method**: Select payment mode
3. **Link Vendor**: (Optional) Associate with specific vendor
4. **Save**: Submit expense entry
5. **Confirmation**: Expense appears in expenses grid

#### Searching/Filtering Expenses:
1. **Date Range**: Use date picker to filter by period
2. **Quick Filters**:
   - Today's expenses
   - This week
   - This month
   - Custom range
3. **Category Filter**: Filter by expense type
4. **Amount Range**: Set minimum/maximum amount filters
5. **Vendor Filter**: Show expenses for specific vendor only

---

## 5. Troubleshooting & FAQ

### Common Issues and Solutions

#### Billing Issues

**Issue**: "Items not appearing in search"
**Solution**: 
- Check if item exists in inventory
- Verify search term spelling
- Ensure item has proper name and category
- Try searching by car type or partial name

**Issue**: "Barcode scanning not working"
**Solution**:
- Ensure barcode is properly assigned to item in inventory
- Check barcode scanner input has focus
- Verify barcode format matches item's barcode field
- Try manual entry if scanner hardware issues

**Issue**: "Bill totals not calculating correctly"
**Solution**:
- Check quantity and price fields for each item
- Verify discount amount is entered correctly
- Ensure advance amount is properly applied
- Refresh page if calculation seems stuck

**Issue**: "WhatsApp message not formatting properly"
**Solution**:
- Ensure customer mobile number is entered
- Check bill is saved before sending
- Verify bill has items and customer information
- Try refreshing and saving bill again

#### Inventory Issues

**Issue**: "Can't update inventory quantities"
**Solution**:
- Check user permissions
- Ensure item exists and is not deleted
- Verify all required fields are filled
- Save changes before navigating away

**Issue**: "New items not appearing in billing"
**Solution**:
- Refresh the billing page after adding items
- Check if item was saved successfully
- Verify item has price and quantity set
- Search using exact item name or barcode

#### Vendor/Customer Issues

**Issue**: "Vendor account balance seems wrong"
**Solution**:
- Review all transactions in account history
- Check if any transactions were entered incorrectly (Credit vs Debit)
- Verify amounts are entered correctly
- Contact support if discrepancies persist

**Issue**: "Customer bill history not loading"
**Solution**:
- Check internet connection
- Try refreshing the page
- Verify customer has actual bill history
- Use different date range if using filters

### Data Validation Errors

**Error**: "Please fill all required fields"
**Resolution**: Ensure all mandatory fields (marked with *) are completed before saving

**Error**: "Invalid mobile number format"
**Resolution**: Enter mobile number in correct format (10 digits for Indian numbers)

**Error**: "Item already exists"
**Resolution**: Check if similar item exists; modify name or use existing item

**Error**: "Insufficient quantity in stock"
**Resolution**: Update inventory quantity or reduce bill quantity

### Performance Issues

**Issue**: "Page loading slowly"
**Solutions**:
- Check internet connection speed
- Clear browser cache and cookies
- Close unnecessary browser tabs
- Try different browser if issues persist

**Issue**: "Search results delayed"
**Solutions**:
- Wait for current search to complete before typing more
- Use more specific search terms
- Limit search results scope

---

## 6. Glossary

**Bill/Invoice**: A sales transaction record containing customer information, items purchased, quantities, and payment details

**Inventory Item**: A product or part stored in the system with details like name, price, quantity, and category

**Vendor/Supplier**: A business or individual who supplies products or services to your business

**Customer**: A person or entity who purchases products or services from your business

**Barcode**: A machine-readable code used for quick item identification and scanning

**Stock Quantity**: The current available quantity of an item in inventory

**Credit Transaction**: Money paid to a vendor (increases vendor balance)

**Debit Transaction**: Money received from a vendor (decreases vendor balance)

**Advance Payment**: Partial payment received before completing the full transaction

**Discount**: Reduction in the total bill amount

**Payment Mode**: Method of payment (Cash, Card, UPI)

**Grand Total**: Final amount after applying discounts and advances

**Bill History**: Record of all previous sales transactions

**Expense**: Business-related expenditure or cost

---

## 7. Data Fields Reference

### Inventory Items (`Item` Model)

| Field | Description | Format | Validation | Required |
|-------|-------------|--------|------------|----------|
| `id` | Unique identifier | Number | Auto-generated | Yes |
| `name` | Item name | String (1-100 chars) | Must be unique | Yes |
| `category` | Item category | String (1-50 chars) | - | Yes |
| `car` | Vehicle type | String (1-50 chars) | - | No |
| `quantity` | Stock quantity | Number | Must be ≥ 0 | Yes |
| `description` | Item description | String (max 500 chars) | - | Yes |
| `price` | Selling price | Number | Must be > 0 | Yes |
| `barcode` | Barcode value | String | Must be unique if provided | No |

### Bills (`BillModel`)

| Field | Description | Format | Validation | Required |
|-------|-------------|--------|------------|----------|
| `id` | Unique bill ID | Number | Auto-generated | - |
| `name` | Customer name | String (1-100 chars) | - | Yes |
| `mobile` | Customer mobile | String | 10 digits | No |
| `paymentMode` | Payment method | Enum | CASH/CARD/UPI | No |
| `billDate` | Transaction date | Date | Valid date | Yes |
| `discount` | Discount amount | Number | Must be ≥ 0 | No |
| `advance` | Advance payment | Number | Must be ≥ 0 | No |

### Bill Items (`BillItemModel`)

| Field | Description | Format | Validation | Required |
|-------|-------------|--------|------------|----------|
| `itemId` | Reference to inventory item | Number | Must exist in inventory | Yes |
| `quantity` | Item quantity | Number | Must be > 0 | Yes |
| `amount` | Unit price | Number | Must be > 0 | Yes |
| `itemName` | Item display name | String | Auto-generated | No |
| `itemTotal` | Line total | Number | quantity × amount | No |

### Vendors (`VendorModel`)

| Field | Description | Format | Validation | Required |
|-------|-------------|--------|------------|----------|
| `id` | Unique vendor ID | Number | Auto-generated | - |
| `name` | Vendor name | String (1-100 chars) | - | Yes |
| `mobile` | Contact number | String | 10 digits | Yes |
| `createdDate` | Registration date | Date | Auto-generated | No |

### Vendor Transactions (`VendorAccountModel`)

| Field | Description | Format | Validation | Required |
|-------|-------------|--------|------------|----------|
| `id` | Transaction ID | Number | Auto-generated | - |
| `supplierId` | Vendor reference | Number | Must exist | Yes |
| `description` | Transaction details | String (max 200 chars) | - | Yes |
| `amount` | Transaction amount | Number | Must be > 0 | Yes |
| `expenseType` | Transaction type | Enum | CREDIT/DEBIT | Yes |
| `paymentMode` | Payment method | Enum | CASH/CARD/UPI | No |
| `date` | Transaction date | Date | Valid date | Yes |

### Customers (`CustomerModel`)

| Field | Description | Format | Validation | Required |
|-------|-------------|--------|------------|----------|
| `name` | Customer name | String (1-100 chars) | - | Yes |
| `mobile` | Customer mobile | String | 10 digits | Yes |

---

## Usage Tips

### Best Practices
1. **Regular Backups**: Export data regularly for safety
2. **Consistent Naming**: Use consistent product naming conventions
3. **Inventory Updates**: Keep stock quantities updated after each sale
4. **Customer Data**: Collect customer mobile numbers for WhatsApp integration
5. **Expense Tracking**: Record all business expenses promptly

### Keyboard Shortcuts
- **Barcode Input**: Press Enter after scanning to add item
- **Quick Search**: Use Ctrl+F to search within lists
- **Tab Navigation**: Use Tab key to move between form fields

### Mobile Usage
- **Responsive Design**: App works on all screen sizes  
- **Touch Friendly**: Large buttons and touch targets
- **Offline Capability**: Limited offline functionality for critical operations

This knowledge base should provide comprehensive guidance for users and AI assistants supporting this inventory management application.
