# Architecture EasySave 1.0 - Livrable 1

Here UML diagrams for the first deliverable

## 1. Use Case
```mermaid
usecaseDiagram
    actor "User" as U

    package "EasySave 1.0 (Console)" {
        usecase "Create a Backup Job" as UC_Create
        usecase "List Backup Jobs" as UC_List
        usecase "Delete a Backup Job" as UC_Delete
        usecase "Execute Backup Job(s)" as UC_Exec
        usecase "Change Language (EN/FR)" as UC_Lang

        %% Job Types
        usecase "Full Backup" as UC_Full
        usecase "Differential Backup" as UC_Diff

        %% Execution Types
        usecase "Execute One Job" as UC_One
        usecase "Execute All Jobs" as UC_All

        %% System Actions
        usecase "Generate Daily Log (JSON)" as UC_Log
        usecase "Update Real-Time State (JSON)" as UC_State
    }

    %% User Interactions
    U --> UC_Create
    U --> UC_List
    U --> UC_Delete
    U --> UC_Exec
    U --> UC_Lang

    %% Generalizations
    UC_Full --|> UC_Create
    UC_Diff --|> UC_Create
    UC_One --|> UC_Exec
    UC_All --|> UC_Exec

    %% Includes
    UC_Exec ..> UC_Log : <<include>>
    UC_Exec ..> UC_State : <<include>>

    %% Constraints
    note right of UC_All
        Sequential Execution
        Max 5 Jobs
    end note
```

## 2. Class
```mermaid
classDiagram
    class Program {
        +Main(args: string[])
    }

    class ConsoleView {
        +ShowMenu()
        +GetInput()
        +ShowProgress()
    }

    class Configuration {
        +List~BackupJob~ Jobs
        +LoadConfig()
        +SaveConfig()
    }

    class BackupJob {
        +string Name
        +string SourceDir
        +string TargetDir
        +BackupType Type
    }

    class BackupService {
        +ExecuteJob(job: BackupJob)
        +ExecuteSequential(ids: List~int~)
        -CopyFile(source, dest)
    }

    class BackupState {
        +string JobName
        +DateTime Timestamp
        +string State
        +int TotalFiles
        +int Progression
        +UpdateStateJSON()
    }

    namespace EasyLog_DLL {
        class Logger {
            +WriteLog(LogData data)
        }
        class LogData {
            +string Name
            +string Source
            +string Target
            +long Size
            +long TransferTime
        }
    }

    Program --> ConsoleView
    Program --> Configuration
    Program --> BackupService
    Configuration *-- "0..5" BackupJob
    BackupService ..> BackupState : Updates
    BackupService ..> Logger : Calls (via DLL)
```

## 3. Sequence Diagram
```mermaid
sequenceDiagram
    participant User
    participant View as ConsoleView
    participant Service as BackupService
    participant State as BackupState
    participant DLL as EasyLog.Logger

    User->>View: Starts Job 1
    View->>Service: ExecuteJob(Job1)
    
    Service->>Service: Calculate file count & size
    Service->>State: InitState(TotalFiles, TotalSize)
    
    loop For each file
        Service->>State: UpdateState(CurrentFile, "Active")
        Service->>DLL: WriteLog(Source, Dest, Time)
        Service->>State: UpdateProgress(Percent)
    end

    Service->>State: UpdateState("Finished")
    View-->>User: Displays "Success"
```