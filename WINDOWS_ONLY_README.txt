================================================================================
                    SQL STUDIO PRO - WINDOWS ONLY APPLICATION
================================================================================

⚠️ IMPORTANT NOTICE ⚠️

This is a Windows Presentation Foundation (WPF) desktop application built with
.NET 8 that CANNOT run in Replit's Linux-based cloud environment.

WHY THIS CANNOT RUN HERE:
-------------------------
• WPF (Windows Presentation Foundation) is a Windows-only UI framework
• Requires .NET Desktop Runtime (not available on Linux)
• Uses Windows-specific APIs (DPAPI for encryption, Windows UI components)
• Designed specifically for Windows 10/11 operating systems

HOW TO USE THIS APPLICATION:
---------------------------
1. Download all files from this Replit project
2. Transfer to a Windows computer (Windows 10 or 11)
3. Install .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
4. Open PowerShell/Command Prompt and navigate to the project folder
5. Run: dotnet restore SqlStudioPro.sln
6. Run: dotnet build SqlStudioPro.sln
7. Run: cd src/UI.Wpf && dotnet run

ALTERNATIVE:
-----------
• Open SqlStudioPro.sln in Visual Studio 2022
• Press F5 to build and run

WHAT YOU GET:
------------
• Professional SQL Server Management Studio clone
• Natural Language to SQL query generation
• Multi-tab query editor with syntax highlighting  
• Schema browser with tables, views, stored procedures
• Entity Framework LINQ execution
• Light/Dark themes
• Secure connection management with DPAPI encryption

For complete instructions, see README.md

================================================================================
