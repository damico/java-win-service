using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;

using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Collections;

namespace JavaWinService
{
    public partial class Service1 : ServiceBase
    {

        private string servicename = null;
        private string execbatch = null;
        private string lockfile = null;

        public Service1()
        {
            InitializeComponent();




        }

        protected override void OnStart(string[] args)
        {

            IDictionary props = ReadDictionaryFile("c:/javawinservice/jws.properties");
            servicename = (string)props["servicename"];
            execbatch = (string)props["execbatch"];
            lockfile = (string)props["lockfile"];

            if (!System.Diagnostics.EventLog.SourceExists(servicename + "_Source")) System.Diagnostics.EventLog.CreateEventSource(servicename + "_Source", servicename + "_Log");

            eventLog1.Source = servicename + "_Source";
            eventLog1.Log = servicename + "_Log";
            eventLog1.WriteEntry(servicename + " OnStart");

            eventLog1.WriteEntry(servicename + " OnStart: " + execbatch);
            ExecuteCommandSync(execbatch);
        }

        protected override void OnStop()
        {
            IDictionary props = ReadDictionaryFile("c:/javawinservice/jws.properties");
            servicename = (string)props["servicename"];
            execbatch = (string)props["execbatch"];
            lockfile = (string)props["lockfile"];

            if (!System.Diagnostics.EventLog.SourceExists(servicename + "_Source")) System.Diagnostics.EventLog.CreateEventSource(servicename + "_Source", servicename + "_Log");

            eventLog1.Source = servicename + "_Source";
            eventLog1.Log = servicename + "_Log";
            try
            {


                eventLog1.WriteEntry(servicename + " OnStop");
                int pid = Int32.Parse(readPidFromLockFile(lockfile));
                FindAndKillProcess(pid);
                File.Delete(lockfile);
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry(servicename + " OnStop: " + e.Message);
            }
        }

        public void ExecuteCommandSync(string command)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = command;
                p.Start();

            } catch (Exception objException)
            {
                eventLog1.WriteEntry("JavaWinService: " + objException.Message);
            }
        }

        public bool FindAndKillProcess(int pid)
        {

            foreach (Process clsProcess in Process.GetProcesses())
            {

                if (clsProcess.Id == pid)
                {

                    clsProcess.Kill();
                    return true;
                }
            }
            return false;
        }

        public string readPidFromLockFile(string filename)
        {
            TextReader tr = new StreamReader(filename);
            string ret = tr.ReadLine();
            Console.WriteLine(ret);
            tr.Close();
            return ret;
        }

        public IDictionary ReadDictionaryFile(string fileName)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(fileName))
            {
                if ((!string.IsNullOrEmpty(line)) &&
                    (!line.StartsWith(";")) &&
                    (!line.StartsWith("#")) &&
                    (!line.StartsWith("'")) &&
                    (line.Contains("=")))
                {
                    int index = line.IndexOf('=');
                    string key = line.Substring(0, index).Trim();
                    string value = line.Substring(index + 1).Trim();

                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                    dictionary.Add(key, value);
                }
            }

            return dictionary;
        }
    }
}
