using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ParseServiceNC2.DB;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ParseServiceNC2
{
    static class ConfigClass
    {
        public enum Level { None, Error, Info, Debug};
        public enum Sql { MySql, MSSql };

        static private int interval;
        static private Sql sqlProvider;
        static private string connectionString;
        static private string dirLog;
        static private Level logLevel = Level.None;

        static private string parsePage;
        static private string parseCharset = "";
        static private bool parseProxy = false;
        static private string parseProxyAddress;
        static private string parseProxyUsername;
        static private string parseProxyPassword;

        static private bool sendData = false;
        static private string sendAddress = "";
        static private bool sendProxy = false;
        static private string sendProxyAddress;
        static private string sendProxyUsername;
        static private string sendProxyPassword;


        #region GetParams
        static public int Interval { get { return interval <= 0 ? 60000 : (int)interval ; } }
        static public string ParsePage { get { return string.IsNullOrEmpty(parsePage) ?
                    "http://localhost/": parsePage; } }
        static public string DirLogFile { get { return !string.IsNullOrEmpty(dirLog) && Path.IsPathRooted(dirLog) ?
                    dirLog : Directory.GetCurrentDirectory(); } }
        static public string InfoLogFile { get { return !string.IsNullOrEmpty(dirLog) && Path.IsPathRooted(dirLog) ?
                    Path.Combine(dirLog, "Info.log") : "Info.log"; } }
        static public string DebugLogFile { get { return !string.IsNullOrEmpty(dirLog) && Path.IsPathRooted(dirLog) ?
                    Path.Combine(dirLog, "Debug.log") : "Debug.log"; } }
        static public string ErrorLogFile { get { return !string.IsNullOrEmpty(dirLog) && Path.IsPathRooted(dirLog) ? 
                    Path.Combine(dirLog, "Error.log") : "Error.log"; } }
        static public Level LogLevel { get { return logLevel; } }
        static public Sql SqlProvider { get { return sqlProvider; } }
        static public bool SendProxy { get { return sendProxy; } }
        static public string SendProxyAddress { get { return sendProxyAddress; } }
        static public string SendProxyUsername { get { return sendProxyUsername; } }
        static public string SendProxyPassword { get { return sendProxyPassword; } }
        static public bool ParseProxy { get { return parseProxy; } }
        static public string ParseProxyAddress { get { return parseProxyAddress; } }
        static public string ParseProxyUsername { get { return parseProxyUsername; } }
        static public string ParseProxyPassword { get { return parseProxyPassword; } }
        static public string ParseCharset { get { return parseCharset; } }

        #endregion

        static public bool GetConfig()
        {
            if (!File.Exists("parseprog.conf"))
            {
                LogClass.CreateErrorLog("Exist config file \"parseprog.conf\" in: " + Directory.GetCurrentDirectory());
                return false;
            }

            StreamReader stream = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "parseprog.conf"));
            int countline = 0;
            try
            {
                Level log = Level.None;
                string line;
                while ((line = stream.ReadLine()) != null)
                {
                    countline++;
                    if (string.IsNullOrEmpty(line.Trim()) || line.Trim()[0] == '#')
                        continue;

                    string[] tryline = line.Trim().ToLower().Split(':');

                    if (tryline[0].Trim() == "interval")
                    {
                        if (!int.TryParse(tryline[1].Trim(), out int param))
                            throw new System.ArgumentException("Can't read interval param: line " + countline);
                        interval = param;
                        LogClass.CreateDebugLog("Read interval param");
                        continue;
                    }
                    if (tryline[0].Trim() == "database")
                    {
                        switch (tryline[1].ToLower().Trim()) {
                            case "mssql":
                                sqlProvider = Sql.MSSql;
                                break;
                            case "mysql":
                                sqlProvider = Sql.MySql;
                                break;
                            default:
                                throw new System.ArgumentException("Can't read database param: line " + countline);
                        }
                        LogClass.CreateDebugLog("Read SQL provider");
                        continue;
                    }
                    if (tryline[0].Trim() == "connectionstring")
                    {
                        if (!string.IsNullOrEmpty(tryline[1].Trim()))
                            throw new System.ArgumentException("Line \"connectionstring\" after \":\" must be empty: line " + countline);
                        int subline = 0;
                        while (true)
                        {
                            line = stream.ReadLine();
                            subline++;
                            if (line == null)
                                throw new System.ArgumentException("Can't find start ConnectionString: " +
                                    "line " + countline + " to " + (countline + subline));
                            line = line.Trim();
                            if (line == "" || line[0] == '#')
                                continue;
                            if (line[0] == '{')
                                break;
                        }
                        string connstr = "";
                        if (line.Length >= 2)
                            connstr = line.Substring(1, line.Length - 1).Trim();
                        if (connstr != "" && connstr[connstr.Length - 1] == '}')
                        {
                            connstr = connstr.Substring(0, line.Length - 2);
                            countline += subline;
                            connectionString = connstr.Trim();
                            continue;
                        }
                        while (true)
                        {
                            line = stream.ReadLine();
                            subline++;
                            if (line == null)
                                throw new System.ArgumentException("Can't find end ConnectionString: " +
                                    "line " + countline + " to " + (countline + subline));
                            line = line.Trim();
                            if (line == "" || line[0] == '#')
                                continue;
                            connstr += line;
                            if (connstr[connstr.Length - 1] == '}')
                            {
                                connstr = connstr.Substring(0, connstr.Length - 2);
                                countline += subline;
                                connectionString = connstr.Trim();
                                break;
                            }
                        }
                        LogClass.CreateDebugLog("Read Connection string");
                        continue;
                    }
                    if (tryline[0].Trim() == "loglevel")
                    {
                        switch (tryline[1].ToLower().Trim())
                        {
                            case "none":
                                log = Level.None;
                                break;
                            case "error":
                                log = Level.Error;
                                break;
                            case "info":
                                log = Level.Info;
                                break;
                            case "debug":
                                log = Level.Debug;
                                break;
                            default:
                                throw new System.ArgumentException("Can't read loglevel param: line " + countline);
                        }
                        LogClass.CreateDebugLog("Read LogLevel param");
                        continue;
                    }
                    if (tryline[0].Trim() == "httppage")
                    {
                        if (string.IsNullOrEmpty(tryline[1].Trim()))
                            throw new System.ArgumentException("Can't read http address: line " + countline);
                        string httptest = tryline[1].Trim();
                        for (var i = 2; i < tryline.Length; i++)
                            httptest += ":"+tryline[i];
                        bool result = Uri.TryCreate(httptest, UriKind.Absolute, out Uri uriResult)
                            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                        if (!result)
                            throw new System.ArgumentException("Http address not valid: line " + countline);
                        parsePage = httptest;
                        LogClass.CreateDebugLog("Read Address parse page");
                        continue;
                    }
                    if (tryline[0].Trim() == "parseproxy")
                    {
                        if (!bool.TryParse(tryline[1].Trim(), out bool param))
                            throw new System.ArgumentException("Can't read parseproxy param: line " + countline);
                        parseProxy = param;
                        LogClass.CreateDebugLog("Read parseproxy param");
                        continue;
                    }
                    if (tryline[0].Trim() == "parseproxyaddress")
                    {
                        if (string.IsNullOrEmpty(tryline[1].Trim()))
                            throw new System.ArgumentException("Can't read parse proxy address: line " + countline);
                        string httptest = tryline[1].Trim();
                        for (var i = 2; i < tryline.Length; i++)
                            httptest += ":" + tryline[i];
                        bool result = Uri.TryCreate(httptest, UriKind.Absolute, out Uri uriResult)
                            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                        if (!result)
                            throw new System.ArgumentException("Parse proxy address not valid: line " + countline);
                        parseProxyAddress = httptest;
                        LogClass.CreateDebugLog("Read parse proxy address");
                        continue;
                    }
                    if (tryline[0].Trim() == "parseproxyusername")
                    {
                        parseProxyUsername = tryline[1].ToLower().Trim();
                        LogClass.CreateDebugLog("Read parse proxy username");
                        continue;
                    }
                    if (tryline[0].Trim() == "parseproxypassword")
                    {
                        parseProxyPassword = tryline[1].ToLower().Trim();
                        LogClass.CreateDebugLog("Read parse proxy password");
                        continue;
                    }
                    if (tryline[0].Trim() == "senddata")
                    {
                        if (!bool.TryParse(tryline[1].Trim(), out bool param))
                            throw new System.ArgumentException("Can't read send data param: line " + countline);
                        sendData = param;
                        LogClass.CreateDebugLog("Read send data param");
                        continue;
                    }
                    if (tryline[0].Trim() == "sendaddress")
                    {
                        if (string.IsNullOrEmpty(tryline[1].Trim()))
                            throw new System.ArgumentException("Can't read send address: line " + countline);
                        string httptest = tryline[1].Trim();
                        for (var i = 2; i < tryline.Length; i++)
                            httptest += ":" + tryline[i];
                        bool result = Uri.TryCreate(httptest, UriKind.Absolute, out Uri uriResult)
                            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                        if (!result)
                            throw new System.ArgumentException("Send address not valid: line " + countline);
                        sendAddress = httptest;
                        LogClass.CreateDebugLog("Read send address");
                        continue;
                    }
                    if (tryline[0].Trim() == "sendproxy")
                    {
                        if (!bool.TryParse(tryline[1].Trim(), out bool param))
                            throw new System.ArgumentException("Can't read sendproxy param: line " + countline);
                        sendProxy = param;
                        LogClass.CreateDebugLog("Read sendproxy param");
                        continue;
                    }
                    if (tryline[0].Trim() == "sendproxyaddress")
                    {
                        if (string.IsNullOrEmpty(tryline[1].Trim()))
                            throw new System.ArgumentException("Can't read send proxy address: line " + countline);
                        string httptest = tryline[1].Trim();
                        for (var i = 2; i < tryline.Length; i++)
                            httptest += ":" + tryline[i];
                        bool result = Uri.TryCreate(httptest, UriKind.Absolute, out Uri uriResult)
                            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                        if (!result)
                            throw new System.ArgumentException("Send proxy address not valid: line " + countline);
                        sendProxyAddress = httptest;
                        LogClass.CreateDebugLog("Read send proxy address");
                        continue;
                    }
                    if (tryline[0].Trim() == "sendproxyusername")
                    {
                        sendProxyUsername = tryline[1].ToLower().Trim();
                        LogClass.CreateDebugLog("Read send proxy username");
                        continue;
                    }
                    if (tryline[0].Trim() == "sendproxypassword")
                    {
                        sendProxyPassword = tryline[1].ToLower().Trim();
                        LogClass.CreateDebugLog("Read send proxy password");
                        continue;
                    }

                    LogClass.CreateErrorLog("Can't read param in line " + countline);
                }
                stream.Close();
                logLevel = log;
            }
            catch (System.ArgumentException e)
            {
                LogClass.CreateErrorLog(e.Message);
                stream.Close();
                return false;
            }
            catch
            {
                LogClass.CreateErrorLog("Catch error while read conf on line " + countline);
                stream.Close();
                return false;
            }
            LogClass.CreateInfoLog("Read config: OK");
            return true;
        }
        static public void GetDefaultConfig()
        {
            interval = 60000;
            parsePage = @"http://10.10.45.153/stat.shtml";
            sqlProvider = Sql.MySql;
            connectionString = @"Server = 10.10.2.66; Database = StovesStatus; Uid = test; Pwd = test; Charset=utf8";
            //@"Data Source=10.10.2.181\SQLE;Initial Catalog=CollectInfo;Trusted_Connection=False;Persist Security Info=True;User ID=sa;Password=karusel1993;"
            dirLog = "";
            logLevel = Level.None;
            sendData = false;
        }
        static public bool TestConfig()
        {
            LogClass.CreateDebugLog("Test Connect to DB.");
            try
            {
                using (MyDBContext db = new MyDBContext(connectionString, sqlProvider))
                {
                    var result = (from a in db.PechStatus
                                  where a.ProcessId == 1
                                  select a).ToList();
                }
            }
            catch(Exception e)
            {
                LogClass.CreateErrorLog("DataBase Error: " + e.Message);
                LogClass.CreateErrorLog("Connection string: " + connectionString);
                return false;
            }

            LogClass.CreateDebugLog("Test Connect to parse page.");
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                HttpClient client = ConnectionClass.CreateGetHttpConnect();
                HttpResponseMessage response = client.GetAsync(ParsePage).Result;
                string charset = response.Content.ReadAsStringAsync().Result;
                Match m = Regex.Match(charset, "<meta.*?charset=([^\"']+)");
                if (m.Success)
                    charset = m.Groups[1].Value.ToLower();
                else
                    charset = "utf-8";
                parseCharset = charset != "ibm866" ? charset : "cp866";
                StreamReader readStream = new StreamReader(
                    client.GetStreamAsync(ParsePage).Result,
                    Encoding.GetEncoding(parseCharset));
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

            LogClass.CreateInfoLog("Test config: OK");
            return true;
        }

    }
}
