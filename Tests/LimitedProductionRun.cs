using Microsoft.VisualStudio.TestTools.UnitTesting;
using nitwitapi;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tests.Http;

namespace Tests
{
    [TestClass]
    public class LimitedProductionRun
    {
        private readonly HttpRequestHandler _httpRequestHandler;

        public LimitedProductionRun()
        {
            _httpRequestHandler = new HttpRequestHandler();
        }

        [TestMethod]
        public async Task Crud_User()
        {
            const string username = "bouwe123456789";

            // Delete the user, just in case.
            await _httpRequestHandler.SendAndAssertDELETERequest($"/users/{username}?pass={Secret.Password}", HttpStatusCode.NoContent);

            // Check the user indeed does NOT exist.
            var response = await _httpRequestHandler.SendAndAssertGETRequest($"/users?pass={Secret.Password}", HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsFalse(content.Contains(username), $"User {username} still exists, which is unexpected");

            // Create the user.
            var json = new StringContent("{ \"username\": \"" + username + "\", \"password\": \"" + Constants.CorrectPassword + "\" }");
            await _httpRequestHandler.SendAndAssertPOSTRequest($"/users?pass={Secret.Password}", json, HttpStatusCode.Created, $"/users/{username}");

            // Check the user indeed exists now.
            response = await _httpRequestHandler.SendAndAssertGETRequest($"/users?pass={Secret.Password}", HttpStatusCode.OK);
            content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains(username), $"User does not exist, which is unexpected");

            // Delete the user, to clean up.
            await _httpRequestHandler.SendAndAssertDELETERequest($"/users/{username}?pass={Secret.Password}", HttpStatusCode.NoContent);
        }
    }
}
