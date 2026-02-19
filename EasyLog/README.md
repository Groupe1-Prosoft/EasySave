# EasyLog — Integration Guide

EasyLog is a standalone logging class library (.NET 8.0) developed for the EasySave project. It writes per-file transfer logs in **JSON** or **XML** format to a daily rotating file.

---

## Compatibility

| EasyLog version | EasySave version | Target framework |
|-----------------|-----------------|------------------|
| 1.0             | V1.0 — V1.1     | .NET 8.0         |
| 1.1             | V2.0            | .NET 8.0         |

EasyLog targets **net8.0** and is compatible with any project targeting .NET 8.0 or higher (including .NET 10.0).

---

## Integration

### Option A — Project reference (recommended for developers in the same solution)

Add the reference to your `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\EasyLog\EasyLog.csproj" />
</ItemGroup>
```

Or via the .NET CLI from your project folder:

```bash
dotnet add reference ../EasyLog/EasyLog.csproj
```

Or in Visual Studio: right-click the project → **Add** → **Project Reference** → tick `EasyLog`.

---

### Option B — Compiled DLL reference

1. Build EasyLog in Release mode:

```bash
dotnet build EasyLog -c Release
```

2. The output DLL is located at:

```
EasyLog/bin/Release/net8.0/EasyLog.dll
```

3. Reference it in your `.csproj`:

```xml
<ItemGroup>
  <Reference Include="EasyLog">
    <HintPath>..\libs\EasyLog.dll</HintPath>
  </Reference>
</ItemGroup>
```

---

## API Reference

### `ILogger` interface

```csharp
namespace EasyLog;

public interface ILogger
{
    /// <summary>
    /// Writes a single log entry. Returns true on success, false on failure.
    /// </summary>
    bool WriteLog(LogData data);
}
```

---

### `LogData` model

```csharp
namespace EasyLog;

public class LogData
{
    public DateTime Timestamp    { get; set; }  // Date and time of the transfer
    public string   Name         { get; set; }  // Backup job name
    public string   Source       { get; set; }  // Full path of the source file
    public string   Target       { get; set; }  // Full path of the destination file
    public long     Size         { get; set; }  // File size in bytes
    public long     TransferTime { get; set; }  // Transfer duration in milliseconds
    public long     EncryptionTime { get; set; }// Encryption duration (ms); 0 = not encrypted; -1 = error
}
```

---

### `Logger` class

```csharp
namespace EasyLog;

public class Logger : ILogger
{
    /// <summary>
    /// Creates a logger. Format: "json" (default) or "xml".
    /// </summary>
    public Logger(string logFormat) { ... }

    /// <summary>
    /// Writes the log entry to today's daily file.
    /// Creates the file and directory if they don't exist.
    /// </summary>
    public bool WriteLog(LogData data) { ... }

    /// <summary>
    /// Returns the path of today's log file (created if missing).
    /// </summary>
    public string CreateDailyLogFile() { ... }
}
```

---

## Usage Example

```csharp
using EasyLog;

// 1. Instantiate with desired format ("json" or "xml")
ILogger logger = new Logger("json");

// 2. Build a log entry
var entry = new LogData
{
    Timestamp      = DateTime.Now,
    Name           = "DocumentsBackup",
    Source         = @"C:\Users\Alice\Documents\report.docx",
    Target         = @"D:\Backup\Documents\report.docx",
    Size           = 204800,       // bytes
    TransferTime   = 45,           // milliseconds
    EncryptionTime = 12            // milliseconds (0 if not encrypted)
};

// 3. Write the log
bool success = logger.WriteLog(entry);
```

---

## Log file location

Log files are created automatically at:

```
%APPDATA%\EasySave\Logs\YYYY-MM-DD.json
%APPDATA%\EasySave\Logs\YYYY-MM-DD.xml
```

A new file is created every day. If the file already exists, new entries are **appended** to the existing array.

---

## JSON log example

```json
[
  {
    "Timestamp": "2025-04-10T14:32:01",
    "Name": "DocumentsBackup",
    "Source": "C:\\Users\\Alice\\Documents\\report.docx",
    "Target": "D:\\Backup\\Documents\\report.docx",
    "Size": 204800,
    "TransferTime": 45,
    "EncryptionTime": 12
  }
]
```

## XML log example

```xml
<?xml version="1.0" encoding="utf-8"?>
<ArrayOfLogData>
  <LogData>
    <Timestamp>2025-04-10T14:32:01</Timestamp>
    <Name>DocumentsBackup</Name>
    <Source>C:\Users\Alice\Documents\report.docx</Source>
    <Target>D:\Backup\Documents\report.docx</Target>
    <Size>204800</Size>
    <TransferTime>45</TransferTime>
    <EncryptionTime>12</EncryptionTime>
  </LogData>
</ArrayOfLogData>
```

---

## Design decisions

- **No static state** — `Logger` is instantiated per service, making it easy to inject and mock in tests.
- **Daily rotation** — one file per day keeps logs manageable and queryable by date.
- **Format agnostic** — the `ILogger` interface hides the JSON/XML detail from callers; swapping format requires only changing the constructor argument.
- **Append semantics** — each `WriteLog()` call reads the current file, appends the new entry, and rewrites the whole array to keep valid JSON/XML structure.
