
#  Diagrams EasySave 2.0 - Livrable 2

### Before writing code, the most critical is to have a clear and comprehensive understanding of the application's architecture. Rushing into implementation without proper planning often leads to a mid structured code, technical debt, and costly refactoring later in the project lifecycle.

## 1. Use Case

#### A Use Case Diagram is a behavioral UML diagram that captures the functional requirements of a system by showing the interactions between users (actors) and the system's features (use cases). This diagram provides a high-level overview of what the EasySave application can do from the user's perspective. It shows that a user can create, list, execute, and delete backup jobs, as well as change the application language. It also illustrates that backup jobs can be either Full or Differential, and that executing a backup includes generating logs and updating the state file.

```mermaid
---
config:
  layout: dagre
---
flowchart LR
 subgraph S["EasySave 3.0 (Avalonia GUI)"]
    direction TB
        UC_Create(["Create Backup Job"])
        UC_List(["List Backup Jobs"])
        UC_Exec(["Execute Backup"])
        UC_ExecPar(["Execute Parallel Backups"])
        UC_Del(["Delete Backup Job"])
        UC_Lang(["Change Language"])
        UC_Settings(["Manage Settings"])
        UC_Full(["Perform Full Backup"])
        UC_Diff(["Perform Differential Backup"])
        UC_Encrypt(["Encrypt File"])
        UC_LogFormat(["Choose Log Format"])
        UC_SetExtensions(["Set Encrypted Extensions"])
        UC_SetBusiness(["Set Business Software"])
        UC_ChooseType(["Choose Backup Type"])
        UC_Pause(["Pause Job(s)"])
        UC_Resume(["Resume Job(s)"])
        UC_Stop(["Stop Job(s)"])
        UC_SetPriority(["Set Priority Extensions"])
        UC_SetMaxSize(["Set Max Large File Size"])
        UC_SetLogMode(["Set Log Mode (Docker)"])
  end

    U(("User")) --> UC_Create & UC_List & UC_Exec & UC_ExecPar & UC_Del & UC_Lang & UC_Settings

    UC_Exec -. "«include»" .-> UC_ChooseType
    UC_Full -. "«extend»<br/>[type = Full]" .-> UC_ChooseType
    UC_Diff -. "«extend»<br/>[type = Differential]" .-> UC_ChooseType
    UC_Encrypt -. "«extend»<br/>[file eligible]" .-> UC_Exec
    UC_ExecPar -. "«include»" .-> UC_Exec
    UC_Pause -. "«extend»<br/>[job running]" .-> UC_Exec
    UC_Resume -. "«extend»<br/>[job paused]" .-> UC_Exec
    UC_Stop -. "«extend»<br/>[job running or paused]" .-> UC_Exec
    UC_Settings -. "«include»" .-> UC_LogFormat
    UC_Settings -. "«include»" .-> UC_SetExtensions
    UC_Settings -. "«include»" .-> UC_SetBusiness
    UC_Settings -. "«include»" .-> UC_SetPriority
    UC_Settings -. "«include»" .-> UC_SetMaxSize
    UC_Settings -. "«include»" .-> UC_SetLogMode

     UC_Create:::caseStyle
     UC_List:::caseStyle
     UC_Exec:::caseStyle
     UC_ExecPar:::caseStyle
     UC_Del:::caseStyle
     UC_Lang:::caseStyle
     UC_Settings:::caseStyle
     UC_Full:::caseStyle
     UC_Diff:::caseStyle
     UC_Encrypt:::caseStyle
     UC_LogFormat:::caseStyle
     UC_SetExtensions:::caseStyle
     UC_SetBusiness:::caseStyle
     UC_ChooseType:::caseStyle
     UC_Pause:::caseStyle
     UC_Resume:::caseStyle
     UC_Stop:::caseStyle
     UC_SetPriority:::caseStyle
     UC_SetMaxSize:::caseStyle
     UC_SetLogMode:::caseStyle

     U:::actorStyle
    classDef actorStyle fill:#fff,stroke:#000,stroke-width:2px
    classDef caseStyle fill:#fff,stroke:#000,stroke-width:1px,rx:20,ry:20
```


    
## 2. Class

#### A Class Diagram is a structural UML diagram that represents the static structure of a system by showing its classes, attributes, methods, and the relationships between them. This diagram defines the architecture of the EasySave application. It shows how different components interact: the Program entry point connects to ConsoleView and Configuration, the BackupService handles the backup logic while updating BackupState and using the Logger (from an external DLL), and Configuration manages up to 5 BackupJob instances. The relationships (composition, association, dependency) clarify how objects are created and used throughout the system.


```mermaid
classDiagram

    namespace GUI_Avalonia {
        class Program:::guiStyle {
            +Main(args: string[])$
        }
        class MainWindow:::guiStyle {
            <<Avalonia View>>
            -dataContext: MainWindowViewModel
        }
        class ViewModelBase:::guiStyle {
            <<abstract>>
        }
        class MainWindowViewModel:::guiStyle {
            -configuration: Configuration
            -backupService: IBackupService
            -homeViewModel: HomeViewModel
            -settingsViewModel: SettingsViewModel
            -currentPage: ViewModelBase
            -isPaneOpen: bool
            +TogglePaneCommand()
            +NavigateCommand(page: string)
            +SwitchLanguageCommand(language: string)
        }
        class HomeViewModel:::guiStyle {
            -configuration: Configuration
            -backupService: IBackupService
            +Jobs: ObservableCollection~SelectableJob~ «property»
            -newName: string
            -newSourceDir: string
            -newTargetDir: string
            -selectedTypeIndex: int
            -statusMessage: string
            -isExecuting: bool
            +CreateJobCommand()
            +ExecuteSelectedCommand()
            +DeleteSelectedCommand()
            +ExecuteJobCommand(job: BackupJob)
            +DeleteJobCommand(job: BackupJob)
            +SelectAllCommand()
            +PauseJobCommand(id: int)
            +ResumeJobCommand(id: int)
            +StopJobCommand(id: int)
            +PauseAllCommand()
            +ResumeAllCommand()
            +StopAllCommand()
        }
        class SettingsViewModel:::guiStyle {
            -configuration: Configuration
            -cryptoSoftPath: string
            -extensionsText: string
            -businessSoftwareName: string
            -selectedLogFormatIndex: int
            -statusMessage: string
            -priorityExtensionsText: string
            -maxLargeFileSizeKB: long
            -selectedLogModeIndex: int
            -dockerServerUrl: string
            +SaveCommand()
        }
        class LogsViewModel:::guiStyle {
            +Logs: ObservableCollection~LogEntry~ «property»
            -statusMessage: string
            +LoadLogsCommand()
        }
        class LocalizationHelper:::guiStyle {
            <<singleton>>
            +Instance: LocalizationHelper «static»
            +CurrentLanguage: string «property»
            +SwitchLanguage(lang: string)
        }
        class LanguageManager:::guiStyle {
            -currentLanguage: string
            -translations: Dictionary~string, string~
            +SetLanguage(lang: string)
            +GetText(key: string) string
            +LoadTranslations() bool
        }
    }

    namespace ConsoleApp {
        class ServiceContainer:::consoleStyle {
            -_languageManager: LanguageManager
            -_configuration: Configuration
            -_view: ConsoleView
            -_logger: ILogger
            -_backupService: BackupService
            -_businessSoftwareMonitor: BusinessSoftwareMonitor
            -_cryptoSoftService: CryptoSoftService
            +GetConfiguration() Configuration
            +GetLanguageManager() LanguageManager
            +GetConsoleView() ConsoleView
            +GetBackupService() BackupService
            +GetLogger() ILogger
            +RefreshBackupService()
        }
        class MenuController:::consoleStyle {
            -_services: ServiceContainer
            -_backupService: BackupService
            -_configuration: Configuration
            -_view: ConsoleView
            -_languageManager: LanguageManager
            +Run()
            -SetupLanguageAndFormat()
            -HandleMenuOption(choice: string) bool
            -HandleListJobs() bool
            -HandleCreateJob() bool
            -HandleExecuteJob() bool
            -HandleExecuteAllJobs() bool
            -HandleDeleteJob() bool
            -HandleChangeLogFormat() bool
            -HandleInvalidOption() bool
        }
        class ConsoleView:::consoleStyle {
            -_languageManager: LanguageManager
            +ShowMenu()
            +SelectLogFormat() string
            +GetInput() string
            +ShowProgress(state: BackupState)
            +DisplayError(message: string)
        }
    }

    namespace Core {
        class IBackupService:::coreStyle {
            <<interface>>
            +ExecuteJob(job: BackupJob) bool
            +ExecuteParallel(ids: List~int~) bool
            +PauseJob(id: int)
            +ResumeJob(id: int)
            +StopJob(id: int)
            +PauseAll()
            +ResumeAll()
            +StopAll()
        }
        class BackupService:::coreStyle {
            -_logger: ILogger
            -_configuration: Configuration
            -_cryptoSoftService: CryptoSoftService
            -_businessMonitor: BusinessSoftwareMonitor
            -_controllers: Dictionary~int, BackupJobController~
            -_largeFileSemaphore: SemaphoreSlim
            +ExecuteJob(job: BackupJob) bool
            +ExecuteParallel(ids: List~int~) bool
            +PauseJob(id: int)
            +ResumeJob(id: int)
            +StopJob(id: int)
            +PauseAll()
            +ResumeAll()
            +StopAll()
            -CopyFile(source: string, dest: string) long
            -GetFileList(directory: string) List~string~
            -CalculateTotalSize(files: List~string~) long
            -CheckPriorityRule(file: string) bool
        }
        class BackupJobController:::coreStyle {
            +JobId: int «property»
            +State: JobControlState «property»
            -_pauseEvent: ManualResetEventSlim
            -_cancellationSource: CancellationTokenSource
            +Pause()
            +Resume()
            +Stop()
            +WaitIfPaused()
        }
        class JobControlState:::coreStyle {
            <<enumeration>>
            Running
            Paused
            Stopped
            Completed
        }
        class CryptoSoftService:::coreStyle {
            -cryptoSoftPath: string
            -_mutex: Mutex
            +EncryptFile(filePath: string) long
            +IsEligible(filePath: string, extensions: List~string~) bool
            +AcquireLock() bool
            +ReleaseLock()
        }
        class BusinessSoftwareMonitor:::coreStyle {
            -processName: string
            -_isMonitoring: bool
            +IsRunning() bool
            +SetProcessName(name: string)
            +StartMonitoring()
            +StopMonitoring()
        }
        class BackupState:::coreStyle {
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
            +UpdateStateXML() bool
            +ToJSON() string
            +ToXML() string
        }
        class BackupJob:::coreStyle {
            +Id: int «property»
            +Name: string «property»
            +SourceDir: string «property»
            +TargetDir: string «property»
            +Type: BackupType «property»
            +Validate() bool
        }
        class BackupType:::coreStyle {
            <<enumeration>>
            Full
            Differential
        }
        class LogMode:::coreStyle {
            <<enumeration>>
            Local
            Remote
            Both
        }
        class Configuration:::coreStyle {
            -jobs: List~BackupJob~
            -configFilePath: string «const»
            -logFormat: string
            -encryptExtensions: List~string~
            -businessSoftwareName: string
            -_lock: object
            +CryptoSoftPath: string «property»
            +PriorityExtensions: List~string~ «property»
            +MaxLargeFileSizeKB: long «property»
            +LogMode: LogMode «property»
            +DockerServerUrl: string «property»
            +GetJobs() List~BackupJob~
            +AddJob(job: BackupJob) bool
            +RemoveJob(id: int) bool
            +LoadConfig() bool
            +SaveConfig() bool
            +GetEncryptExtensions() List~string~
            +SetEncryptExtensions(ext: List~string~)
            +GetBusinessSoftwareName() string
            +SetBusinessSoftwareName(name: string)
            +GetLogFormat() string
            +SetLogFormat(format: string)
        }
    }

    namespace EasyLog_DLL {
        class ILogger:::logStyle {
            <<interface>>
            +WriteLog(data: LogData) bool
        }
        class Logger:::logStyle {
            -logFilePath: string
            -logFormat: string
            -_lock: object
            +WriteLog(data: LogData) bool
            +CreateDailyLogFile() string
        }
        class LogDispatcher:::logStyle {
            -_localLogger: Logger
            -_remoteLogger: RemoteLogService
            -_mode: LogMode
            +WriteLog(data: LogData) bool
        }
        class RemoteLogService:::logStyle {
            -_serverUrl: string
            -_machineId: string
            +SendLog(data: LogData) bool
            +Connect(url: string) bool
            +Disconnect()
        }
        class LogData:::logStyle {
            +Timestamp: DateTime «property»
            +Name: string «property»
            +Source: string «property»
            +Target: string «property»
            +Size: long «property»
            +TransferTime: long «property»
            +EncryptionTime: long «property»
            +ToJSON() string
            +ToXML() string
        }
    }

    %% ─── Styles par namespace ───────────────────────────────────────────
    classDef guiStyle     fill:#ffd6d6,stroke:#cc0000,color:#000
    classDef consoleStyle fill:#fff3cd,stroke:#d4930a,color:#000
    classDef coreStyle    fill:#d4edda,stroke:#28a745,color:#000
    classDef logStyle     fill:#cce5ff,stroke:#0056b3,color:#000

    %% ─── Relations GUI ──────────────────────────────────────────────────
    Program *-- MainWindow : creates
    Program *-- MainWindowViewModel : creates
    MainWindow --> MainWindowViewModel : binds to
    ViewModelBase <|-- MainWindowViewModel
    ViewModelBase <|-- HomeViewModel
    ViewModelBase <|-- SettingsViewModel
    ViewModelBase <|-- LogsViewModel
    MainWindowViewModel *-- HomeViewModel : creates
    MainWindowViewModel *-- SettingsViewModel : creates
    MainWindowViewModel --> Configuration : uses
    MainWindowViewModel ..> LocalizationHelper : uses
    HomeViewModel --> IBackupService : uses
    HomeViewModel --> Configuration : uses
    SettingsViewModel --> Configuration : uses
    LocalizationHelper --> LanguageManager : delegates to

    %% ─── Relations Core ─────────────────────────────────────────────────
    IBackupService <|.. BackupService : implements
    BackupService --> Configuration : reads config
    BackupService *-- "*" BackupJobController : manages
    BackupJobController --> JobControlState : has state
    BackupJobController ..> BackupState : creates & updates
    BackupService ..> LogData : creates
    BackupService --> ILogger : calls
    BackupService --> CryptoSoftService : uses
    BackupService --> BusinessSoftwareMonitor : checks
    Configuration *-- "*" BackupJob : contains
    BackupJob --> BackupType : has type
    Configuration --> LogMode : uses

    %% ─── Relations EasyLog ──────────────────────────────────────────────
    ILogger <|.. Logger : implements
    ILogger <|.. LogDispatcher : implements
    LogDispatcher --> Logger : local write
    LogDispatcher --> RemoteLogService : remote write
    Logger ..> LogData : writes
    RemoteLogService ..> LogData : sends

    %% ─── Relations ConsoleApp ───────────────────────────────────────────
    ServiceContainer *-- Configuration : creates
    ServiceContainer *-- ConsoleView : creates
    ServiceContainer *-- BackupService : creates
    ServiceContainer *-- BusinessSoftwareMonitor : creates
    ServiceContainer *-- CryptoSoftService : creates
    ServiceContainer --> LanguageManager : uses
    ServiceContainer --> ILogger : uses
    MenuController --> ServiceContainer : uses
    MenuController --> BackupService : calls
    MenuController --> Configuration : reads
    MenuController --> ConsoleView : uses
    MenuController --> LanguageManager : uses
    ConsoleView --> LanguageManager : uses
    ConsoleView ..> BackupState : displays
```
 
## 3. Sequence Diagram Creation backup

#### Here are some sequence diagram examples (creation, execution, deletion, etc.). More sequence diagrams can be added; these are just examples.


#### A Sequence Diagram is a behavioral UML diagram that shows how objects interact in a particular scenario over time, displaying the sequence of messages exchanged between participants.This diagram illustrates the step-by-step process of creating a new backup job. It shows the user providing job details through the console, the system checking if the maximum limit of 5 jobs has been reached, validating the new job, and persisting it to the configuration file. The alternative flow handles the case where the user has already reached the maximum number of jobs.


```mermaid
sequenceDiagram
    actor User
    participant MainWindow
    participant HomeViewModel
    participant Configuration

    User->>MainWindow: Click "Create backup job"
    MainWindow->>HomeViewModel: CreateJobCommand()

    HomeViewModel->>Configuration: AddJob(job)
    Configuration->>Configuration: job.Validate()
    Configuration->>Configuration: SaveConfig()
    Configuration-->>HomeViewModel: Success

    HomeViewModel-->>MainWindow: Update Jobs list
    MainWindow-->>User: Job displayed in list
```

## 4. Sequence Diagram Execution backup

#### This diagram details the execution flow of a backup job. It demonstrates how the system transitions through states (ACTIVE → COMPLETED), processes each file in a loop, logs transfer information for every copied file, and continuously updates the progress displayed to the user. 

```mermaid
sequenceDiagram
    actor User
    participant MainWindow
    participant HomeViewModel
    participant IBackupService
    participant BusinessSoftwareMonitor
    participant CryptoSoftService
    participant BackupState
    participant ILogger

    User->>MainWindow: Click "Execute"
    MainWindow->>HomeViewModel: ExecuteJobCommand()
    HomeViewModel->>IBackupService: ExecuteJob()

    IBackupService->>BusinessSoftwareMonitor: IsRunning()

    alt Business software detected
        BusinessSoftwareMonitor-->>IBackupService: true
        IBackupService->>ILogger: WriteLog()
        IBackupService-->>HomeViewModel: Backup blocked
        HomeViewModel-->>MainWindow: Display error
    else No business software
        BusinessSoftwareMonitor-->>IBackupService: false
        IBackupService->>BackupState: new BackupState()

        loop For each file in source
            IBackupService->>BackupService: CopyFile()
            IBackupService->>CryptoSoftService: EncryptFile()
            IBackupService->>ILogger: WriteLog()
            IBackupService->>BackupState: UpdateStateJSON()
        end

        IBackupService-->>HomeViewModel: Backup completed
        HomeViewModel-->>MainWindow: Update status
    end
```



## 5. Sequence Diagram Supression backup

#### This diagram shows the important process of deleting a backup job. It emphasizes the confirmation step before deletion, ensuring the user consciously agrees to remove the job. This prevents accidental data loss.
```mermaid
   sequenceDiagram
    actor User
    participant MainWindow
    participant HomeViewModel
    participant Configuration

    User->>MainWindow: Click "Delete job"
    MainWindow->>HomeViewModel: DeleteJobCommand()

    HomeViewModel->>Configuration: RemoveJob()
    Configuration->>Configuration: SaveConfig()
    Configuration-->>HomeViewModel: Job removed

    HomeViewModel-->>MainWindow: Update Jobs list
    MainWindow-->>User: Job removed from list  
```

## 6. Sequence Diagram switch languish

#### This diagram shows how the application handles language changes at runtime. When the user selects a new language, the LanguageManager loads the appropriate translations, the configuration is saved to persist the preference, and the interface refreshes to display text in the selected language. This supports the requirement (French/English) of the application.
```mermaid
sequenceDiagram
    actor User
    participant MainWindow
    participant MainWindowViewModel
    participant LocalizationHelper
    participant LanguageManager

    User->>MainWindow: Select language
    MainWindow->>MainWindowViewModel: SwitchLanguageCommand(lang)

    MainWindowViewModel->>LocalizationHelper: SwitchLanguage(lang)
    LocalizationHelper->>LanguageManager: SetLanguage(lang)
    LanguageManager->>LanguageManager: LoadTranslations()
    LanguageManager-->>LocalizationHelper: done
    LocalizationHelper-->>MainWindowViewModel: Language changed


    MainWindowViewModel-->>MainWindow: Update UI bindings
    MainWindow-->>User: Interface refreshed
```

## 7. Sequence Diagram differential backup

#### This diagram highlights the key difference between full and differential backups. Unlike a full backup that copies all files, a differential backup checks each file's modification date and only copies files that have changed since the last backup.
```mermaid
sequenceDiagram
    actor User
    participant MainWindow
    participant HomeViewModel
    participant IBackupService
    participant CryptoSoftService
    participant ILogger

    User->>MainWindow: Click "Execute differential"
    MainWindow->>HomeViewModel: ExecuteJobCommand()
    HomeViewModel->>IBackupService: ExecuteJob()

   IBackupService->>BackupService: Check job.Type == Differential

    loop For each file in source
        IBackupService->>BackupService: IsFileModified()

        alt File modified
            IBackupService->>BackupService: CopyFile()
            IBackupService->>CryptoSoftService: EncryptFile()
            IBackupService->>ILogger: WriteLog()
        else File unchanged
            IBackupService->>BackupService: Skip file
        end
    end

    IBackupService-->>HomeViewModel: Backup completed
    HomeViewModel-->>MainWindow: Update status
```

## 8. Activity diagram

#### An Activity Diagram is a behavioral UML diagram that models the workflow or business process of a system, showing the sequence of activities and decision points from start to finish. This comprehensive diagram provides a complete view of the backup execution workflow. It maps every step from job validation to completion, including decision points for backup type (full vs. differential), file comparison logic, error handling, logging operations, and progress tracking.

```mermaid
flowchart TD
    Start([Start]) --> LoadJobs[Load selected jobs]
    LoadJobs --> LaunchPar[Launch all jobs in parallel]

    LaunchPar --> ValidateJob{Job valid?}
    ValidateJob -->|No| DisplayError[Display error message]
    DisplayError --> EndFail([End])

    ValidateJob -->|Yes| SetActive[Set state to ACTIVE]
    SetActive --> ScanSource[Scan source directory]

    ScanSource --> CheckType{Backup type?}
    CheckType -->|Full| ProcessAll[Process all files]
    CheckType -->|Differential| FilterModified[Filter modified files only]

    ProcessAll --> CopyLoop
    FilterModified --> CopyLoop

    CopyLoop[Next file] --> CheckPause{Paused or stopped?}
    CheckPause -->|Stopped| EndStop([End - STOPPED])
    CheckPause -->|Paused| CheckPause
    CheckPause -->|Running| CheckBusiness{Business software running?}

    CheckBusiness -->|Yes| CheckBusiness
    CheckBusiness -->|No| CheckPriority{Non-priority file & priority pending?}

    CheckPriority -->|Yes| CheckPriority
    CheckPriority -->|No| CheckLarge{File size > n Ko & slot busy?}

    CheckLarge -->|Yes| WaitSlot[Wait for large file slot]
    WaitSlot --> CopyFile[Copy file]
    CheckLarge -->|No| CopyFile

    CopyFile --> CheckEncrypt{File eligible for encryption?}
    CheckEncrypt -->|Yes| Encrypt[Encrypt via CryptoSoft]
    Encrypt --> LogFile[Log transfer details]
    CheckEncrypt -->|No| LogFile

    LogFile --> UpdateProgress[Update state & progress]
    UpdateProgress --> MoreFiles{More files?}
    MoreFiles -->|Yes| CopyLoop
    MoreFiles -->|No| SetCompleted[Set state to COMPLETED]
    SetCompleted --> EndSuccess([End - COMPLETED])

    style Start fill:#2196F3,color:#fff
    style EndFail fill:#f44336,color:#fff
    style EndStop fill:#f44336,color:#fff
    style EndSuccess fill:#4CAF50,color:#fff
    style ValidateJob fill:#FFE4B5
    style CheckType fill:#FFE4B5
    style CheckPause fill:#FFE4B5
    style CheckBusiness fill:#FFE4B5
    style CheckPriority fill:#FFE4B5
    style CheckLarge fill:#FFE4B5
    style CheckEncrypt fill:#FFE4B5
    style MoreFiles fill:#FFE4B5
```

