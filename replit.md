# SQL Studio Pro - Replit Project

## ⚠️ IMPORTANT: Windows-Only Application

This project contains a **Windows Presentation Foundation (WPF) desktop application** that is designed specifically for Windows 10/11. It **CANNOT run** in Replit's Linux-based cloud environment.

## Project Overview

**SQL Studio Pro** is a professional SQL Server Management Studio (SSMS) clone with advanced Natural Language Processing capabilities. It provides a comprehensive interface for managing SQL Server databases, executing queries, and translating natural language into SQL.

### Key Features
- **Connection Management**: Secure database connections with DPAPI encryption
- **Schema Browser**: TreeView display of databases, tables, views, stored procedures
- **Multi-Tab Query Editor**: SQL and LINQ query execution with syntax highlighting
- **NLP to SQL**: AI-powered natural language to SQL/LINQ translation
- **Results Grid**: Sortable, filterable data with CSV export
- **Modern UI**: Material Design themes with light/dark mode

### Technology Stack
- **.NET 8** (net8.0-windows)
- **WPF** (Windows Presentation Foundation)
- **Entity Framework Core 8**
- **Microsoft.Data.SqlClient**
- **AvalonEdit** (syntax highlighting)
- **MaterialDesignThemes** (UI framework)
- **Serilog** (logging)

## Project Structure

```
SqlStudioPro/
├── src/
│   ├── Core.Domain/          - Domain models, DTOs, entities
│   ├── Core.DataAccess/      - Data access, QueryExecutor, SchemaReader
│   ├── Core.Nlp/             - Natural language processing module
│   ├── Core.Services/        - Business logic and orchestration
│   └── UI.Wpf/               - WPF application, MVVM views
├── tests/
│   └── Tests/                - Unit tests (xUnit)
├── SqlStudioPro.sln          - Visual Studio solution file
└── README.md                 - Complete documentation

```

## Architecture

### Multi-Layered Design
1. **UI.Wpf**: Presentation layer using MVVM pattern
2. **Core.Services**: Business logic orchestration
3. **Core.DataAccess**: Data access and database operations
4. **Core.Nlp**: Natural language processing
5. **Core.Domain**: Shared models and DTOs

### Key Patterns
- **MVVM**: Model-View-ViewModel separation
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Async/Await**: Non-blocking operations throughout
- **Repository Pattern**: Data access abstraction

## How to Use This Project

### Download & Build on Windows

1. **Download** all files from this Replit project to your Windows computer

2. **Install Prerequisites**:
   - [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Visual Studio 2022 (recommended) or VS Code

3. **Build the Solution**:
   ```bash
   cd path/to/SqlStudioPro
   dotnet restore SqlStudioPro.sln
   dotnet build SqlStudioPro.sln --configuration Release
   ```

4. **Run the Application**:
   ```bash
   cd src/UI.Wpf
   dotnet run
   ```

   OR open `SqlStudioPro.sln` in Visual Studio and press F5

### Requirements
- Windows 10 or Windows 11 (64-bit)
- .NET 8 SDK or Runtime
- SQL Server (LocalDB, Express, Standard, or Enterprise)

## Natural Language Processing

The application includes a sophisticated **rule-based NLP engine** that:
- Parses natural language using pattern matching
- Identifies tables and columns from your database schema
- Generates parameterized SQL queries
- Creates equivalent Entity Framework LINQ expressions

### Example NLP Queries
- `"Show me all active customers"`
- `"Get orders from the last 30 days"`
- `"Find customers where email contains gmail"`
- `"Top 10 products ordered by price"`

### Upgrading to LLM-Powered NLP
The NLP module uses a pluggable architecture (`INaturalLanguageQueryTranslator`). You can replace the rule-based implementation with OpenAI/Anthropic:

1. Install Semantic Kernel package
2. Implement `ILlmNaturalLanguageTranslator`
3. Update DI registration in `App.xaml.cs`

## Security Features

- ✅ **DPAPI Encryption** - Windows Data Protection API for passwords
- ✅ **Parameterized Queries** - SQL injection protection
- ✅ **Connection Timeouts** - Configurable timeout settings
- ✅ **No Plaintext Storage** - Credentials encrypted at rest

## Database Setup Example

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
```

## Testing

Run unit tests:
```bash
dotnet test tests/Tests/Tests.csproj
```

Tests include:
- NLP translator pattern matching
- Connection service operations
- Schema reader metadata extraction
- Query execution and results

## Recent Changes

**2024-11-08**: Initial project creation
- Complete multi-layered architecture implemented
- MVVM pattern with dependency injection
- Rule-based NLP translator
- Material Design UI with theming
- Comprehensive unit tests
- Full documentation

## Known Limitations

1. **Windows-Only**: WPF requires Windows OS
2. **Cannot Run in Replit**: Linux-based environment incompatible
3. **EF Execution**: Dynamic LINQ execution not yet implemented (use SQL instead)
4. **NLP Accuracy**: Rule-based engine has limited patterns (upgrade to LLM for better results)

## Future Enhancements

- Visual query builder
- Query history and favorites
- Excel export with formatting
- Database diagram viewer
- Backup/restore functionality
- Stored procedure debugging

## Logs & Configuration

- **Connection Storage**: `%APPDATA%/SqlStudioPro/connections.json`
- **Application Logs**: `%APPDATA%/SqlStudioPro/Logs/app.log`

## Support & Troubleshooting

See README.md for:
- Complete installation instructions
- Troubleshooting guide
- Keyboard shortcuts
- Feature documentation

## License

This project is provided as-is for educational and commercial use.

## Version

- **Version**: 1.0.0
- **.NET**: 8.0
- **Target Framework**: net8.0-windows
- **Build Date**: November 8, 2024

---

**Remember**: This is a Windows desktop application. Download the project to your Windows computer to build and run it!
