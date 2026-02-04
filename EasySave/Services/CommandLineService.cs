using System;
using System.Collections.Generic;

namespace EasySave.Services
{
    public class CommandLineService
    {

        public List<int> ParseArgument(string argument)
        {
            List<int> jobIndices = new List<int>();

            if (argument.Contains("-"))
            {
                // Format "1-3" → jobs 1, 2, 3
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
                // Format "1;3" → jobs 1 and 3
                string[] parts = argument.Split(';');
                foreach (string part in parts)
                {
                    jobIndices.Add(int.Parse(part));
                }
            }
            else
            {
                // Only one number "2" → job 2
                jobIndices.Add(int.Parse(argument));
            }

            return jobIndices;
        }
    }
}