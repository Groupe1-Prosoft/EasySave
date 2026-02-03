# Architecture EasySave 1.0 - Livrable 1

Here UML diagrams for the first deliverable

## 1. Use Case
```mermaid
usecaseDiagram
    actor "User" as U
    package "EasySave v1.0" {
        usecase "Create a backup job" as UC1
        usecase "Execute a backup job" as UC2
        usecase "Execute sequentially" as UC3
        usecase "Change language" as UC4
        usecase "Manage Daily Log" as UC_Log
        usecase "Update State" as UC_State
    }
    U --> UC1
    U --> UC2
    U --> UC3
    U --> UC4
    UC2 ..> UC_Log : include
    UC2 ..> UC_State : include
    UC3 ..> UC2 : extend
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