#!/bin/bash

cat << 'EOF'
================================================================================
                    SQL STUDIO PRO - WINDOWS ONLY APPLICATION
================================================================================

⚠️  THIS APPLICATION CANNOT RUN IN THIS ENVIRONMENT ⚠️

This is a Windows Presentation Foundation (WPF) desktop application that
requires Windows 10/11 to run.

Replit's environment is Linux-based and does not support WPF applications.

TO USE THIS APPLICATION:
------------------------
1. Download all project files to your Windows computer
2. Install .NET 8 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0
3. Open a terminal in the project folder
4. Run these commands:

   dotnet restore SqlStudioPro.sln
   dotnet build SqlStudioPro.sln
   cd src/UI.Wpf
   dotnet run

OR use Visual Studio 2022:
   - Open SqlStudioPro.sln
   - Press F5 to run

FEATURES INCLUDED:
------------------
✓ SQL Server connection management with secure DPAPI encryption
✓ Database schema browser (tables, views, stored procedures)
✓ Multi-tab SQL query editor with syntax highlighting (AvalonEdit)
✓ Entity Framework LINQ query execution
✓ Natural Language to SQL translation (AI-powered)
✓ Query results grid with export to CSV
✓ SSMS-style docking layout
✓ Light/Dark themes (MaterialDesign)
✓ Complete MVVM architecture with Dependency Injection

For detailed instructions, open README.md

================================================================================
EOF
