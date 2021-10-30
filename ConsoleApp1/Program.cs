using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static List<string> lastProcesses = new List<string>();
        static void Main(string[] args)
        {
            string whiteList = $"Teams\ndevenv\nsystem\nDesktop\nsystem\n{Process.GetCurrentProcess().ProcessName}";
            RegistryKey registry = Registry.CurrentUser;
            if (registry.GetSubKeyNames().Any(item => item.Contains("WhiteList")))
            {
                registry = Registry.CurrentUser.OpenSubKey("WhiteList");
            }
            else
            {
                registry = Registry.CurrentUser.CreateSubKey("WhiteList");
                registry.SetValue("WhiteProc", whiteList);
            }

            try
            {
                if (args[0].Contains("last"))
                {
                    if (File.Exists("lastprocesses.txt"))
                    {
                        lastProcesses = File.ReadAllLines("lastprocesses.txt").ToList();
                    }
                    else
                    {
                        Console.WriteLine("No recent processes!");
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Process.GetProcessesByName(args[0]).ToList().ForEach(item =>
                    {
                        if (whiteList.Contains(item.ProcessName.ToLower()))
                        {
                            Console.WriteLine("Not allowed!");
                            Environment.Exit(0);
                        }
                    });
                    if (Process.GetProcessesByName(args[0]).ToList().Count == 0)
                    {
                        Console.WriteLine("Try again!");
                        Environment.Exit(0);
                    }
                    lastProcesses.Add(args[0]);
                }
                
                Task.Factory.StartNew(() => {
                    KillProcess();
                });

                Console.WriteLine("exit- to exit\nclose - exit with save");
                string dispose = Console.ReadLine();
                if (dispose.Contains("exit"))
                {
                    Environment.Exit(0);
                }
                if (dispose.Contains("close"))
                {
                    File.WriteAllLines("lastprocesses.txt", lastProcesses);
                    Environment.Exit(0);
                }
            }
            catch (Exception ex) { }


            Environment.Exit(0);
        }
        static void KillProcess()
        {
            while (true)
            {
                foreach (var item in lastProcesses)
                {
                    Process.GetProcessesByName(item).ToList().ForEach(item =>
                    {
                        if (lastProcesses.ToList().Any(name => name.ToLower().Contains(item.ProcessName.ToLower())))
                        {
                            item.Kill();
                        }
                    });
                }

                Thread.Sleep(1000);
            }
        }
    }
}
