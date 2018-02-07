using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ParseServiceNC2
{
    static class ConnectionClass
    {
        static private NetworkCredential proxyCredential;
        static private WebProxy proxy;
        static private HttpClient client;
        static private HttpClientHandler httpClientHandler;

        static public HttpClient CreateSendJSONConnect()
        {
            if (ConfigClass.SendProxy)
            {
                if (string.IsNullOrEmpty(ConfigClass.SendProxyUsername))
                {
                    httpClientHandler = new HttpClientHandler()
                    {
                        Proxy = new WebProxy(ConfigClass.SendProxyAddress),
                        UseProxy = true,
                    };
                }
                else
                {
                    proxyCredential = new NetworkCredential(
                        ConfigClass.SendProxyUsername,
                        ConfigClass.SendProxyPassword);
                    proxy = new WebProxy(ConfigClass.SendProxyAddress, false)
                        {
                            UseDefaultCredentials = false,
                            Credentials = proxyCredential,
                        };
                    httpClientHandler = new HttpClientHandler()
                        {
                            Proxy = proxy,
                            PreAuthenticate = true,
                            UseDefaultCredentials = false,
                        };
                    httpClientHandler.Credentials = proxyCredential;
                }
            }
            client = null;
            client = ConfigClass.SendProxy ? new HttpClient(httpClientHandler) : new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("test-api", "1"));
            return client;
        }
        static public HttpClient CreateGetHttpConnect()
        {
            if (ConfigClass.ParseProxy)
            {
                if (string.IsNullOrEmpty(ConfigClass.ParseProxyUsername))
                {
                    httpClientHandler = new HttpClientHandler()
                    {
                        Proxy = new WebProxy(ConfigClass.ParseProxyAddress),
                        UseProxy = true,
                    };
                }
                else
                {
                    proxyCredential = new NetworkCredential(
                        ConfigClass.ParseProxyUsername,
                        ConfigClass.ParseProxyPassword);
                    proxy = new WebProxy(ConfigClass.ParseProxyAddress, false)
                    {
                        UseDefaultCredentials = false,
                        Credentials = proxyCredential,
                    };
                    httpClientHandler = new HttpClientHandler()
                    {
                        Proxy = proxy,
                        PreAuthenticate = true,
                        UseDefaultCredentials = false,
                    };
                    httpClientHandler.Credentials = proxyCredential;
                }
            }
            client = null;
            client = ConfigClass.ParseProxy ? new HttpClient(httpClientHandler) : new HttpClient();
            return client;
        }
    }
}
