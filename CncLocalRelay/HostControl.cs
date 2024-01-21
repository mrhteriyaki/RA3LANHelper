using System;

namespace CncLocalRelay
{
    public class HostControl
    {
        readonly static string filePath = "C:\\Windows\\System32\\drivers\\etc\\hosts";
        readonly static string lineData = "127.0.0.1 natneg.server.cnc-online.net";

        public static void EnableHostRecord(bool Enable)
        {
            if (Enable)
            {
                if (!FileControl.LineExists(filePath, lineData))
                {
                    Console.WriteLine("Adding host record.");
                    FileControl.AddLineToFile(filePath, lineData);
                }
            }
            else
            {
                if (FileControl.LineExists(filePath, lineData))
                {
                    Console.WriteLine("Removing host record");
                    FileControl.RemoveLineFromFile(filePath, lineData);
                }
            }
            
        }

        public static bool CheckHostRecord()
        {
            return FileControl.LineExists(filePath, lineData);
        }


    }
}
