#  Diagrams EasySave 1.0 - Livrable 1

### Before writing code, the most critical is to have a clear and comprehensive understanding of the application's architecture. Rushing into implementation without proper planning often leads to a mid structured code, technical debt, and costly refactoring later in the project lifecycle.

## 1. Use Case

#### A Use Case Diagram is a behavioral UML diagram that captures the functional requirements of a system by showing the interactions between users (actors) and the system's features (use cases). This diagram provides a high-level overview of what the EasySave application can do from the user's perspective. It shows that a user can create, list, execute, and delete backup jobs, as well as change the application language. It also illustrates that backup jobs can be either Full or Differential, and that executing a backup includes generating logs and updating the state file.

```mermaid
---
config:
  layout: dagre
---
flowchart LR
 subgraph S["EasySave 1.0 (Console)"]
    direction TB
        UC_Create(["Create Backup Job"])
        UC_List(["List Backup Jobs"])
        UC_Exec(["Execute Backup"])
        UC_ExecSeq(["Execute Sequential Backups"])
        UC_Del(["Delete Backup Job"])
        UC_Lang(["Change Language"])
        UC_Full(["Perform Full Backup"])
        UC_Diff(["Perform Differential Backup"])
        UC_Log(["Write Daily Log"])
        UC_State(["Update State File"])
  end
    U(("User")) --> UC_Create & UC_List & UC_Exec & UC_ExecSeq & UC_Del & UC_Lang
    UC_Full -. "«extend»<br/>[type = Full]" .-> UC_Exec
    UC_Diff -. "«extend»<br/>[type = Differential]" .-> UC_Exec
    UC_Exec -. "«include»" .-> UC_Log
    UC_Exec -. "«include»" .-> UC_State
    UC_ExecSeq -. "«include»" .-> UC_Exec

     UC_Create:::caseStyle
     UC_List:::caseStyle
     UC_Exec:::caseStyle
     UC_ExecSeq:::caseStyle
     UC_Del:::caseStyle
     UC_Lang:::caseStyle
     UC_Full:::caseStyle
     UC_Diff:::caseStyle
     UC_Log:::caseStyle
     UC_State:::caseStyle
     U:::actorStyle
    classDef actorStyle fill:#fff,stroke:#000,stroke-width:2px
    classDef caseStyle fill:#fff,stroke:#000,stroke-width:1px,rx:20,ry:20

```

    
## 2. Class

#### A Class Diagram is a structural UML diagram that represents the static structure of a system by showing its classes, attributes, methods, and the relationships between them. This diagram defines the architecture of the EasySave application. It shows how different components interact: the Program entry point connects to ConsoleView and Configuration, the BackupService handles the backup logic while updating BackupState and using the Logger (from an external DLL), and Configuration manages up to 5 BackupJob instances. The relationships (composition, association, dependency) clarify how objects are created and used throughout the system.
```mermaid
classDiagram

    class Program {
        +Main(args: string[])$
    }

    class CommandLineService {
        +ParseArgument(argument: string) List~int~
    }

    class ConsoleView {
        -languageManager: LanguageManager
        +ShowMenu()
        +SelectLogFormat() 
        +GetInput() string
        +ShowProgress(state: BackupState)
        +DisplayError(message: string)
    }

    class LanguageManager {
        -currentLanguage: string
        -translations: Dictionary~string, string~
        +SetLanguage(lang: string)
        +GetText(key: string) string
        +LoadTranslations() bool
    }

    class Configuration {
        -jobs: List~BackupJob~
        -configFilePath: string «const»
        -logFormat: string
        +GetJobs() List~BackupJob~
        +AddJob(job: BackupJob) bool
        +RemoveJob(id: int) bool
        +LoadConfig() bool
        +SaveConfig() bool
    }

    class BackupService {
        -logger: Logger
        -configuration: Configuration
        +ExecuteJob(job: BackupJob) bool
        +ExecuteSequential(ids: List~int~) bool
        -CopyFile(source: string, dest: string) long
        -GetFileList(directory: string) List~string~
        -CalculateTotalSize(files: List~string~) long
        -UpdateProgress(current: int, total: int)
    }

    class BackupState {
        +JobName: string «property»
        +Timestamp: DateTime «property»
        +State: string «property»
        +TotalFiles: int «property»
        +TotalSize: long «property»
        +Progression: int «property»
        +FilesRemaining: int «property»
        +SizeRemaining: long «property»
        +CurrentSourceFile: string «property»
        +CurrentTargetFile: string «property»
        +UpdateStateJSON() bool
        +ToJSON() string
    }

    class BackupJob {
        +Id: int «property»
        +Name: string «property»
        +SourceDir: string «property»
        +TargetDir: string «property»
        +Type: BackupType «property»
        +Validate() bool
    }

    class BackupType {
        <<enumeration>>
        Full
        Differential
    }

    class ILogger {
        <<interface>>
        <<EasyLog_DLL>>
        +WriteLog(data: LogData) bool
    }

    class Logger {
        <<EasyLog_DLL>>
        -logFilePath: string
        -logFormat: string
        +WriteLog(data: LogData) bool
        +CreateDailyLogFile() string
        
    }

    class LogData {
        <<EasyLog_DLL>>
        +Timestamp: DateTime «property»
        +Name: string «property»
        +Source: string «property»
        +Target: string «property»
        +Size: long «property»
        +TransferTime: long «property»
        +ToJSON() string
        +ToXML() string 
    }

    Program *-- ConsoleView : creates
    Program *-- BackupService : creates
    Program ..> CommandLineService : uses
    ConsoleView --> LanguageManager : uses

    BackupService --> Configuration : reads jobs
    BackupService ..> BackupState : creates & updates
    BackupService ..> LogData : creates
    BackupService --> ILogger : calls

    Configuration *-- "1..5" BackupJob : contains
    BackupJob --> BackupType : has type

    ILogger <|.. Logger : implements
    Logger ..> LogData : writes 
```




## 3. Sequence Diagram Creation backup

#### Here are some sequence diagram examples (creation, execution, deletion, etc.). More sequence diagrams can be added; these are just examples.


#### A Sequence Diagram is a behavioral UML diagram that shows how objects interact in a particular scenario over time, displaying the sequence of messages exchanged between participants.This diagram illustrates the step-by-step process of creating a new backup job. It shows the user providing job details through the console, the system checking if the maximum limit of 5 jobs has been reached, validating the new job, and persisting it to the configuration file. The alternative flow handles the case where the user has already reached the maximum number of jobs.


```mermaid
sequenceDiagram
    actor User
    participant ConsoleView
    participant BackupService
    participant Configuration
    participant BackupJob

    User->>ConsoleView: Select "Create backup job"
    ConsoleView->>User: Request job details
    User->>ConsoleView: Provide details

    ConsoleView->>BackupService: CreateJob()
    BackupService->>Configuration: GetJobs()
    Configuration-->>BackupService: List of current jobs

    alt Less than 5 jobs
        BackupService->>BackupJob: new BackupJob()
        BackupJob->>BackupJob: Validate()
        BackupJob-->>BackupService: Valid job

        BackupService->>Configuration: AddJob()
        Configuration->>Configuration: SaveConfig()
        Configuration-->>BackupService: Success

        BackupService-->>ConsoleView: Job created
        ConsoleView-->>User: "Job created successfully"
    else 5 jobs already exist
        BackupService-->>ConsoleView: Error: maximum reached
        ConsoleView-->>User: "Maximum 5 jobs reached"
    end
```

## 4. Sequence Diagram Execution backup

#### This diagram details the execution flow of a backup job. It demonstrates how the system transitions through states (ACTIVE → COMPLETED), processes each file in a loop, logs transfer information for every copied file, and continuously updates the progress displayed to the user. 
```mermaid
sequenceDiagram
    actor User
    participant ConsoleView
    participant BackupService
    participant BackupState
    participant LogData
    participant ILogger

    User->>ConsoleView: Execute backup job
    ConsoleView->>BackupService: ExecuteJob()

    BackupService->>BackupState: new BackupState()
    BackupService->>BackupState: UpdateStateJSON()

    loop For each file in source
        BackupService->>BackupService: CopyFile()

        BackupService->>LogData: new LogData()
        BackupService->>ILogger: WriteLog()

        BackupService->>BackupState: UpdateProgress()
        BackupState->>BackupState: UpdateStateJSON()
    end

    BackupService->>BackupState: UpdateStateJSON()
    BackupService-->>ConsoleView: Backup completed
    ConsoleView-->>User: "Backup successful"
```

## 5. Sequence Diagram Supression backup

#### This diagram shows the important process of deleting a backup job. It emphasizes the confirmation step before deletion, ensuring the user consciously agrees to remove the job. This prevents accidental data loss.
```mermaid
   sequenceDiagram
    actor User
    participant ConsoleView
    participant BackupService
    participant Configuration

    User->>ConsoleView: Delete job
    ConsoleView->>User: Confirm deletion?
    User->>ConsoleView: Yes

    ConsoleView->>BackupService: DeleteJob()
    BackupService->>Configuration: RemoveJob()
    Configuration->>Configuration: SaveConfig()
    Configuration-->>BackupService: Job removed
    BackupService-->>ConsoleView: Success

    ConsoleView-->>User: "Job deleted successfully"  
```

## 6. Sequence Diagram switch languish

#### This diagram shows how the application handles language changes at runtime. When the user selects a new language, the LanguageManager loads the appropriate translations, the configuration is saved to persist the preference, and the interface refreshes to display text in the selected language. This supports the requirement (French/English) of the application.
```mermaid
sequenceDiagram
    actor User
    participant ConsoleView
    participant LanguageManager

    User->>ConsoleView: Change language
    ConsoleView->>User: Select language (FR/EN)
    User->>ConsoleView: Select language

    ConsoleView->>LanguageManager: SetLanguage()
    LanguageManager->>LanguageManager: LoadTranslations()
    LanguageManager-->>ConsoleView: Language changed

    ConsoleView->>ConsoleView: ShowMenu()
    ConsoleView-->>User: Menu refreshed
```

## 7. Sequence Diagram differential backup

#### This diagram highlights the key difference between full and differential backups. Unlike a full backup that copies all files, a differential backup checks each file's modification date and only copies files that have changed since the last backup.
```mermaid
sequenceDiagram
    actor User
    participant ConsoleView
    participant BackupService
    participant BackupJob
    participant ILogger

    User->>ConsoleView: Execute differential backup
    ConsoleView->>BackupService: ExecuteJob()

    BackupService->>BackupJob: GetType()
    BackupJob-->>BackupService: DIFFERENTIAL

    loop For each file in source
        BackupService->>BackupService: IsFileModified()

        alt File has been modified
            BackupService->>BackupService: CopyFile()
            BackupService->>ILogger: WriteLog()
        else File unchanged
            BackupService->>BackupService: Skip file
        end
    end

    BackupService-->>ConsoleView: Backup completed
    ConsoleView-->>User: "Differential backup done"
```

## 8. Activity diagram

#### An Activity Diagram is a behavioral UML diagram that models the workflow or business process of a system, showing the sequence of activities and decision points from start to finish. This comprehensive diagram provides a complete view of the backup execution workflow. It maps every step from job validation to completion, including decision points for backup type (full vs. differential), file comparison logic, error handling, logging operations, and progress tracking.
```mermaid
flowchart TD
    Start([Start]) --> LoadJob[Load selected Backup Job]

    LoadJob --> ValidateJob{Job valid?}

    ValidateJob -->|No| DisplayError[Display error message]
    DisplayError --> EndFail([End])

    ValidateJob -->|Yes| SetActive[Set state to ACTIVE]

    SetActive --> ScanSource[Scan source directory]

    ScanSource --> CheckType{Backup type?}

    CheckType -->|Full| ProcessAll[Process all files]
    CheckType -->|Differential| FilterModified[Filter modified files only]

    ProcessAll --> CopyLoop
    FilterModified --> CopyLoop

    CopyLoop[Copy files one by one] --> CopyResult{Copy successful?}

    CopyResult -->|No| HandleError[Log error and notify user]
    HandleError --> MoreFiles

    CopyResult -->|Yes| LogFile[Log transfer details]
    LogFile --> UpdateProgress[Update state and progress]
    UpdateProgress --> MoreFiles{More files?}

    MoreFiles -->|Yes| CopyLoop
    MoreFiles -->|No| SetCompleted[Set state to COMPLETED]

    SetCompleted --> DisplaySuccess[Display success message]
    DisplaySuccess --> EndSuccess([End])

    style Start fill:#2196F3,color:#fff
    style EndFail fill:#f44336,color:#fff
    style EndSuccess fill:#4CAF50,color:#fff
    style ValidateJob fill:#FFE4B5
    style CheckType fill:#FFE4B5
    style CopyResult fill:#FFE4B5
    style MoreFiles fill:#FFE4B5
```
