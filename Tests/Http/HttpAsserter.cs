using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Tests.Http
{
    internal class HttpAsserter
    {
        private HttpClient HttpClient
        {
            get
            {
                var client = ClientProvider.HttpClient;
                client.DefaultRequestHeaders.Clear();
                return client;
            }
        }

        public async Task<HttpResponseMessage> SendAndAssertGETRequest(string relativeUri, HttpStatusCode expectedStatusCode)
        {
            var response = await SendGETRequest(relativeUri);
            Assert.AreEqual(expectedStatusCode, response.StatusCode);
            return response;
        }

        public async Task<HttpResponseMessage> SendGETRequest(string relativeUri)
        {
            var response = await HttpClient.GetAsync(relativeUri);
            return response;
        }

        public async Task<HttpResponseMessage> SendGETRequest(string relativeUri, string token)
        {
            var client = HttpClient;
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var response = await client.GetAsync(relativeUri);
            return response;
        }

        public async Task<HttpResponseMessage> SendAndAssertPOSTRequest(string relativeUri, HttpContent content, HttpStatusCode expectedStatusCode, string expectedLocationUri = null)
        {
            var response = await SendPOSTRequest(relativeUri, content);

            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            if (expectedLocationUri != null)
            {
                var locationUri = response.GetLocationUri();
                Assert.AreEqual(expectedLocationUri, locationUri);
            }

            return response;
        }

        public async Task<HttpResponseMessage> SendPOSTRequest(string relativeUri, HttpContent content)
        {
            var response = await HttpClient.PostAsync(relativeUri, content);
            return response;
        }

        public async Task<HttpResponseMessage> SendAndAssertPUTRequest(string relativeUri, HttpContent content, HttpStatusCode expectedStatusCode)
        {
            var response = await SendPUTRequest(relativeUri, content);
            Assert.AreEqual(expectedStatusCode, response.StatusCode);
            return response;
        }

        public async Task<HttpResponseMessage> SendPUTRequest(string relativeUri, HttpContent content)
        {
            var response = await HttpClient.PutAsync(relativeUri, content);
            return response;
        }

        public async Task<HttpResponseMessage> SendAndAssertDELETERequest(string relativeUri, HttpStatusCode expectedStatusCode)
        {
            var response = await SendDELETERequest(relativeUri);
            Assert.AreEqual(expectedStatusCode, response.StatusCode);
            return response;
        }

        public async Task<HttpResponseMessage> SendDELETERequest(string relativeUri)
        {
            var response = await HttpClient.DeleteAsync(relativeUri);
            return response;
        }
    }
}
