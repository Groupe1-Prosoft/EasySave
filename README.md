# EasySave - ProSoft Backup Software

Welcome to the official repository of the **EasySave** project.
This software is developed as part of the "Génie Logiciel" module (CESI - A3 FISA INFO).

## Development Team
* **Yanis**
* **Rayene**
* **Fayçal**
* **Maxime**

## Project Description
EasySave is a backup solution developed in C# .NET Core for **ProSoft**.
The project follows a 3-phase development cycle:
* **V1.0:** Console Application, Sequential backups, JSON Logs.
* **V2.0:** Graphical User Interface (MVVM), Encryption (CryptoSoft), XML/JSON Logs.
* **V3.0:** Parallel backups, Task prioritization, Centralized logs.

## Technical Stack
* **Language:** C#
* **Framework:** .NET 8.0
* **IDE:** Visual Studio 2022 (or newer)
* **Version Control:** Git & GitHub

## Solution Architecture
The solution is divided into several projects to adhere to the Separation of Concerns principle:
1.  **EasySave (Console App):** Application entry point, menu management, and execution.
2.  **EasyLog (Class Library / DLL):** Independent log management (JSON/XML) and real-time state tracking.

## Development Workflow
To ensure code quality and avoid conflicts, we strictly follow these rules:

1.  **Branching Strategy:**
    * `main`: Stable and deliverable version (never commit directly here).
    * `develop`: Common development version.
    * `feature/feature-name`: Working branch for each task (e.g., `feature/json-logs`, `feature/interface`).

2.  **Conventions:**
    * No personal names in branch names.
    * Code and comments must be in English.
    * No dead code or duplication (DRY principle).

## Installation & Usage
1.  Clone the repository: `git clone https://github.com/yyyanis/EasySave.git`
2.  Open the `.sln` file in Visual Studio.
3.  Ensure the startup project is set to **EasySave**.
4.  Build and Start (F5).
5. If you're running on vs code or anything else type this command to run it on the terminal : dotnet run --project EasySave
6. To run the job in the command line way paste this commad on command line : 

dotnet build

Execute first job :
dotnet run -- 1

Execute jobs 1 to 3 :
dotnet run -- 1-3

 Execute jobs 1 and 3 :
 dotnet run -- "1;3"
 
 ## Logs
 For the logs and the state.json, it'll be generate on your computer especially on this path : C/User/username/AppData/Roaming/EasySave
