using Newtonsoft.Json.Linq;
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
        static private string charset = "utf-8";
        static public List<PechStatus> ParseHtml()
        {
            List<string> html = new List<string>();
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                HttpClient client = ConnectionClass.CreateGetHttpConnect();
                StreamReader readStream = new StreamReader(
                    client.GetStreamAsync(ConfigClass.ParsePage).Result,
                    Encoding.GetEncoding(charset));

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
                        Splav = html[i + 1] == "-" ? null : new Splavy(html[i + 1]),
                        Ukazanie = html[i + 2] == "-" ? null : new Ukazaniya(html[i + 2]),
                        Contract = html[i + 3] == "-" ? null : new Contracts(html[i + 3]),
                        Nabor = html[i + 4],
                        Complect = html[i + 5],
                        ElectVes = int.Parse(html[i + 6]),
                        Operation = new Operations(html[i + 7]),
                        Start = DateTime.Parse(html[i + 8]),
                        EndTeorStr = html[i + 9],
                        EndFact = DateTime.Now,
                    });
            }
            catch(Exception e)
            {
                LogClass.CreateErrorLog("Error parse page to pechstatus elements: " + e.Message);
                return new List<PechStatus>();
            }
            return pechStats;
        }
        static public bool TestHtml()
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                HttpClient client = ConnectionClass.CreateGetHttpConnect();
                HttpResponseMessage response = client.GetAsync(ConfigClass.ParsePage).Result;
                string cs = response.Content.ReadAsStringAsync().Result;
                Match m = Regex.Match(cs, "<meta.*?charset=([^\"']+)");
                if (m.Success)
                    cs = m.Groups[1].Value.ToLower();
                else
                    cs = "utf-8";
                charset = cs != "ibm866" ? cs : "cp866";
                StreamReader readStream = new StreamReader(
                    client.GetStreamAsync(ConfigClass.ParsePage).Result,
                    Encoding.GetEncoding(charset));
                string result = "";
                while (!readStream.EndOfStream)
                    result += readStream.ReadLine();
                readStream.Close();
                m = Regex.Match(result, @"<title>\s*(.+?)\s*</title>");
                if (m.Success)
                    result = m.Groups[1].Value;
                else
                    result = "";
                LogClass.CreateDebugLog("Parse HTML title: " + result);
            }
            catch (Exception e)
            {
                LogClass.CreateErrorLog("Parse HTML Error: " + e.Message);
                return false;
            }
            return true;
        }
        static public bool SendPechStats(List<PechStatus> pechstat)
        {
            try
            {
                JArray jPechStats = new JArray();
                foreach (var a in pechstat)
                {
                    JObject o = new JObject{
                            { "Plavka", a.Plavka },
                            { "Splav", a.Splav != null ? a.Splav.Splav : "-" },
                            { "Ukazanie", a.Ukazanie != null ? a.Ukazanie.Ukazanie : "-" },
                            { "Contract", a.Contract != null ? a.Contract.Contract : "-" },
                            { "Nabor", a.Nabor },
                            { "Complect", a.Complect },
                            { "ElectVes", a.ElectVes },
                            { "Operation", a.Operation.Operation },
                            { "Start", a.Start },
                            { "EndTeor", a.EndTeor.HasValue ? a.EndTeor.ToString() : "-" },
                        };
                    jPechStats.Add(o);
                }
                HttpClient client = ConnectionClass.CreateSendJSONConnect();
                var httpContent = new StringContent(jPechStats.ToString(),
                    Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(ConfigClass.SendAddress, httpContent).GetAwaiter().GetResult();
                //LogClass.CreateInfoLog(response.StatusCode.ToString());
            }
            catch(Exception e)
            {
                LogClass.CreateErrorLog("Can't send data to remote server: " + e.Message);
                return false;
            }
            return true;
        }
    }
}
