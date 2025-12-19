using System;
using System.Diagnostics;

namespace CncLocalRelay
{
    public class HostControl
    {
        readonly static string lineData = "127.0.0.1 natneg.server.cnc-online.net";

        public static void EnableHostRecord(bool Enable)
        {
            string filePath = GetHostLocation();

            if (Enable)
            {
                if (!FileControl.LineExists(filePath, lineData))
                {
                    Trace.WriteLine("Adding host record.");
                    FileControl.AddLineToFile(filePath, lineData);
                }
            }
            else
            {
                if (FileControl.LineExists(filePath, lineData))
                {
                    Trace.WriteLine("Removing host record");
                    FileControl.RemoveLineFromFile(filePath, lineData);
                }
            }
        }

        public static bool CheckHostRecord()
        {
            return FileControl.LineExists(GetHostLocation(), lineData);
        }

        public static string GetHostLocation()
        {
            if (OperatingSystem.IsWindows())
            {
                return "C:\\Windows\\System32\\drivers\\etc\\hosts";
            }
            else if (OperatingSystem.IsLinux())
            {
                return "/etc/hosts";
            }
            else
            {
                throw new Exception("Unsupported OS");
            }
        }


    }
}
