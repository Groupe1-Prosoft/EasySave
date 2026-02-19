# EasySave - User Guide

Welcome to the EasySave User Guide. This document explains how to use the EasySave console application to manage, execute, and monitor your backup jobs.

---

## 1. Getting Started

When you launch the application for the first time, you will be prompted to select your preferred language:

* Press `1` for English
* Press `2` for French

Your choice is saved automatically for future sessions.

---

## 2. Main Menu Navigation

After selecting your language, you will access the main interactive menu. Type the number corresponding to your choice and press `Enter`.

### Option 1: Create a backup job
EasySave allows you to save up to **5 different backup configurations**. To create a new job, the system will ask you for:

1.  **Name:** A descriptive name for your backup (e.g., "WorkDocuments").
2.  **Source Path:** The absolute path of the folder you want to copy (e.g., `C:\Users\Name\Documents`).
3.  **Target Path:** The absolute path where the backup will be stored (e.g., `D:\Backups\Docs`).
4.  **Type:** * `1` for **Full Backup**: Copies all files from the source to the target, overwriting existing ones.
    * `2` for **Differential Backup**: Compares source and target files. It only copies files that are new or have been modified since the last backup.

### Option 2: Execute a backup job
This option allows you to run your configured backups.
* You can type a single job ID (e.g., `1`).
* You can type multiple IDs separated by a semicolon to run them sequentially (e.g., `1;3;5`).

### Option 3: List backup jobs
Displays a table of your currently saved backup jobs, showing their ID, Name, Source, Target, and Type.

### Option 4: Delete a backup job
Type the ID of the job you want to remove. The application will ask for a final confirmation before deleting the configuration.

### Option 5: Exit
Safely closes the EasySave application.

---

## 3. Command Line Interface (CLI) Usage

If you prefer automating tasks, you can execute backup jobs directly from your terminal using arguments.

* **Execute a single job** (e.g., Job 1):
    ```bash
    dotnet run --project EasySave -- 1
    ```
* **Execute a range of jobs** (e.g., Jobs 1 through 3):
    ```bash
    dotnet run --project EasySave -- 1-3
    ```
* **Execute specific scattered jobs** (e.g., Jobs 1 and 3):
    ```bash
    dotnet run --project EasySave -- "1;3"
    ```

---

## 4. Monitoring Your Backups

EasySave keeps track of everything automatically through two main files located in the application directory:

### Real-Time Progress (`state.json`)
This file is updated continuously while a backup is running. You can open it to see:
* The current file being copied.
* The number of files remaining.
* The overall progression percentage.
* The current state (**ACTIVE**, **COMPLETED**, or **ERROR**).

### Daily Execution Logs (`log.json` / `log.xml`)
Every file copied generates a log entry. These files are created daily (named `YYYY-MM-DD.json`) and contain:
* The exact timestamp of the transfer.
* The source and target paths.
* The size of the file.
* The transfer time (in milliseconds).

---

## 5. Troubleshooting Common Errors

* **"Maximum number of jobs reached"**: You must delete an existing backup job (Option 4) before creating a new one.
* **"Directory does not exist"**: Double-check the path you entered for your Source or Target. Ensure you have the right permissions to access these folders.
