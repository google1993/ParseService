using System;
using System.Threading.Tasks;
using ParseServiceNC2.DB;
using System.Collections.Generic;

namespace ParseServiceNC2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(DateTime.Now + ": Read config");
            if (!ConfigClass.GetConfig()) return;
            Console.WriteLine(DateTime.Now + ": Test parametrs");
            if (!ConfigClass.TestConfig()) return;
            Console.WriteLine(DateTime.Now + ": Start program");
            MainBody();
        }

        static void MainBody()
        {
            int delay = 0;
            while (true)
            {
                Task.Delay(delay).GetAwaiter().GetResult();
                delay = ConfigClass.Interval;
                LogClass.CreateInfoLog("Start cycle");

                List<PechStatus> pechStats = HtmlClass.ParseHtml();
                if (pechStats.Count <= 0)
                    continue;

                pechStats = DatabaseClass.FormatStatus(pechStats);
                if (pechStats.Count <= 0)
                    continue;
                
                List<PechStatus> lastStats = new List<PechStatus>();
                if (!DatabaseClass.GetLastStatus(out lastStats))
                    continue;

                List<PechStatus> writeNew = new List<PechStatus>();
                List<PechStatus> update = new List<PechStatus>();
                foreach (var s in pechStats)
                {
                    PechStatus a = lastStats.Find(x => x.Pech == s.Pech);
                    if (a == null || a.Plavka != s.Plavka || a.OperationId != s.OperationId ||
                        a.Start != s.Start)
                        writeNew.Add(s);
                    else
                    {
                        s.ProcessId = a.ProcessId;
                        update.Add(s);
                    }
                }
                DatabaseClass.UdateStatus(update);
                DatabaseClass.AddStatus(writeNew);
            }
        }
    }
}
