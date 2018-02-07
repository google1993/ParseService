using System;
using System.Collections.Generic;
using System.Text;
using ParseServiceNC2.DB;
using System.IO;

namespace ParseServiceNC2
{
    static class LogClass
    {
        static public void CreateErrorLog(string information)
        {
            if (ConfigClass.LogLevel < ConfigClass.Level.Error)
                return;
            if (!Directory.Exists(ConfigClass.DirLogFile))
                Directory.CreateDirectory(ConfigClass.DirLogFile);
            using (StreamWriter sw = File.AppendText(ConfigClass.ErrorLogFile))
            {
                sw.WriteLine(DateTime.Now + ": " + information);
            }
        }
        static public void CreateDebugLog(string information)
        {
            if (ConfigClass.LogLevel < ConfigClass.Level.Debug)
                return;
            if (!Directory.Exists(ConfigClass.DirLogFile))
                Directory.CreateDirectory(ConfigClass.DirLogFile);
            using (StreamWriter sw = File.AppendText(ConfigClass.DebugLogFile))
            {
                sw.WriteLine(DateTime.Now + ": " + information);
            }
        }
        static public void CreateInfoLog(string information)
        {
            if (ConfigClass.LogLevel < ConfigClass.Level.Info)
                return;
            if (!Directory.Exists(ConfigClass.DirLogFile))
                Directory.CreateDirectory(ConfigClass.DirLogFile);
            using (StreamWriter sw = File.AppendText(ConfigClass.InfoLogFile))
            {
                sw.WriteLine(DateTime.Now + ": " + information);
            }
        }
    }
}
