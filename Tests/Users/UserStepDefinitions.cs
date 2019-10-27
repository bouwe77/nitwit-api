using Microsoft.VisualStudio.TestTools.UnitTesting;
using nitwitapi;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tests.Http;

namespace Tests.Users
{
    internal class UserStepDefinitions
    {
        private readonly HttpRequestHandler _httpRequestHandler;

        public UserStepDefinitions()
            : this(new HttpRequestHandler())
        {
        }

        public UserStepDefinitions(HttpRequestHandler httpRequestHandler)
        {
            _httpRequestHandler = httpRequestHandler;
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
            var response = await _httpRequestHandler.SendAndAssertGETRequest($"/users?pass={Secret.Password}", HttpStatusCode.OK);
            await AssertResponseContainsNoUsers(response);
        }

        public async Task<HttpResponseMessage> WHEN_AllUsersAreRequested()
        {
            return await _httpRequestHandler.SendGETRequest($"/users?pass={Secret.Password}");
        }

        public async Task<HttpResponseMessage> WHEN_OneUserIsRequested(string username)
        {
            return await _httpRequestHandler.SendGETRequest($"/users/{username}");
        }

        public async Task<HttpResponseMessage> WHEN_WhoAmIIsRequested()
        {
            return await _httpRequestHandler.SendGETRequest($"/users/whoami");
        }

        public async Task<HttpResponseMessage> WHEN_UserIsCreated(string username, string password)
        {
            var jsonString = "{ \"username\": \"" + username + "\", \"password\": \"" + password + "\" }";
            return await WHEN_UserIsCreatedWithJsonString(jsonString);
        }

        public async Task<HttpResponseMessage> WHEN_UserIsCreatedWithJsonString(string jsonString)
        {
            var json = new StringContent(jsonString);
            return await _httpRequestHandler.SendPOSTRequest($"/users?pass={Secret.Password}", json);
        }

        public async Task<HttpResponseMessage> WHEN_UserIsDeleted(string username)
        {
            return await _httpRequestHandler.SendDELETERequest($"/users/{username}?pass={Secret.Password}");
        }

        public async Task<HttpResponseMessage> WHEN_AllUsersAreDeleted()
        {
            return await _httpRequestHandler.SendDELETERequest($"/users?pass={Secret.Password}");
        }

        public async Task THEN_ResponseContainsTheFollowingUsers(HttpResponseMessage response, params string[] usernames)
        {
            await AssertResponseContainsTheFollowingUsers(response, usernames);
        }

        public async Task THEN_ResponseContainsNoUsers(HttpResponseMessage response)
        {
            await AssertResponseContainsNoUsers(response);
        }

        public async Task THEN_TheFollowingUsersExist(params string[] usernames)
        {
            await AssertTheFollowingUsersExist(usernames);
        }

        public async Task THEN_NoUsersExist()
        {
            await AssertNoUsersExist();
        }

        private async Task CreateTheFollowingUsers(string[] usernames)
        {
            foreach (var username in usernames)
            {
                var json = new StringContent("{ \"username\": \"" + username + "\", \"password\": \"" + Constants.CorrectPassword + "\" }");
                await _httpRequestHandler.SendAndAssertPOSTRequest($"/users?pass={Secret.Password}", json, HttpStatusCode.Created);
            }
        }

        private async Task AssertTheFollowingUsersExist(string[] usernames)
        {
            var response = await _httpRequestHandler.SendAndAssertGETRequest($"/users?pass={Secret.Password}", HttpStatusCode.OK);
            await AssertResponseContainsTheFollowingUsers(response, usernames);
        }

        private async Task AssertNoUsersExist()
        {
            var response = await _httpRequestHandler.SendAndAssertGETRequest($"/users?pass={Secret.Password}", HttpStatusCode.OK);
            await AssertResponseContainsNoUsers(response);
        }

        public async Task AssertResponseContainsNoUsers(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("[]", content);
        }

        public async Task AssertResponseContainsTheFollowingUsers(HttpResponseMessage response, string[] usernames)
        {
            var content = await response.Content.ReadAsStringAsync();
            var stuff = content.Split("},{");
            Assert.AreEqual(usernames.Count(), stuff.Count());
        }

        private async Task DeleteAllUsers()
        {
            await _httpRequestHandler.SendAndAssertDELETERequest($"/users?pass={Secret.Password}", HttpStatusCode.NoContent);
        }
    }
}
