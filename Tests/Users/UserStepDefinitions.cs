using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tests.Http;

namespace Tests.Users
{
    internal class UserStepDefinitions
    {
        private readonly HttpAsserter _asserter;

        public UserStepDefinitions()
        {
            _asserter = new HttpAsserter();
        }

        public async Task GIVEN_ThereAreTheFollowingUsers(params string[] users)
        {
            // Delete all users.
            await DeleteAllUsers();

            // Create the new users.
            await CreateTheFollowingUsers(users);

            // Assert all created users are there.
            await AssertTheFollowingUsersExist(users);
        }

        public async Task GIVEN_ThereAreNoUsers()
        {
            // Delete all users.
           await DeleteAllUsers();

            // Assert there are no users anymore.
            var response = await _asserter.SendAndAssertGETRequest("/users", HttpStatusCode.OK);
            await AssertResponseContainsNoUsers(response);
        }

        public async Task<HttpResponseMessage> WHEN_AllUsersAreRequested()
        {
            return await _asserter.SendGETRequest("/users");
        }

        public async Task<HttpResponseMessage> WHEN_OneUserIsRequested(string username)
        {
            return await _asserter.SendGETRequest($"/users/{username}");
        }

        public async Task<HttpResponseMessage> WHEN_UserIsCreated(string username)
        {
            var jsonString = "{ \"user\": \"" + username + "\" }";
            return await WHEN_UserIsCreatedWithJsonString(jsonString);
        }

        public async Task<HttpResponseMessage> WHEN_UserIsCreatedWithJsonString(string jsonString)
        {
            var json = new StringContent(jsonString);
            return await _asserter.SendPOSTRequest($"/users", json);
        }

        public async Task<HttpResponseMessage> WHEN_UserIsDeleted(string username)
        {
            return await _asserter.SendDELETERequest($"/users/{username}?pass=z0BnkB7E2ET01qaN");
        }

        public async Task<HttpResponseMessage> WHEN_AllUsersAreDeleted()
        {
            return await _asserter.SendDELETERequest($"/users?pass=z0BnkB7E2ET01qaN");
        }

        public async Task THEN_ResponseContainsTheFollowingUsers(HttpResponseMessage response, params string[] users)
        {
            await AssertResponseContainsTheFollowingUsers(response, users);
        }

        public async Task THEN_ResponseContainsNoUsers(HttpResponseMessage response)
        {
            await AssertResponseContainsNoUsers(response);
        }

        public async Task THEN_TheFollowingUsersExist(params string[] users)
        {
            await AssertTheFollowingUsersExist(users);
        }

        public async Task THEN_NoUsersExist()
        {
            await AssertNoUsersExist();
        }

        private async Task CreateTheFollowingUsers(string[] users)
        {
            foreach (var user in users)
            {
                var json = new StringContent("{ \"user\": \"" + user + "\" }");
                await _asserter.SendAndAssertPOSTRequest("/users", json, HttpStatusCode.Created);
            }
        }

        private async Task AssertTheFollowingUsersExist(string[] users)
        {
            var response = await _asserter.SendAndAssertGETRequest("/users", HttpStatusCode.OK);
            await AssertResponseContainsTheFollowingUsers(response, users);
        }

        private async Task AssertNoUsersExist()
        {
            var response = await _asserter.SendAndAssertGETRequest("/users", HttpStatusCode.OK);
            await AssertResponseContainsNoUsers(response);
        }

        public async Task AssertResponseContainsNoUsers(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("[]", content);
        }

        public async Task AssertResponseContainsTheFollowingUsers(HttpResponseMessage response, string[] users)
        {
            var content = await response.Content.ReadAsStringAsync();
            var stuff = content.Split("},{");
            Assert.AreEqual(users.Count(), stuff.Count());
        }

        private async Task DeleteAllUsers()
        {
            await _asserter.SendAndAssertDELETERequest("/users?pass=z0BnkB7E2ET01qaN", HttpStatusCode.NoContent);
        }
    }
}
