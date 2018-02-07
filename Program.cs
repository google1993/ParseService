using System;
using System.Threading.Tasks;
using ParseServiceNC2.DB;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.IO;
using System.Text;

namespace ParseServiceNC2
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!ConfigClass.GetConfig()) return;
            if (!ConfigClass.TestConfig()) return;
            MainBody().GetAwaiter().GetResult();
        }

        static async Task MainBody()
        {
            while (true)
            {
                await Task.Delay(ConfigClass.Interval);
                LogClass.CreateInfoLog("Start cycle");
                List<PechStatus> pechStats = HtmlClass.ParseHtml();
                if (pechStats.Count <= 0)
                    continue;

                /*
                using (MyDBContext db = new MyDBContext(connStr, flagMS))
                {
                    await SendPechStats(pechStats);
                    
                    foreach (var pechStat in pechStats)
                    {
                        if (pechStat.Splav.Splav == "-")
                        {
                            pechStat.Splav = null;
                            pechStat.SplavId = null;
                        }
                        else
                        {
                            var result = (from p in db.Splavy
                                          where p.Splav == pechStat.Splav.Splav
                                          select p).ToList();
                            if (result.Count() > 0)
                            {
                                pechStat.Splav = null;
                                pechStat.SplavId = result.FirstOrDefault().SplavId;
                            }
                        }

                        if (pechStat.Ukazanie.Ukazanie == "-")
                        {
                            pechStat.Ukazanie = null;
                            pechStat.UkazanieId = null;
                        }
                        else
                        {
                            var result = (from p in db.Ukazaniya
                                          where p.Ukazanie == pechStat.Ukazanie.Ukazanie
                                          select p).ToList();
                            if (result.Count() > 0)
                            {
                                pechStat.Ukazanie = null;
                                pechStat.UkazanieId = result.FirstOrDefault().UkazanieId;
                            }
                        }

                        if (pechStat.Contract.Contract == "-")
                        {
                            pechStat.Contract = null;
                            pechStat.ContractId = null;
                        }
                        else
                        {
                            var result = (from p in db.Contracts
                                          where p.Contract == pechStat.Contract.Contract
                                          select p).ToList();
                            if (result.Count() > 0)
                            {
                                pechStat.Contract = null;
                                pechStat.ContractId = result.FirstOrDefault().ContractId;
                            }
                        }

                        {
                            var result = (from p in db.Operations
                                          where p.Operation == pechStat.Operation.Operation
                                          select p).ToList();
                            if (result.Count() > 0)
                            {
                                pechStat.Operation = null;
                                pechStat.OperationId = result.FirstOrDefault().OperationId;
                            }
                        }

                        {
                            int? id = null;
                            var result = (from p in db.PechStatus
                                         where p.Pech == pechStat.Pech &&
                                               p.PrefixPlav == pechStat.PrefixPlav &&
                                               p.NomerPlav == pechStat.NomerPlav &&
                                               p.OperFlag == pechStat.OperFlag &&
                                               p.OperationId == pechStat.OperationId &&
                                               p.Start == pechStat.Start
                                         orderby p.ProcessId descending
                                         select p).ToList();
                            if (result.Count() > 0)
                                id = result.First().ProcessId;
                            if (!id.HasValue)
                            {
                                db.PechStatus.Add(pechStat);
                            }
                            else
                            {
                                PechStatus a = db.PechStatus.Find(id);
                                a.ChangeParam(pechStat);
                            }
                        }

                        db.SaveChanges();
                    }
                }*/

            }
        }
        /*
        static async private Task SendPechStats(List<PechStatus> a)
        {
            try
            {
                JArray jPechStats = new JArray();
                foreach (var pechstat in a)
                {
                    JObject o = new JObject{
                            { "Plavka", pechstat.Plavka },
                            { "Splav", pechstat.Splav.Splav },
                            { "Ukazanie", pechstat.Ukazanie.Ukazanie },
                            { "Contract", pechstat.Contract.Contract },
                            { "Nabor", pechstat.Nabor },
                            { "Complect", pechstat.Complect },
                            { "ElectVes", pechstat.ElectVes },
                            { "Operation", pechstat.Operation.Operation },
                            { "Start", pechstat.Start },
                            { "EndTeor", pechstat.EndTeor },
                            { "Prostoy", pechstat.Prostoy }
                        };
                    jPechStats.Add(o);
                }
                HttpClient client = new Connect().CreateConnect(false);
                var httpContent = new StringContent(jPechStats.ToString(),
                    Encoding.UTF8,"application/json");
                HttpResponseMessage response = 
                    await client.PostAsync(@"http://127.0.0.1:5011/setinfo", httpContent);
                Console.WriteLine(response.StatusCode.ToString());
            }
            catch
            {
            }
        }*/

        /*
        static private void GetNewParams(string[] args)
        {
            for(int i =0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-i":
                        i++;
                        int.TryParse(args[i], out interval);
                        break;
                    case "-http":
                        i++;
                        httpPage = args[i];
                        break;
                    case "-mysql":
                        i++;
                        flagMS = false;
                        connStr = args[i];
                        break;
                    case "-mssql":
                        i++;
                        flagMS = true;
                        connStr = args[i];
                        break;
                    case "-log":
                        i++;
                        logFile = args[i];
                        break;
                    case "-debug":
                        i++;
                        bool.TryParse(args[i], out flagDebug);
                        break;
                }

            }
        }
        */
    }
}
