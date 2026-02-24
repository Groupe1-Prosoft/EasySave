using System;
using System.Diagnostics;
using System.IO;

namespace EasySave.Services
{
    /// <summary>
    /// Detects if a specified business software process is currently running.
    /// </summary>
    public class BusinessSoftwareMonitor
    {
        private string processName = string.Empty;

        // Sets the name of the process to monitor
        public void SetProcessName(string name)
        {
            processName = name;
        }

        // Checks if the specified process is currently running
        public bool IsRunning()
        {
            // Return false if no process name is set
            if (string.IsNullOrEmpty(processName))
                return false;

            string nameToCheck = processName;

            // Remove ".exe" if present because GetProcessesByName does not support it
            if (nameToCheck.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                nameToCheck = Path.GetFileNameWithoutExtension(nameToCheck);
            }

            // Return true if at least one process with this name is found
            return Process.GetProcessesByName(nameToCheck).Length > 0;
        }
    }
}