using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tests.Http;

namespace Tests.Auth
{
    public class AuthenticationStepDefinitions
    {
        private readonly HttpRequestHandler _httpRequestHandler;

        public AuthenticationStepDefinitions()
        {
            _httpRequestHandler = new HttpRequestHandler();
        }

        public async Task<HttpResponseMessage> WHEN_AuthenticatingUser(string username, string password = Constants.CorrectPassword)
        {
            return await Authenticate(username, password);
        }

        public async Task<string> GIVEN_UserIsAuthenticated(string username)
        {
            var response = await Authenticate(username);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var token = await response.GetResponseBody();

            return token;
        }

        private async Task<HttpResponseMessage> Authenticate(string username, string password = Constants.CorrectPassword)
        {
            var jsonString = "{ \"username\": \"" + username + "\", \"password\": \"" + password + "\" }";
            var json = new StringContent(jsonString);
            return await _httpRequestHandler.SendPOSTRequest($"/authentication", json);
        }
    }
}
