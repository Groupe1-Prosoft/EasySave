# Architecture EasySave 1.0 - Livrable 1

Here UML diagrams for the first deliverable

## 1. Use Case
```mermaid
---
config:
  layout: fixed
---
flowchart LR
 subgraph S["EasySave 1.0 (Console)"]
    direction TB
        UC_Create(["Create a Backup Job"])
        UC_List(["List Backup Jobs"])
        UC_Exec(["Execute Backup Job"])
        UC_Del(["Delete a Backup Job"])
        UC_Lang(["Change Language"])
        UC_Full(["Full Backup"])
        UC_Diff(["Differential Backup"])
        UC_Log(["Generate Logs JSON"])
        UC_State(["Update State JSON"])
  end
    U(("User")) --> UC_Create & UC_List & UC_Exec & UC_Del & UC_Lang
    UC_Full --> UC_Create
    UC_Diff --> UC_Create
    UC_Exec -. include .-> UC_Log & UC_State

     UC_Create:::caseStyle
     UC_List:::caseStyle
     UC_Exec:::caseStyle
     UC_Del:::caseStyle
     UC_Lang:::caseStyle
     UC_Full:::caseStyle
     UC_Diff:::caseStyle
     UC_Log:::caseStyle
     UC_State:::caseStyle
     U:::actorStyle
    classDef actorStyle fill:#fff,stroke:#000,stroke-width:2px
    classDef caseStyle fill:#fff,stroke:#000,stroke-width:1px,rx:20,ry:20
    classDef sysStyle fill:#f4f4f4,stroke:#000,stroke-width:2px


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