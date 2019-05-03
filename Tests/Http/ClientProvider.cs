using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using nitwitapi;
using System;
using System.Net.Http;

namespace Tests.Http
{
    internal static class ClientProvider
    {
        private static HttpClient _client;
        private const bool _startupLocalServer = true;

        public static HttpClient HttpClient
        {
            get
            {
                if (_client == null)
                {
                    if (_startupLocalServer)
                    {
                        var server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
                        _client = server.CreateClient();
                    }
                    else
                    {
                        throw new NotImplementedException();
                        //const string remoteServerUrl = "";
                        //_client = new HttpClient()
                    }
                }

                return _client;
            }
        }
    }
}
