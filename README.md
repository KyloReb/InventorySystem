# 📦 Inventory Management System

A **Windows Forms application** for managing supplies and assets inventory, built with **C# (.NET Framework 4.7.2)**.
This project provides a user-friendly interface for viewing, editing, and tracking inventory data — complete with user roles, logging, and SQL Server connectivity.

---

## 🚀 Features

* **View Inventory:** Display supplies and assets tables from a SQL Server database.
* **Edit Mode:** Admin users can add, edit, and delete records with change tracking.
* **User Roles:** Supports Admin, Guest, and other roles with permission-based UI.
* **Logging:** Logs all user actions and errors for auditing and troubleshooting.
* **Progress Feedback:** Status bar and progress bar provide real-time feedback.
* **Configurable Settings:** Table names and connection strings can be customized via `App.config`.

---

## 🧰 Technologies Used

* **Language:** C# 7.3
* **Framework:** .NET Framework 4.7.2
* **UI:** Windows Forms (WinForms)
* **Database:** SQL Server (`System.Data.SqlClient`)
* **Configuration:** `App.config`

---

## ⚙️ Getting Started

### 📝 Prerequisites

Before running the application, make sure you have:

* **Visual Studio 2022** or later
* **SQL Server** instance with required inventory tables
* **.NET Framework 4.7.2** installed

---

### 🧩 Setup Instructions

#### 1. **Clone the Repository**

```bash
git clone https://github.com/yourusername/inventory-management-system.git
```

#### 2. **Configure the Database**

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

#### 3. **Build and Run the Project**

1. Open the solution in **Visual Studio**.
2. Build the project with `Ctrl + Shift + B`.
3. Run the application with `F5`.

---

## 💡 Usage Guide

### 🔐 1. Login

* Enter your **username** and **password**.
* The system loads the dashboard with features based on your assigned role.

### 📋 2. View Inventory

* Use the **menu** to switch between **Supplies** and **Assets**.
* Data is displayed in an interactive **DataGridView**.

### ✏️ 3. Edit Inventory *(Admin Only)*

* Click **Edit Supplies** or **Edit Assets** to enable edit mode.
* Add, modify, or delete records via context menu options.
* Save or discard changes as needed.

### 🧾 4. View Logs

* All user actions and system errors are logged.
* Admin users can view logs through the **Logs** menu option.

### 🚪 5. Exit or Logout

* Use the menu options to **logout** or **exit**.
* If there are unsaved changes, a confirmation prompt will appear.

---

## 🗂️ Project Structure

| File / Folder                                         | Description                                              |
| ----------------------------------------------------- | -------------------------------------------------------- |
| `InventorySystem/ViewFrm.cs`                          | Main form for viewing and editing inventory data         |
| `InventorySystem/Services/DatabaseService.cs`         | Handles SQL Server operations                            |
| `InventorySystem/Services/DataGridViewEditService.cs` | Manages grid editing and change tracking                 |
| `InventorySystem/Services/LoggingService.cs`          | Handles action and error logging                         |
| `InventorySystem/LoginFrm.cs`                         | Login form for authentication and role validation        |
| `InventorySystem/App.config`                          | Application configuration (connection strings, settings) |

---

## 🤝 Contributing

Pull requests are welcome!
For major changes, please open an **issue** first to discuss what you’d like to modify or improve.

---

## 📜 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

### 💬 Support

For questions or support, please open an **issue** on [GitHub](https://github.com/yourusername/inventory-management-system/issues).

---

✅ **Summary of Content:**

* Project Overview & Features
* Technologies Used
* Setup & Configuration
* Usage Guide
* File Structure
* Contribution & License

---

Would you like me to add **badges** (like build status, license, or .NET version) and a **screenshots/demo section** to make it look even more professional on GitHub?
