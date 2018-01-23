using ParseService.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParseService
{
    class HtmlClass
    {
        public List<string> GetHtml(string url)
        {
            List<string> html = new List<string>();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;
                readStream = new StreamReader(receiveStream, Encoding.GetEncoding(866));
                bool flag = false;
                int i = 1;
                while (!readStream.EndOfStream)
                {
                    string line = readStream.ReadLine();
                    string convLine = ClearHtml(line);
                    if (line.IndexOf("<b>") != -1)
                    {
                        if (!flag)
                            flag = true;
                    }
                    else
                    {
                        if (flag)
                        {
                            convLine = convLine + "!";
                            flag = false;
                        }
                    }
                    if (i > 28 && i < 653)
                        html.Add(convLine);
                    i++;
                }
                response.Close();
                readStream.Close();
            }
            return html;
        }

        private string ClearHtml(string line)
        {
            return Regex.Replace(line, "<.*?>", String.Empty).Trim();
        }

        public List<Process> ParseToProcess(List<string> processStr)
        {
            List<Process> processes = new List<Process>();
            for (int i = 0; i < processStr.Count; i += 13)
            {
                Process process = new Process()
                {
                    Plavka = processStr[i],
                    Splav = processStr[i + 1],
                    Ukazanie = processStr[i + 2],
                    Contract = processStr[i + 3],
                    Nabor = processStr[i + 4],
                    Complect = processStr[i + 5],
                    ElectVes = int.Parse(processStr[i + 6]),
                    Operation = processStr[i + 7],
                    OperFlag = (processStr[i + 12] == "!") ? true : false,
                    Start = DateTime.Parse(processStr[i + 8]),
                    EndTeorStr = processStr[i + 9],
                    EndFact = DateTime.Now,
                    ProstoyStr = processStr[i + 10],
                };
                processes.Add(process);
            }
            return processes;
        }
    }
}
