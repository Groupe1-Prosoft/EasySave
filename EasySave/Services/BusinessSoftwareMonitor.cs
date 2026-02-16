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

        public void SetProcessName(string name)
        {
            processName = name;
        }

        public bool IsRunning()
        {
            if (string.IsNullOrEmpty(processName))
                return false;

            // Ajout nécessaire : On retire ".exe" si présent car GetProcessesByName ne le supporte pas
            string nameToCheck = processName;
            if (nameToCheck.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                nameToCheck = Path.GetFileNameWithoutExtension(nameToCheck);
            }

            return Process.GetProcessesByName(nameToCheck).Length > 0;
        }
    }
}