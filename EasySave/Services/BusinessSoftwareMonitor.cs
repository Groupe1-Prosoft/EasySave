using System.Diagnostics;


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

            return Process.GetProcessesByName(processName).Length > 0;
        }


    } 
}





