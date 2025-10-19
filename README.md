# 📦 Inventory Management System

A **Windows Forms application** for managing supplies and assets inventory, built with **C# (.NET Framework 4.7.2)**. This project provides a comprehensive interface for viewing, editing, searching, and exporting inventory data — complete with **user roles**, **logging**, **SQL Server connectivity**, and **advanced export capabilities**.

---

## 🚀 Features

### 🔍 **Data Management**
* **View Inventory:** Display supplies and assets tables from a SQL Server database
* **Smart Search:** Real-time search across all columns with instant filtering
* **Edit Mode:** Admin users can add, edit, and delete records with change tracking
* **Data Refresh:** Manual refresh to get latest data from database

### 👥 **User & Security**
* **User Roles:** Supports Admin, Guest, and other roles with permission-based UI
* **Authentication:** Secure login system with role-based access control
* **Audit Logging:** Comprehensive logging of all user actions and system events

### 🖨️ **Print & Export**
* **Print Preview:** Preview reports before printing with pagination
* **Print Dialog:** Professional printing with page setup options
* **Multiple Export Formats:**
  - **Excel (.xlsx):** Full formatting with auto-fit columns
  - **PDF (.pdf):** Professional reports with headers and styling
  - **CSV (.csv):** Comma-separated values for data analysis
  - **Images:** PNG, JPEG, BMP formats for screenshots

### 🎯 **User Experience**
* **Progress Feedback:** Status bar and progress bar provide real-time feedback
* **Search Highlight:** Visual feedback for active search filters
* **Form Auto-sizing:** Dynamic form width adjustment based on data
* **Unsaved Changes Protection:** Prevents data loss with confirmation prompts

---

## 🧰 Technologies Used

* **Language:** C# 7.3
* **Framework:** .NET Framework 4.7.2
* **UI:** Windows Forms (WinForms)
* **Database:** SQL Server (`System.Data.SqlClient`)
* **Export Libraries:** 
  - **EPPlus** for Excel export
  - **iText 7** for PDF generation
* **Configuration:** `App.config`

---

## ⚙️ Getting Started

### 📝 Prerequisites

Before running the application, make sure you have:

* **Visual Studio 2022** or later
* **SQL Server** instance with required inventory tables
* **.NET Framework 4.7.2** installed
* **NuGet Packages:**
  - EPPlus (for Excel export)
  - iText7 (for PDF export)

---

### 🧩 Setup Instructions

#### 1. **Clone the Repository**

```bash
git clone https://github.com/yourusername/inventory-management-system.git
```

#### 2. **Install Required NuGet Packages**

```bash
Install-Package EPPlus
Install-Package itext7
```

#### 3. **Configure the Database**

Update your connection string in `InventorySystem/App.config`:

```xml
<connectionStrings>
  <add name="InventoryManagementConnection"
       connectionString="Data Source=YOUR_SERVER;Initial Catalog=YOUR_DB;Integrated Security=True" />
</connectionStrings>
```

Optionally, set custom table names under the `appSettings` section:

```xml
<appSettings>
  <add key="SuppliesTable" value="SuppliesInventory" />
  <add key="AssetsTable" value="AssetsInventory" />
</appSettings>
```

#### 4. **Build and Run the Project**

1. Open the solution in **Visual Studio**
2. Build the project with `Ctrl + Shift + B`
3. Run the application with `F5`

---

## 💡 Usage Guide

### 🔐 1. Login

* Enter your **username** and **password**
* The system loads the dashboard with features based on your assigned role

### 🔍 2. Search & Filter Data

* **Real-time Search:** Type in the search box to instantly filter results
* **Multi-column Search:** Searches across all visible columns
* **Clear Search:** Press `ESC` or clear the search box to show all records
* **Search Status:** Status bar shows number of matching records

### 📋 3. View Inventory

* Use the **menu** to switch between **Supplies** and **Assets**
* Data is displayed in an interactive **DataGridView**
* **Auto-sized Columns:** Form width adjusts to fit data content

### ✏️ 4. Edit Inventory *(Admin Only)*

* Click **Edit Supplies** or **Edit Assets** to enable edit mode
* **Add Records:** Use "Add New" from menu or context menu
* **Edit Records:** Double-click cells or use context menu
* **Delete Records:** Select rows and use delete option
* **Save/Discard:** Save changes to database or discard modifications

### 🖨️ 5. Print & Export

#### **Printing Options:**
* **Print Preview:** See how the report will look before printing
* **Page Setup:** Configure margins, orientation, and paper size
* **Print Dialog:** Select printer and print options

#### **Export Formats:**
* **Excel (.xlsx):** Perfect for data analysis and reporting
* **PDF (.pdf):** Professional documents with headers and formatting
* **CSV (.csv):** Compatible with spreadsheet applications
* **Images:** PNG, JPEG, BMP formats for presentations

### 🧾 6. View Logs

* All user actions and system errors are logged
* Admin users can view logs through the **Logs** menu option

### 🚪 7. Exit or Logout

* Use the menu options to **logout** or **exit**
* If there are unsaved changes, a confirmation prompt will appear

---

## 🖨️ Print & Export Features

### **Print Export Service**
The `PrintExportService` class provides comprehensive printing and export capabilities:

```csharp
// Initialize service
printExportService = new PrintExportService(dataGridView1, loggingService);

// Set report title
printExportService.SetTitle("Custom Report Title");

// Export to various formats
printExportService.ExportToExcel(filePath);
printExportService.ExportToPdf(filePath);
printExportService.ExportToCsv(filePath);
printExportService.ExportToImage(format, filePath);

// Printing options
printExportService.ShowPrintPreview();
printExportService.ShowPrintDialog();
printExportService.ShowPageSetup();
```

### **PDF Formatter Service**
Professional PDF generation with `PdfFormatterService`:

* **Landscape A4** page layout for optimal data display
* **Styled Headers** with company branding
* **Column Alignment** based on data types (right for numbers, left for text)
* **Alternating Row Colors** for better readability
* **Auto-generated Timestamps** and page numbers

### **Excel Export Features**
* **Auto-fit Columns** for optimal data display
* **Bold Headers** for clear column identification
* **EPPlus License Management** for non-commercial use
* **Large Dataset Support** with progress tracking

---

## 🔍 Search Functionality

### **Smart Search Implementation**
```csharp
// Real-time search across all columns
PerformSearch(string searchText);

// Case-insensitive filtering
DataTable filteredTable = dataLoadingService.PerformSearch(originalDataTable, searchText);

// Visual search status
isSearchActive = true;
toolStripStatusLabel1.Text = $"Found {dataTable.Rows.Count} records matching '{searchText}'";
```

### **Search Features**
* **Real-time Filtering:** Results update as you type
* **Multi-field Search:** Searches across all visible columns
* **Keyboard Shortcuts:**
  - `Enter`: Execute search
  - `Escape`: Clear search
* **Search Status:** Clear indication of filtered vs. total records
* **Export Filtered Data:** Print/export only filtered results

---

## 🪵 Log Folder and Log File Location

The application automatically creates and manages **log files** for auditing and troubleshooting. The log folder and file path are configurable via `App.config` and managed by the `LoggingService`.

### 📂 Default Log Location

* **Log Folder:**
  By default, logs are stored in your system's temporary directory under `InventorySystem_Logs`.

  **Example:**
  ```
  C:\Users\<YourUsername>\AppData\Local\Temp\InventorySystem_Logs\
  ```

* **Log File Name:**
  Each session creates a new log file with a timestamp, e.g.:
  ```
  InventorySystem_Log_20251012_153000.txt
  ```

### ⚙️ Configuration (`App.config`)

Customize logging behavior in the `<loggingConfiguration>` section:

```xml
<loggingConfiguration>
  <add key="LogFolderPath" value="%TEMP%\InventorySystem_Logs" />
  <add key="LogFolderName" value="InventorySystem_Logs" />
  <add key="LogFileNamePrefix" value="InventorySystem_Log" />
  <add key="LogFileExtension" value=".txt" />
  <add key="AutoCleanupOldLogs" value="true" />
  <add key="LogRetentionDays" value="30" />
</loggingConfiguration>
```

**Configuration Keys:**

| Key                    | Description                                                        |
| ---------------------- | ------------------------------------------------------------------ |
| **LogFolderPath**      | Base path for logs (supports environment variables like `%TEMP%`)  |
| **LogFolderName**      | Subfolder name for storing logs                                    |
| **LogFileNamePrefix**  | Prefix used for each log file                                      |
| **LogFileExtension**   | File extension for logs (e.g., `.txt`)                            |
| **AutoCleanupOldLogs** | Enables automatic deletion of old logs                            |
| **LogRetentionDays**   | Number of days to retain old log files                            |

### ⚙️ How Logging Works

1. On startup, the application creates the log folder if it does not exist
2. Each run generates a **new log file** with a unique timestamp
3. All actions, errors, search queries, and export operations are logged
4. Old logs are automatically deleted based on retention settings

### 📑 Accessing Logs

* Open the **latest log file** or **log folder** directly from the application's menu
* Log files are **plain text (.txt)** and can be opened using any text editor
* Search for specific operations using timestamps and category tags

---

## 🗂️ Project Structure

| File / Folder                                         | Description                                              |
| ----------------------------------------------------- | -------------------------------------------------------- |
| `InventorySystem/ViewFrm.cs`                          | Main form with search, print, and export features        |
| `InventorySystem/Services/PrintExportService.cs`      | Handles all printing and export operations               |
| `InventorySystem/Services/PdfFormatterService.cs`     | Professional PDF report generation                       |
| `InventorySystem/Services/DatabaseService.cs`         | Handles SQL Server operations                            |
| `InventorySystem/Services/DataGridViewEditService.cs` | Manages grid editing and change tracking                 |
| `InventorySystem/Services/DataLoadingService.cs`      | Handles data loading and search functionality            |
| `InventorySystem/Services/LoggingService.cs`          | Handles application logging                              |
| `InventorySystem/LoginFrm.cs`                         | Login form for authentication and role validation        |
| `InventorySystem/App.config`                          | Application configuration (connection strings, settings) |

---

## 🎯 Key Service Classes

### **PrintExportService**
- Manages all print and export operations
- Supports multiple formats (Excel, PDF, CSV, Images)
- Handles print preview and page setup
- Integrated progress tracking and error handling

### **PdfFormatterService**
- Generates professional PDF reports
- Landscape layout optimized for data tables
- Automatic header/footer with timestamps
- Styled tables with proper alignment

### **DataLoadingService**
- Efficient data loading with progress reporting
- Real-time search across multiple columns
- Data refresh capabilities
- Connection management and validation

---

## 🤝 Contributing

Pull requests are welcome! For major changes, please open an **issue** first to discuss what you'd like to modify or improve.

### **Areas for Contribution**
- Additional export formats
- Enhanced search filters
- Report templates
- Dashboard analytics
- Mobile-responsive web version

---

## 📜 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

## 💬 Support

For questions or support, please open an **issue** on [GitHub](https://github.com/yourusername/inventory-management-system/issues).

---

## 🆕 Recent Updates

### **v2.0 - Print & Export Release**
- ✅ Added comprehensive printing system
- ✅ Multiple export formats (Excel, PDF, CSV, Images)
- ✅ Real-time search functionality
- ✅ Professional PDF report generation
- ✅ Enhanced user interface with progress tracking
- ✅ Improved error handling and logging

### **v1.0 - Core Features**
- ✅ Basic inventory management
- ✅ User authentication and roles
- ✅ SQL Server integration
- ✅ Data editing capabilities
- ✅ Basic logging system
---

*Inventory Management System - Professional inventory tracking with advanced export capabilities*
