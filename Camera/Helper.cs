using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Camera
{
    public class IPMacMapper
    {
        private static List<IPAndMac> list;

        private static StreamReader ExecuteCommandLine(String file, String arguments = "")
        {
            var cmd = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
            cmd.Start();

            cmd.StandardInput.WriteLine("chcp 437"); //rus 866
            cmd.StandardInput.Flush();
            cmd.StandardInput.WriteLine("ping 192.168.0.255 -n 0");
            cmd.StandardInput.WriteLine("arp -a"); //rus 866

            cmd.StandardInput.Close();
            return cmd.StandardOutput;


            //ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.CreateNoWindow = true;
            //startInfo.WindowStyle = ProcessWindowStyle.Normal;
            //startInfo.UseShellExecute = false;
            //startInfo.RedirectStandardOutput = true;
            //startInfo.FileName = file;
            //startInfo.Arguments = arguments;

            //Process process = Process.Start(startInfo);

        }

        private static void InitializeGetIPsAndMac()
        {


            var arpStream = ExecuteCommandLine("arp", "-a");
            List<string> result = new List<string>();
            while (!arpStream.EndOfStream)
            {
                var line = arpStream.ReadLine().Trim();
                result.Add(line);
            }

            list = result.Where(x => !string.IsNullOrEmpty(x) && (x.Contains("dynamic") || x.Contains("static")))
                .Select(x =>
                {
                    string[] parts = x.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    return new IPAndMac { IP = parts[0].Trim(), MAC = parts[1].Trim() };
                }).ToList();
        }

        public static string FindIPFromMacAddress(string macAddress)
        {
            macAddress = macAddress.Replace(':', '-');
            InitializeGetIPsAndMac();
            IPAndMac item = list.SingleOrDefault(x => x.MAC == macAddress.ToLower());
            if (item == null)
                return null;
            return item.IP;
        }

        public static string FindMacFromIPAddress(string ip)
        {
            InitializeGetIPsAndMac();
            IPAndMac item = list.SingleOrDefault(x => x.IP == ip);
            if (item == null)
                return null;
            return item.MAC;
        }

        private class IPAndMac
        {
            public string IP { get; set; }
            public string MAC { get; set; }
        }
    }
}
