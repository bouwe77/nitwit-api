using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tests.Http
{
    internal class HttpAsserter
    {
        private readonly HttpClient _client;

        public HttpAsserter()
        {
            _client = ClientProvider.HttpClient;
        }

        public async Task<HttpResponseMessage> SendAndAssertGETRequest(string relativeUri, HttpStatusCode expectedStatusCode)
        {
            var response = await SendGETRequest(relativeUri);
            Assert.AreEqual(expectedStatusCode, response.StatusCode);
            return response;
        }

        public async Task<HttpResponseMessage> SendGETRequest(string relativeUri)
        {
            var response = await _client.GetAsync(relativeUri);
            return response;
        }

        public async Task<HttpResponseMessage> SendAndAssertPOSTRequest(string relativeUri, HttpContent content, HttpStatusCode expectedStatusCode, string expectedLocationUri = null)
        {
            var response = await SendPOSTRequest(relativeUri, content);

            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            if (expectedLocationUri != null)
            {
                var locationUri = GetLocationUri(response);
                Assert.AreEqual(expectedLocationUri, locationUri);
            }

            return response;
        }

        public string GetLocationUri(HttpResponseMessage response)
        {
            string locationUri = null;
            if (response.Headers.TryGetValues("Location", out var values))
            {
                locationUri = values.FirstOrDefault();
            }

            return locationUri;
        }

        public async Task<HttpResponseMessage> SendPOSTRequest(string relativeUri, HttpContent content)
        {
            var response = await _client.PostAsync(relativeUri, content);
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
            var response = await _client.PutAsync(relativeUri, content);
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
            var response = await _client.DeleteAsync(relativeUri);
            return response;
        }
    }
}
