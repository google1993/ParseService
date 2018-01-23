using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ParseService.DB;
using ParseService.Elements;
using System.Data.Entity;

namespace ParseService
{
    class MainProg
    {
        static void Main()
        {
            MainBody().GetAwaiter().GetResult();
        }

        static private async Task MainBody()
        {
            while (true)
            {
                Console.WriteLine(DateTime.Now.ToString(@"MM.dd.yyyy HH:mm") + " Get HTML.");
                List<string> html = new HtmlClass().GetHtml(@"http://10.10.45.153/stat.shtml");

                Console.WriteLine(DateTime.Now.ToString(@"MM.dd.yyyy HH:mm") + " Parse HTML.");
                List<Process> processes = new HtmlClass().ParseToProcess(html);

                Console.WriteLine(DateTime.Now.ToString(@"MM.dd.yyyy HH:mm") + " Aply Changes.");
                Database.SetInitializer<SostoyanieContext>(null);
                using (SostoyanieContext db = new SostoyanieContext())
                {
                    foreach (var process in processes)
                    {
                        int? id = null;
                        var result = (from p in db.Processes
                                      where p.Pech == process.Pech &&
                                            p.PrefixPlav == process.PrefixPlav &&
                                            p.NomerPlav == process.NomerPlav &&
                                            p.OperFlag == process.OperFlag &&
                                            p.Operation == process.Operation &&
                                            p.Start == process.Start
                                      orderby p.ProcessId descending
                                      select p).ToList();
                        if (result.Count() > 0)
                            id = result.First().ProcessId;
                        if (!id.HasValue)
                        {
                            db.Processes.Add(process);
                        }
                        else
                        {
                            Process a = db.Processes.Find(id);
                            a.ChangeParam(process);
                        }
                    }
                    Console.WriteLine(DateTime.Now.ToString(@"MM.dd.yyyy HH:mm") + " Save Changes.");
                    db.SaveChanges();
                }
                Console.WriteLine(DateTime.Now.ToString(@"MM.dd.yyyy HH:mm") + " Wait a minute.");
                await Task.Delay(60000);
            }
        }
    }
}
