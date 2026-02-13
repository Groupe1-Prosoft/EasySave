using System;
using System.Collections.Generic;

namespace EasySave.Services
{
    /// <summary>
    /// Parses a single command-line argument into an ordered list of job indices.
    /// Supported formats: "start-end", "a;b;c", or "n".
    /// Uses int.Parse and throws if numeric parsing fails.
    /// </summary>
    public class CommandLineService
    {
        /// <summary>
        /// Converts the argument into a list of job indices without validation.
        /// </summary>
        public List<int> ParseArgument(string argument)
        {
            List<int> jobIndices = new List<int>();

            if (argument.Contains("-"))
            {
                string[] parts = argument.Split('-');
                int start = int.Parse(parts[0]);
                int end = int.Parse(parts[1]);
                for (int i = start; i <= end; i++)
                {
                    jobIndices.Add(i);
                }
            }
            else if (argument.Contains(";"))
            {
                string[] parts = argument.Split(';');
                foreach (string part in parts)
                {
                    jobIndices.Add(int.Parse(part));
                }
            }
            else
            {
                jobIndices.Add(int.Parse(argument));
            }

            return jobIndices;
        }
    }
}