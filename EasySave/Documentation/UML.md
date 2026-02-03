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
        +ParseArguments(args: string[]) List~int~
    }
    
    class ConsoleView {
        +ShowMenu()
        +GetInput() string
        +ShowProgress(BackupState state)
        +DisplayError(string message)
    }
    
    class LanguageManager {
        -string CurrentLanguage
        -Dictionary~string, string~ Translations
        +SetLanguage(string lang)
        +GetText(string key) string
        +LoadTranslations() bool
    }
    

    
    class Configuration {
        -List~BackupJob~ Jobs
        -string ConfigFilePath
        +GetJobs() List~BackupJob~
        +AddJob(BackupJob job) bool
        +RemoveJob(int id) bool
        +LoadConfig() bool
        +SaveConfig() bool
        +ToJSON() string
        +FromJSON(string json) Configuration
    }
    
    class BackupService {
        -Logger logger
        -BackupState currentState
        +ExecuteJob(job: BackupJob) bool
        +ExecuteSequential(ids: List~int~) bool
        -CopyFile(source: string, dest: string) long
        -GetFileList(directory: string) List~string~
        -CalculateTotalSize(files: List~string~) long
        -UpdateProgress(current: int, total: int)
    }
    
    class BackupState {
        +string JobName
        +DateTime Timestamp
        +string State
        +int TotalFiles
        +long TotalSize
        +int Progression
        +int FilesRemaining
        +long SizeRemaining
        +string CurrentSourceFile
        +string CurrentTargetFile
        +UpdateStateJSON() bool
        +ToJSON() string
    }
    

    
    class BackupJob {
        +int Id
        +string Name
        +string SourceDir
        +string TargetDir
        +BackupType Type
        +Validate() bool
    }
    
    class BackupType {
        <<enumeration>>
        Full
        Differential
    }
    

    
    class Logger {
        <<EasyLog_DLL>>
        -string LogFilePath
        +WriteLog(LogData data) bool
        +CreateDailyLogFile() string
    }
    
    class LogData {
        <<EasyLog_DLL>>
        +DateTime Timestamp
        +string Name
        +string Source
        +string Target
        +long Size
        +long TransferTime
        +ToJSON() string
        +Validate() bool
    }
    


    Program *-- ConsoleView
    Program *-- Configuration
    Program --> BackupService
    ConsoleView --> LanguageManager
    
    
    BackupService --> Configuration
    BackupService ..> BackupState : updates
    BackupService ..> Logger : calls
    Configuration *-- "1..5" BackupJob
    
    
    BackupJob --> BackupType
    BackupState ..> Configuration : persists
    Logger --> LogData : creates
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