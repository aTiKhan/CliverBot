﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Management;
using System.Diagnostics;

namespace Cliver
{
    public static class SystemInfo
    {
        public static List<string> GetScreenshotFiles()
        {
            string temp_dir = PathRoutines.CreateDirectory(Path.GetTempPath() + "\\" + ProgramRoutines.GetAppName());
            DateTime delete_time = DateTime.Now.AddDays(-3);
            foreach (FileInfo fi in (new DirectoryInfo(temp_dir)).GetFiles())
                if (fi.LastWriteTime < delete_time)
                    try
                    {
                        fi.Delete();
                    }
                    catch { }
            List<string> files = new List<string>();
            string file = temp_dir + "\\screenshot_" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + ".jpg";
            int display = 1;
            foreach (Screen s in Screen.AllScreens)
            {
                string f;
                if (display == 1)
                    f = file;
                else
                    f = PathRoutines.InsertSuffixBeforeFileExtension(file, "_" + display);
                display++;
                Rectangle bounds = s.Bounds;
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    }
                    bitmap.Save(f, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                files.Add(f);
            }
            return files;
        }

        public static string GetWindowsVersion()
        {
            List<string> vs = new List<string>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption, Version, CSDVersion FROM Win32_OperatingSystem"))
                foreach (var os in searcher.Get())
                    vs.Add("" + os["Caption"] + ", " + os["Version"] + ", " + os["CSDVersion"]);
            if (vs.Count > 0)
                return vs[0];
            return Environment.OSVersion.ToString();
        }

        static public ulong GetTotalPhysicalMemory()
        {
            return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
        }

        static public TimeSpan GetUpTime()
        {
            using (var uptime = new PerformanceCounter("System", "System Up Time"))
            {
                uptime.NextValue(); //Call this an extra time before reading its value
                return TimeSpan.FromSeconds(uptime.NextValue());
            }
        }

        public static List<ProcessorInfo> GetProcessorInfo()
        {
            List<ProcessorInfo> pis = new List<ProcessorInfo>();
            using (ManagementObjectSearcher win32Proc = new ManagementObjectSearcher("select * from Win32_Processor")
                //win32CompSys = new ManagementObjectSearcher("select * from Win32_ComputerSystem"),
                //win32Memory = new ManagementObjectSearcher("select * from Win32_PhysicalMemory")
                )
            {
                foreach (ManagementObject mo in win32Proc.Get())
                {
                    pis.Add(new ProcessorInfo
                    {
                        clockSpeed = mo["CurrentClockSpeed"].ToString(),
                        procName = mo["Name"].ToString(),
                        manufacturer = mo["Manufacturer"].ToString(),
                        version = mo["Version"].ToString(),
                    });
                }
            }
            return pis;
        }
        public class ProcessorInfo
        {
            public string clockSpeed;
            public string procName;
            public string manufacturer;
            public string version;
        }

        public static Dictionary<string, DiskInfo> GetDiskInfo()
        {
            Dictionary<string, DiskInfo> dis = new Dictionary<string, DiskInfo>();
            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                if (!d.IsReady)
                    continue;
                dis[d.Name] = new DiskInfo { total = d.TotalSize, free = d.TotalFreeSpace };
            }
            return dis;
        }
        public class DiskInfo
        {
            public long total;
            public long free;
        }

        //string disk_size2string(int size)
        //{
        //    foreach(uint m in new uint[] {2^30, 2^20, 2^10, 1})
        //    string s = 
        //}

        public static IPAddress GetLocalIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip;
            }
            return null;
        }
    }
}