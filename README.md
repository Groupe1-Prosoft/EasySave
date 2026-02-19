# EasySave - ProSoft Backup Software

EasySave is a lightweight backup solution developed for the "Génie Logiciel" module (CESI - A3 FISA INFO).

## Team
- Yanis
- Rayene
- Fayçal
- Maxime

## Project overview
EasySave provides a console-based backup application (V1) with planned evolutions:
- V1.0: Console application — sequential backups, JSON logs.
- V2.0: GUI (MVVM), encryption, XML/JSON logs.
- V3.0: Parallel backups, task prioritization, centralized logs.

## Technical stack
- Language: C#
- Frameworks: .NET 8.0 (projects may target .NET 8 and .NET 10)
- IDE: Visual Studio 2022 (or newer)
- Version control: Git & GitHub

---

## Repository structure

````
EasySave/
├── Diagrams/
│   └── UML.md                       # Current Architecture Diagrams (V2)
├── EasySave/                        # Console Application (V1)
│   ├── Program.cs                   # Entry point (Controller - MVC)
│   ├── Localization/
│   │   └── LanguageManager.cs       # Singleton - Language management (EN/FR)
│   ├── Models/
│   │   ├── BackupJob.cs             # Backup job data model
│   │   ├── BackupState.cs           # Real-time backup state
│   │   └── BackupType.cs            # Enum: Full / Differential
│   ├── Services/
│   │   ├── BackupService.cs         # Facade - Backup execution logic
│   │   └── CommandLineService.cs    # CLI argument parser
│   └── Views/
│       └── ConsoleView.cs           # Console UI display
├── EasyLog/                         # DLL for logging
│   ├── ILogger.cs                   # Logger interface (ISP)
│   ├── Logger.cs                    # Logger implementation
│   └── LogData.cs                   # Log data model
│   └── Integration-guide.md         
├── EasySave.AvaloniaApp/            # GUI Application (V2 - MVVM)
│   ├── ViewModels/                  # MVVM ViewModels
│   └── Views/                       # MVVM Views
└── README.md
````

---

## EasyLog DLL

EasyLog is a standalone class library that handles all log writing for EasySave. It is versioned and distributed independently of the main application.

> **Full integration guide:** [EasyLog/Integration-guide.md](EasyLog/Integration-guide.md)


## Solution Architecture
The solution is divided into several projects to adhere to the Separation of Concerns principle:
1.  **EasySave (Console App):** Application entry point, menu management, and execution.
2.  **EasyLog (Class Library / DLL):** Independent log management (JSON/XML) and real-time state tracking.
3.  **EasySave.AvaloniaApp (GUI):** Graphical interface using MVVM pattern (for V2).


### Architecture (MVC Pattern)

The console application follows the **Model-View-Controller** pattern:

````
┌─────────────────────────────────────────────────────────────┐
│                        Program.cs                           │
│                       (CONTROLLER)                          │
│  - Handles user input                                       │
│  - Coordinates View and Services                            │
└─────────────────┬───────────────────────────┬───────────────┘
                  │                           │
                  ▼                           ▼
┌─────────────────────────────┐   ┌───────────────────────────┐
│      ConsoleView            │   │     BackupService         │
│         (VIEW)              │   │       (SERVICE)           │
│  - ShowMenu()               │   │  - ExecuteJob()           │
│  - GetInput()               │   │  - ExecuteSequential()    │
│  - DisplayError()           │   │  - CopyDirectory()        │
│  - ShowProgress()           │   │                           │
└─────────────────────────────┘   └───────────────────────────┘
                 │                           │
                 ▼                           ▼
┌─────────────────────────────┐   ┌───────────────────────────┐
│    LanguageManager          │   │     EasyLog DLL           │
│    (SINGLETON)              │   │     (Logger)              │
│  - GetText()                │   │  - WriteLog()             │
│  - SetLanguage()            │   │  - CreateDailyLogFile()   │
└─────────────────────────────┘   └───────────────────────────┘
````

---

## Design Patterns

| Pattern | Class | Description |
|---------|-------|-------------|
| **Singleton** | `LanguageManager` | Single instance for translations across the app |
| **Facade** | `BackupService` | Simplifies complex backup operations |
| **MVC** | `Program`, `ConsoleView`, `Models` | Separation of concerns |
| **Dependency Injection** | `BackupService(ILogger)` | Loose coupling with logger |

### Singleton Pattern (LanguageManager)

The `LanguageManager` uses the Singleton pattern to ensure a single instance manages translations across the entire application.

```csharp
// Private constructor prevents direct instantiation
private LanguageManager() { }

// Thread-safe lazy initialization
private static readonly Lazy<LanguageManager> _instance = new(() => new LanguageManager());

// Global access point
public static LanguageManager Instance => _instance.Value;

// Usage
var lang = LanguageManager.Instance;
lang.SetLanguage("fr");
string text = lang.GetText("Goodbye"); // "Au revoir !"
```
**Advantages:**

Guarantees a single instance across the application
Thread-safe instantiation via Lazy<T>
Thread-safe access via lock — SetLanguage(), GetText(), and CurrentLanguage are synchronized so no thread can observe a partially-updated state (e.g., language code changed but dictionary not yet swapped)
Global access without passing references everywhere
Prevents inconsistent language state

## Facade Pattern (BackupService)
The `BackupService` acts as a Facade, hiding the complexity of file operations, state management, and logging behind a simple interface.

```csharp
// Client code is simple - complexity hidden inside
_backupService.ExecuteJob(job);         // Single job
_backupService.ExecuteSequential(jobs); // Multiple jobs
```
**Advantages:**

-Simplifies client code (Program.cs only calls 2 methods)
-Hides complexity of file copying, state tracking, and logging
-Easy to modify internal implementation without affecting clients

## Dependency Injection (BackupService)

The `BackupService` receives its dependencies through constructor injection, following the Dependency Inversion Principle (DIP).

```csharp
// Interface defines the contract
public interface ILogger
{
    void WriteLog(LogData data);
}

// Injection via constructor
ILogger logger = new Logger();
var backupService = new BackupService(logger);
```
**Advantages:**

-Loose coupling between BackupService and Logger
-Easy to swap implementations (e.g., for unit testing)
-Follows SOLID principles (DIP)

___

## Development Workflow
To ensure code quality and avoid conflicts, we strictly follow these rules:

## Branching Strategy

```
main          ─────●─────────────────●───────────► (stable releases)
                   │                 ▲
                   │                 │ merge
develop       ─────●────●────●──────●────────────► (integration)
                        │    ▲      ▲
                        │    │      │ merge
feat/xxx   ─────────────●────●      │
feat/yyy   ─────────────────────────●
```
1. **Branch types:**
main: Stable and deliverable version (tags: v1.0, v2.0, etc.)
develop: Common development version
feat/*: Working branch for each task

2.  **Conventions:**
    * No personal names in branch names.
    * Code and comments must be in English.
    * No dead code or duplication (DRY principle).

    ___

## Installation & Quick Start

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download)

### Run the Application
1. Clone the repository: 
   ```bash
   git clone [https://github.com/Groupe1-Prosoft/EasySave.git](https://github.com/Groupe1-Prosoft/EasySave.git)

2. Navigate to the project directory:

cd EasySave

3. Run the console application:
   
   dotnet run --project EasySave
   
User Guide
For detailed instructions on how to navigate the menus, configure backups, and use the Command Line Interface (CLI) parameters, please refer to our dedicated documentation:

Read the EasySave User Guide

## Documentation and Deliverables

### UML Diagrams
Our documentation uses a "living document" approach. The UML diagrams file always reflects the architecture of the latest active deliverable (currently **V2.0**).

* [**View Current Project Architecture (UML)**](Diagrams/UML.md)

### Release (Download)
Access the compiled version for evaluation below:

- [**Download Deliverable v2.0 (Encryption & Monitoring)**](https://github.com/Groupe1-Prosoft/EasySave/releases/tag/v2.0)
- [Download Deliverable v1.1 (XML/JSON Logs)](https://github.com/Groupe1-Prosoft/EasySave/releases/tag/v1.1)
- [Download Deliverable v1.0 (Old version)](https://github.com/Groupe1-Prosoft/EasySave/releases/tag/v1.0)
