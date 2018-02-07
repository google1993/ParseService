using ParseServiceNC2.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace ParseServiceNC2
{
    static class HtmlClass
    {
        static public List<PechStatus> ParseHtml()
        {
            List<string> html = new List<string>();
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                HttpClient client = ConnectionClass.CreateGetHttpConnect();
                StreamReader readStream = new StreamReader(
                    client.GetStreamAsync(ConfigClass.ParsePage).Result,
                    Encoding.GetEncoding(ConfigClass.ParseCharset));

                for (int i = 0; !readStream.EndOfStream; i++)
                {
                    string line = readStream.ReadLine();
                    string convLine = Regex.Replace(line, "<.*?>", String.Empty).Trim();
                    if (i > 27 && i < 652 && !string.IsNullOrEmpty(convLine))
                        html.Add(convLine);
                }
                readStream.Close();
            }
            catch(Exception e)
            {
                LogClass.CreateErrorLog("Error parse page: " + e.Message);
                return new List<PechStatus>();
            }
            List<PechStatus> pechStats = new List<PechStatus>();
            try
            {
                for (int i = 0; i < html.Count; i += 12)
                    pechStats.Add(new PechStatus()
                    {
                        Plavka = html[i],
                        Splav = new Splavy(html[i + 1]),
                        Ukazanie = new Ukazaniya(html[i + 2]),
                        Contract = new Contracts(html[i + 3]),
                        Nabor = html[i + 4],
                        Complect = html[i + 5],
                        ElectVes = int.Parse(html[i + 6]),
                        Operation = new Operations(html[i + 7]),
                        Start = DateTime.Parse(html[i + 8]),
                        EndTeorStr = html[i + 9],
                        EndFact = DateTime.Now,
                        ProstoyStr = html[i + 10],
                    });
            }
            catch(Exception e)
            {
                LogClass.CreateErrorLog("Error parse page to pechstatus elements: " + e.Message);
                return new List<PechStatus>();
            }
            return pechStats;
        }


    }
}
