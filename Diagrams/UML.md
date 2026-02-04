#  EasySave 1.0 - Livrable 1

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

## Here are some sequence diagram examples (creation, execution, deletion, etc.). More sequence diagrams can be added; these are just examples.


## 3. Sequence Diagram Creation backup
```mermaid
sequenceDiagram
    actor User
    participant ConsoleView
    participant Configuration
    participant BackupJob
    
    User->>ConsoleView: Create new backup job
    ConsoleView->>User: Request job details
    User->>ConsoleView: Enter name, source, target, type
    
    ConsoleView->>Configuration: GetJobList()
    Configuration-->>ConsoleView: Current jobs (count)
    
    alt Less than 5 jobs
        ConsoleView->>BackupJob: new BackupJob(details)
        BackupJob->>BackupJob: Validate()
        BackupJob-->>ConsoleView: Job created
        
        ConsoleView->>Configuration: AddJob(job)
        Configuration->>Configuration: SaveConfig()
        Configuration-->>ConsoleView: Success
        
        ConsoleView-->>User: "Job created successfully"
    else 5 jobs exist
        ConsoleView-->>User: "Maximum 5 jobs reached"
    end
    
```

## 4. Sequence Diagram Execution backup
```mermaid
sequenceDiagram
    actor User
    participant ConsoleView
    participant BackupService
    participant BackupState
    participant Logger
    participant LogData
    
    User->>ConsoleView: Execute backup job #2
    ConsoleView->>BackupService: ExecuteBackupJob(job)
    
    BackupService->>BackupState: UpdateState(ACTIVE)
    BackupState-->>ConsoleView: State updated
    
    loop For each file
        BackupService->>BackupService: CopyFile(source, dest)
        
        BackupService->>LogData: new LogData(file info)
        BackupService->>Logger: WriteLog(logData)
        
        BackupService->>BackupState: UpdateProgress()
        BackupState-->>ConsoleView: Progress updated
        ConsoleView->>User: Show progress
    end
    
    BackupService->>BackupState: UpdateState(COMPLETED)
    BackupService-->>ConsoleView: Backup completed
    ConsoleView-->>User: "Backup successful"
```

## 5. Sequence Diagram Supression backup
```mermaid
sequenceDiagram
    actor User
    participant ConsoleView
    participant Configuration
    
    User->>ConsoleView: Delete job #3
    ConsoleView->>User: Confirm deletion?
    User->>ConsoleView: Yes
    
    ConsoleView->>Configuration: RemoveJob(3)
    Configuration->>Configuration: SaveConfig()
    Configuration-->>ConsoleView: Job removed
    
    ConsoleView-->>User: "Job deleted successfully"
```

## 6. Sequence Diagram switch languish
```mermaid
sequenceDiagram
    actor User
    participant ConsoleView
    participant LanguageManager
    participant Configuration
    
    User->>ConsoleView: Change language
    ConsoleView->>User: Select language (FR/EN)
    User->>ConsoleView: English
    
    ConsoleView->>LanguageManager: SetLanguage("English")
    LanguageManager->>LanguageManager: LoadTranslations()
    LanguageManager-->>ConsoleView: Language changed
    
    ConsoleView->>Configuration: SaveConfig()
    ConsoleView-->>User: Menu refreshed in English
```

## 7. Sequence Diagram differential backup
```mermaid
sequenceDiagram
    actor User
    participant ConsoleView
    participant BackupService
    participant BackupJob
    participant Logger
    
    User->>ConsoleView: Execute differential backup
    ConsoleView->>BackupService: ExecuteBackupJob(job)
    
    BackupService->>BackupJob: Type?
    BackupJob-->>BackupService: DIFFERENTIAL
    
    loop For each file
        BackupService->>BackupService: File modified?
        
        alt File is newer
            BackupService->>BackupService: CopyFile()
            BackupService->>Logger: WriteLog()
        else File unchanged
            BackupService->>BackupService: Skip file
        end
    end
    
    BackupService-->>ConsoleView: Completed
    ConsoleView-->>User: "Differential backup done"
```

## 8. Activity diagram
```mermaid
flowchart TD
    Start([Backup Start]) --> LoadJob[Load selected BackupJob]
    
    LoadJob --> ValidateJob{Validate Job<br/>BackupJob.Validate}
    
    ValidateJob -->|Invalid| DisplayError[Display error via ConsoleView]
    DisplayError --> End([End - Failed])
    
    ValidateJob -->|Valid| InitState[Initialize BackupState<br/>State = INACTIVE]
    
    InitState --> SetActive[BackupState.UpdateState<br/>State = ACTIVE]
    
    SetActive --> UpdateStateFile1[Update state.json<br/>via BackupState.UpdateStateJSON]
    
    UpdateStateFile1 --> ScanSource[Scan source directory<br/>BackupService.GetFileList]
    
    ScanSource --> CalcTotal[Calculate TotalFiles and TotalSize<br/>BackupService.CalculateTotalSize]
    
    CalcTotal --> UpdateState2[Update BackupState<br/>TotalFiles, TotalSize]
    
    UpdateState2 --> CheckType{Check Backup<br/>Type}
    
    CheckType -->|FULL| ProcessFull[Mode: Full Backup<br/>All files]
    CheckType -->|DIFFERENTIAL| ProcessDiff[Mode: Differential Backup<br/>Modified files only]
    
    ProcessFull --> LoopStart[Loop start<br/>For each file]
    ProcessDiff --> LoopStart
    
    LoopStart --> GetNextFile[Get next file]
    
    GetNextFile --> CheckDiffNeeded{Type = DIFFERENTIAL?}
    
    CheckDiffNeeded -->|No FULL| CopyFile
    CheckDiffNeeded -->|Yes| CompareDate{Source file<br/>more recent?}
    
    CompareDate -->|No| SkipFile[Skip file<br/>Decrement FilesRemaining]
    CompareDate -->|Yes| CopyFile
    
    CopyFile[BackupService.CopyFile<br/>Copy file]
    
    CopyFile --> UpdateProgress1[Update BackupState<br/>CurrentSourceFile<br/>CurrentTargetFile]
    
    UpdateProgress1 --> UpdateStateFile2[Write to state.json]
    
    UpdateStateFile2 --> StartTimer[Start timer]
    
    StartTimer --> PerformCopy{Copy successful?}
    
    PerformCopy -->|Error| StopTimerError[Stop timer]
    StopTimerError --> LogError[Logger.WriteLog<br/>TransferTime = -1]
    LogError --> UpdateStateError[BackupState.UpdateState<br/>State = ERROR]
    UpdateStateError --> DisplayErrorMsg[Display error<br/>ConsoleView.DisplayError]
    DisplayErrorMsg --> DecideAction{Continue?}
    
    DecideAction -->|Yes| MoreFiles
    DecideAction -->|No| EndError([End - Error])
    
    PerformCopy -->|Success| StopTimer[Stop timer<br/>Calculate transfer time]
    
    StopTimer --> CreateLogData[Create LogData<br/>timestamp, name, source,<br/>target, size, transferTime]
    
    CreateLogData --> WriteLog[Logger.WriteLog<br/>Write to YYYY-MM-DD.json]
    
    WriteLog --> CheckLogFile{Daily log file<br/>exists?}
    
    CheckLogFile -->|No| CreateLogFile[Logger.CreateDailyLogFile<br/>Create new file]
    CreateLogFile --> AppendLog
    CheckLogFile -->|Yes| AppendLog[Append to log file]
    
    AppendLog --> UpdateProgress2[BackupService.UpdateProgress<br/>Progression++<br/>FilesRemaining--<br/>SizeRemaining -= fileSize]
    
    UpdateProgress2 --> CalcProgression[BackupState.CalculateProgression<br/>Progression = copied/total * 100]
    
    CalcProgression --> UpdateStateFile3[Update state.json]
    
    UpdateStateFile3 --> ShowProgress[ConsoleView.ShowProgress<br/>Display progress bar]
    
    SkipFile --> ShowProgress
    
    ShowProgress --> MoreFiles{More files<br/>to process?}
    
    MoreFiles -->|Yes| GetNextFile
    MoreFiles -->|No| SetCompleted[BackupState.UpdateState<br/>State = COMPLETED<br/>Progression = 100]
    
    SetCompleted --> UpdateStateFinal[Update state.json]
    
    UpdateStateFinal --> DisplaySuccess[ConsoleView.DisplaySuccess<br/>Display success message]
    
    DisplaySuccess --> EndSuccess([End - Success])
    
    style Start fill:#0000FF
    style End fill:#FF0000
    style EndError fill:#FF0000
    style EndSuccess fill:#0000FF
    style ValidateJob fill:#FFE4B5
    style CheckType fill:#FFE4B5
    style CheckDiffNeeded fill:#FFE4B5
    style CompareDate fill:#FFE4B5
    style PerformCopy fill:#FFE4B5
    style CheckLogFile fill:#FFE4B5
    style MoreFiles fill:#FFE4B5
    style DecideAction fill:#FFE4B5
```
