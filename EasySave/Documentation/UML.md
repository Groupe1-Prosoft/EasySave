# Architecture EasySave 1.0 - Livrable 1

Here UML diagrams for the first deliverable

## 1. Use Case
à ajouter 
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