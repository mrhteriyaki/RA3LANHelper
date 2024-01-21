using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CncLocalRelay
{
    public class FileControl
    {
        public static bool LineExists(string filePath, string lineToCheck)
        {
            if (!File.Exists(filePath))
            {
                return false; // If the file doesn't exist, the line cannot exist.
            }

            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (line.Equals(lineToCheck, StringComparison.OrdinalIgnoreCase))
                {
                    return true; // Line exists in the file.
                }
            }

            return false; // Line not found in the file.
        }

        public static bool LineStarts(string filePath, string lineToCheck)
        {
            if (!File.Exists(filePath))
            {
                return false; // If the file doesn't exist, the line cannot exist.
            }

            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (line.StartsWith(lineToCheck, StringComparison.OrdinalIgnoreCase))
                {
                    return true; 
                }
            }

            return false; 
        }


        public static void AddLineToFile(string filePath, string lineToAdd)
        {
            using (StreamWriter writer = File.AppendText(filePath))
            {
                writer.WriteLine(lineToAdd);
            }
        }

       
        public static void RemoveLineFromFile(string filePath, string lineToRemove)
        {
            string[] lines = File.ReadAllLines(filePath);
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                foreach (string line in lines)
                {
                    if (!line.Equals(lineToRemove, StringComparison.OrdinalIgnoreCase))
                    {
                        writer.WriteLine(line);
                    }
                }
            }
        }

        public static void RemoveStartingWithFromFile(string filePath, string startofLineToRemove)
        {
            string[] lines = File.ReadAllLines(filePath);
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                foreach (string line in lines)
                {
                    if (!line.StartsWith(startofLineToRemove, StringComparison.OrdinalIgnoreCase))
                    {
                        writer.WriteLine(line);
                    }
                }
            }
        }

    }
}
