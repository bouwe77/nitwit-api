using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tests.Http;

namespace Tests.Authentication_Authorization
{
    public class AuthenticationStepDefinitions
    {
        private readonly HttpAsserter _asserter;

        public AuthenticationStepDefinitions()
        {
            _asserter = new HttpAsserter();
        }

        public async Task<HttpResponseMessage> WHEN_AuthenticatingUser(string username, string password = Constants.CorrectPassword)
        {
            return await Authenticate(username, password);
        }

        public async Task<HttpResponseMessage> GIVEN_UserIsAuthenticated(string username)
        {
            var response = await Authenticate(username);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            return response;
        }

        private async Task<HttpResponseMessage> Authenticate(string username, string password = Constants.CorrectPassword)
        {
            var jsonString = "{ \"username\": \"" + username + "\", \"password\": \"" + password + "\" }";
            var json = new StringContent(jsonString);
            return await _asserter.SendPOSTRequest($"/authentication", json);
        }
    }
}
