using System.Collections.Generic;
using EasySave.Models;

namespace EasySave.Services
{
    /// <summary>
    /// Defines the contract for backup execution.
    /// Depends on abstraction, not on concrete implementation (DIP).
    /// </summary>
    public interface IBackupService
    {
        bool ExecuteJob(BackupJob job);
        bool ExecuteSequential(List<int> ids);
    }
}
