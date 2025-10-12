# 📦 Inventory Management System

A **Windows Forms application** for managing supplies and assets inventory, built with **C# (.NET Framework 4.7.2)**.
This project provides a user-friendly interface for viewing, editing, and tracking inventory data — complete with **user roles**, **logging**, and **SQL Server connectivity**.

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

## 🪵 Log Folder and Log File Location

The application automatically creates and manages **log files** for auditing and troubleshooting.
The log folder and file path are configurable via `App.config` and managed by the `LoggingService`.

---

### 📂 Default Log Location

* **Log Folder:**
  By default, logs are stored in your system’s temporary directory under `InventorySystem_Logs`.

  **Example:**

  ```
  C:\Users\<YourUsername>\AppData\Local\Temp\InventorySystem_Logs\
  ```

* **Log File Name:**
  Each session creates a new log file with a timestamp, e.g.:

  ```
  InventorySystem_Log_20251012_153000.txt
  ```

---

### ⚙️ Configuration (`App.config`)

You can customize the log folder and file naming in the `<loggingConfiguration>` section:

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
| **LogFolderPath**      | Base path for logs (supports environment variables like `%TEMP%`). |
| **LogFolderName**      | Subfolder name for storing logs.                                   |
| **LogFileNamePrefix**  | Prefix used for each log file.                                     |
| **LogFileExtension**   | File extension for logs (e.g., `.txt`).                            |
| **AutoCleanupOldLogs** | Enables automatic deletion of old logs.                            |
| **LogRetentionDays**   | Number of days to retain old log files.                            |

---

### ⚙️ How Logging Works

1. On startup, the application creates the log folder if it does not exist.
2. Each run generates a **new log file** with a unique timestamp.
3. All actions, errors, and events are written to the current log file.
4. Old logs are automatically deleted based on the retention settings.

---

### 📑 Accessing Logs

* Open the **latest log file** or **log folder** directly from the application’s menu.
* Log files are **plain text (.txt)** and can be opened using any text editor.

---

## 🗂️ Project Structure

| File / Folder                                         | Description                                              |
| ----------------------------------------------------- | -------------------------------------------------------- |
| `InventorySystem/ViewFrm.cs`                          | Main form for viewing and editing inventory data         |
| `InventorySystem/Services/DatabaseService.cs`         | Handles SQL Server operations                            |
| `InventorySystem/Services/DataGridViewEditService.cs` | Manages grid editing and change tracking                 |
| `InventorySystem/Services/LoggingService.cs`          | Handles application logging                              |
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

## 💬 Support

For questions or support, please open an **issue** on [GitHub](https://github.com/yourusername/inventory-management-system/issues).

---

✅ **Summary of Content**

* Project Overview & Features
* Technologies Used
* Setup & Configuration
* Usage Guide
* Logging Configuration & Access
* Project Structure
* Contribution & License

---

Would you like me to add a short **“Screenshots / Demo”** section (with example placeholders for images) to make your GitHub page more visual and professional?
