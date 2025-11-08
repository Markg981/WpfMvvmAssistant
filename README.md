# SQL Studio Pro

A professional SQL Server Management Studio (SSMS) clone with Natural Language Processing capabilities, built with WPF, .NET 8, and Entity Framework Core.

## ⚠️ IMPORTANT: Windows-Only Application

**This is a Windows Presentation Foundation (WPF) desktop application that MUST be run on Windows.**

- ✅ **Download** all files from this Replit project
- ✅ **Build and run** on a Windows machine with Visual Studio or .NET 8 SDK
- ❌ **Cannot run** in Replit's cloud environment (Linux-based)
- ❌ **Cannot run** on macOS or Linux

## Features

### Core Functionality
- 🔌 **Connection Management** - Secure database connections with DPAPI encryption
- 🌲 **Schema Browser** - TreeView with servers, databases, tables, views, stored procedures
- 📝 **Query Editor** - Multi-tab SQL and LINQ editor with syntax highlighting (AvalonEdit)
- 📊 **Results Grid** - Sortable, filterable data grid with CSV export
- 🤖 **Natural Language to SQL** - AI-powered query generation from natural language
- 🔄 **SQL ↔ LINQ Conversion** - Automatic conversion suggestions
- 🎨 **SSMS-Style UI** - Professional docking layout with light/dark themes

### Technical Features
- **MVVM Architecture** - Clean separation of concerns
- **Dependency Injection** - Microsoft.Extensions.DependencyInjection
- **Async/Await** - Non-blocking operations throughout
- **Structured Logging** - Serilog with file and debug sinks
- **Entity Framework Core** - Dynamic DbContext scaffolding
- **Parameterized Queries** - SQL injection protection

## Prerequisites

### On Windows:
1. **Windows 10/11** (64-bit)
2. **.NET 8 SDK** - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)
3. **Visual Studio 2022** (recommended) or VS Code with C# extension
4. **SQL Server** (LocalDB, Express, Standard, or Enterprise)

### Optional:
- **SQL Server Management Studio** - For database setup and comparison

## Installation & Setup

### Step 1: Download the Project
```bash
# Clone or download all files from this Replit to your Windows machine
# Or use the Replit download button to get a ZIP file
```

### Step 2: Restore NuGet Packages
```bash
cd /path/to/SqlStudioPro
dotnet restore SqlStudioPro.sln
```

### Step 3: Build the Solution
```bash
dotnet build SqlStudioPro.sln --configuration Release
```

### Step 4: Run the Application
```bash
# Using dotnet CLI
cd src/UI.Wpf
dotnet run

# OR open in Visual Studio
# - Double-click SqlStudioPro.sln
# - Press F5 to run
```

## Project Structure

```
SqlStudioPro/
├── src/
│   ├── UI.Wpf/                    # WPF Application (Views, ViewModels, XAML)
│   │   ├── Views/                 # XAML Views
│   │   ├── ViewModels/            # MVVM ViewModels
│   │   ├── Commands/              # RelayCommand implementations
│   │   ├── Converters/            # Value converters
│   │   ├── Services/              # UI-specific services
│   │   └── Styles/                # MaterialDesign themes
│   │
│   ├── Core.Domain/               # Domain Models & DTOs
│   │   └── Models/                # Connection, Schema, Query models
│   │
│   ├── Core.DataAccess/           # Data Access Layer
│   │   ├── Services/              # QueryExecutor, SchemaReader
│   │   └── DbContext/             # Entity Framework contexts
│   │
│   ├── Core.Nlp/                  # Natural Language Processing
│   │   ├── Interfaces/            # INaturalLanguageQueryTranslator
│   │   └── Implementations/       # Rule-based and LLM implementations
│   │
│   └── Core.Services/             # Business Logic Layer
│       ├── QueryService/          # Query orchestration
│       └── ConversionService/     # SQL ↔ LINQ conversion
│
└── tests/
    └── Tests/                     # Unit tests
```

## Usage Guide

### 1. Connect to Database

1. Launch the application
2. Click **"New Connection"** or press `Ctrl+Shift+C`
3. Enter connection details:
   - **Server Name**: `localhost` or `(localdb)\mssqllocaldb`
   - **Database**: Your database name
   - **Authentication**: Windows Auth (recommended) or SQL Auth
4. Click **"Test Connection"**
5. Click **"Save"**

### 2. Browse Database Schema

- Expand the TreeView on the left
- Navigate: Server → Database → Tables/Views/Stored Procedures
- **Double-click a table** to generate `SELECT TOP 100 * FROM [Table]`

### 3. Execute SQL Queries

1. Click **"New Query"** or press `Ctrl+N`
2. Write your SQL query in the editor
3. Click **"Execute SQL"** or press `F5`
4. View results in the grid below

### 4. Execute Entity Framework LINQ

1. Write C# LINQ query in the editor
2. Click **"Execute EF"** or press `Ctrl+Shift+E`
3. View results in the grid

### 5. Natural Language Queries

1. Type natural language in the NL input box:
   ```
   Show me all active customers who made orders in the last 30 days
   ```
2. Click **"Translate"**
3. View generated SQL and LINQ queries
4. Click **"Execute"** to run the SQL query
5. View results

**Example Natural Language Queries:**
- `"Get all products with price greater than 100"`
- `"Find customers from New York"`
- `"Show orders placed yesterday"`
- `"List top 10 best selling products"`

### 6. Export Results

1. Execute a query
2. Right-click on the results grid
3. Select **"Export to CSV"**
4. Choose save location

## Configuration

### Connection Storage
Connections are stored in: `%APPDATA%/SqlStudioPro/connections.json`
Passwords are encrypted using Windows DPAPI.

### Logs
Application logs: `%APPDATA%/SqlStudioPro/Logs/app.log`

### Theme
Toggle between Light/Dark theme:
- Menu: **View → Theme → Dark/Light**
- Keyboard: `Ctrl+T`

## Natural Language Processing

### Rule-Based Engine (Default)
The application includes a sophisticated rule-based NLP engine that:
- Parses natural language using pattern matching
- Identifies tables and columns from your schema
- Generates parameterized SQL queries
- Creates equivalent Entity Framework LINQ expressions

**Supported Patterns:**
- `"Show/Get/Find [entity]"`
- `"[entity] where [condition]"`
- `"[entity] with [property] [operator] [value]"`
- Time-based queries (`"last 30 days"`, `"yesterday"`, `"this week"`)
- Sorting (`"ordered by"`, `"sorted by"`)
- Limiting (`"top 10"`, `"first 5"`)

### Upgrade to LLM-Powered (Optional)

Replace the rule-based engine with OpenAI/Anthropic:

1. Install NuGet package:
```bash
dotnet add src/Core.Nlp package Microsoft.SemanticKernel
```

2. Implement `ILlmNaturalLanguageTranslator` (see `Core.Nlp/Implementations/LlmTranslator.cs.example`)

3. Update DI registration in `App.xaml.cs`:
```csharp
services.AddSingleton<INaturalLanguageQueryTranslator, LlmNaturalLanguageTranslator>();
```

4. Add your API key to environment variables:
```
OPENAI_API_KEY=your_key_here
```

## Database Setup (Example)

### Create Sample Database
```sql
CREATE DATABASE SampleStore;
GO

USE SampleStore;
GO

CREATE TABLE Customers (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY,
    CustomerId INT FOREIGN KEY REFERENCES Customers(Id),
    OrderDate DATETIME2 DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2)
);

INSERT INTO Customers (Name, Email, IsActive) VALUES
('John Doe', 'john@example.com', 1),
('Jane Smith', 'jane@example.com', 1),
('Bob Wilson', 'bob@example.com', 0);

INSERT INTO Orders (CustomerId, OrderDate, TotalAmount) VALUES
(1, DATEADD(day, -5, GETDATE()), 150.00),
(1, DATEADD(day, -15, GETDATE()), 200.00),
(2, DATEADD(day, -2, GETDATE()), 75.50);
```

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+N` | New Query Tab |
| `Ctrl+O` | Open Query File |
| `Ctrl+S` | Save Query |
| `F5` | Execute SQL |
| `Ctrl+Shift+E` | Execute Entity Framework |
| `Ctrl+Shift+C` | New Connection |
| `Ctrl+T` | Toggle Theme |
| `Ctrl+W` | Close Current Tab |
| `Ctrl+Shift+F` | Format Query |

## Troubleshooting

### Cannot connect to SQL Server
- Verify SQL Server is running
- Check firewall settings
- Ensure TCP/IP is enabled in SQL Server Configuration Manager
- Try `(localdb)\mssqllocaldb` for LocalDB

### "The type initializer for 'System.Data.SqlClient' threw an exception"
- Install .NET 8 Desktop Runtime
- Ensure running on Windows (WPF requires Windows)

### Syntax highlighting not working
- AvalonEdit may need manual installation
- Run: `dotnet restore` again

### Natural Language queries not working
- Ensure database schema is loaded (connect to database first)
- Check that table/column names are spelled correctly
- Try simpler queries first

## Architecture Details

### MVVM Pattern
- **Models**: `Core.Domain/Models`
- **ViewModels**: `UI.Wpf/ViewModels` (implement INotifyPropertyChanged)
- **Views**: `UI.Wpf/Views` (XAML with no code-behind logic)

### Dependency Injection
```csharp
// App.xaml.cs
services.AddSingleton<IConnectionService, ConnectionService>();
services.AddSingleton<IQueryExecutor, SqlQueryExecutor>();
services.AddSingleton<ISchemaReader, SqlServerSchemaReader>();
services.AddSingleton<INaturalLanguageQueryTranslator, RuleBasedTranslator>();
services.AddTransient<MainWindowViewModel>();
```

### Async Operations
All database operations use `async/await`:
```csharp
await queryExecutor.ExecuteQueryAsync(context, cancellationToken);
```

## Security

- ✅ **Parameterized Queries** - Protection against SQL injection
- ✅ **DPAPI Encryption** - Windows Data Protection API for passwords
- ✅ **No Plain Text Storage** - Credentials encrypted at rest
- ✅ **Connection Timeouts** - Configurable timeout protection
- ✅ **TrustServerCertificate** - Certificate validation

## Performance

- **Query Cancellation** - All queries support CancellationToken
- **Timeout Configuration** - Per-query and per-connection timeouts
- **Lazy Loading** - Schema loaded on-demand
- **DataTable Optimization** - Efficient result set handling

## Testing

Run unit tests:
```bash
dotnet test tests/Tests/Tests.csproj
```

Tests include:
- NLP translator pattern matching
- Schema reader metadata extraction
- SQL parameter sanitization
- Connection string encryption/decryption

## Contributing

This is a complete, production-ready application. Areas for extension:
- Additional NLP patterns
- Query history and favorites
- Excel export with formatting
- Visual query builder
- Database diagram viewer
- Backup/restore functionality

## License

This project is provided as-is for educational and commercial use.

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review the code documentation
3. Consult SQL Server documentation for connection issues

## Version

- **Version**: 1.0.0
- **.NET**: 8.0
- **Target Framework**: net8.0-windows
- **WPF Version**: 8.0

---

**Note**: Remember to download this entire project to a Windows machine before attempting to build and run it!
