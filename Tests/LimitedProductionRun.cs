using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tests.Http;

namespace Tests
{
    [TestClass]
    public class LimitedProductionRun
    {
        private readonly HttpAsserter _asserter;

        public LimitedProductionRun()
        {
            _asserter = new HttpAsserter();
        }

        [TestMethod]
        public async Task Crud_User()
        {
            const string user = "bouwe123456789";

            // Delete the user, just in case.
            await _asserter.SendAndAssertDELETERequest($"/users/{user}?pass=z0BnkB7E2ET01qaN", HttpStatusCode.NoContent);

            // Check the user indeed does NOT exist.
            var response = await _asserter.SendAndAssertGETRequest("/users", HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsFalse(content.Contains(user), $"User {user} still exists, which is unexpected");

            // Create the user.
            var json = new StringContent("{ \"user\": \"" + user + "\" }");
            await _asserter.SendAndAssertPOSTRequest("/users", json, HttpStatusCode.Created, $"/users/{user}");

            // Check the user indeed exists now.
            response = await _asserter.SendAndAssertGETRequest("/users", HttpStatusCode.OK);
            content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains(user), $"User does not exist, which is unexpected");

            // Delete the user, to clean up.
            await _asserter.SendAndAssertDELETERequest($"/users/{user}?pass=z0BnkB7E2ET01qaN", HttpStatusCode.NoContent);
        }
    }
}
