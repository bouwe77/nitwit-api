using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tests.Http;

namespace Tests.Posts
{
    internal class PostStepDefinitions
    {
        private readonly HttpAsserter _asserter;

        public PostStepDefinitions()
        {
            _asserter = new HttpAsserter();
        }

        internal async Task GIVEN_UserHasNoPosts(string username)
        {
            // Delete all posts for the user.
            await DeleteAllPosts(username);

            // Assert the user has no posts anymore.
            var response = await _asserter.SendAndAssertGETRequest($"/users/{username}/posts", HttpStatusCode.OK);
            await AssertResponseContainsNoPosts(response);
        }

        internal async Task GIVEN_UserHasFollowingPosts(string username, params string[] posts)
        {
            // Delete all posts for the user.
            await DeleteAllPosts(username);

            // Create the new posts.
            await CreateTheFollowingPosts(username, posts);

            // Assert all created posts are there.
            await AssertTheFollowingPostsExist(username, posts);
        }

        internal async Task<IEnumerable<string>> GIVEN_UserHasFollowingNumberOfPosts(string username, int numberOfPosts)
        {
            // Delete all posts for the user.
            await DeleteAllPosts(username);

            // Create the new posts.
            var posts = Enumerable.Range(0, numberOfPosts).Select(p => $"This is post {p}").ToArray();
            var postIds = await CreateTheFollowingPosts(username, posts);

            // Assert all created posts are there.
            await AssertTheFollowingPostsExist(username, posts);

            return postIds;
        }

        public async Task<HttpResponseMessage> WHEN_AllPostsAreRequested(string username)
        {
            return await _asserter.SendGETRequest($"/users/{username}/posts");
        }

        public async Task<HttpResponseMessage> WHEN_OnePostIsRequested(string username, string postId)
        {
            return await _asserter.SendGETRequest($"/users/{username}/posts/{postId}");
        }

        internal async Task<HttpResponseMessage> WHEN_PostIsAdded(string username, string post)
        {
            var jsonString = "{ \"content\": \"" + post + "\" }";
            return await WHEN_PostIsAddedWithJsonString(username, jsonString);
        }

        public async Task<HttpResponseMessage> WHEN_PostIsAddedWithJsonString(string username, string jsonString)
        {
            var json = new StringContent(jsonString);
            return await _asserter.SendPOSTRequest($"/users/{username}/posts", json);
        }

        public async Task<HttpResponseMessage> WHEN_AllPostsAreDeleted(string username)
        {
            return await _asserter.SendDELETERequest($"/users/{username}/posts?pass=z0BnkB7E2ET01qaN");
        }

        internal async Task<HttpResponseMessage> WHEN_OnePostIsDeleted(string username, string postId)
        {
            return await _asserter.SendDELETERequest($"/users/{username}/posts/{postId}");
        }

        internal async Task THEN_UserHasFollowingPosts(string username, params string[] posts)
        {
            await AssertUserHasFollowingPosts(username, posts);
        }

        public async Task THEN_ResponseContainsNoPosts(HttpResponseMessage response)
        {
            await AssertResponseContainsNoPosts(response);
        }

        public async Task THEN_ResponseContainsTheFollowingPosts(HttpResponseMessage response, params string[] posts)
        {
            await AssertResponseContainsTheFollowingPosts(response, posts);
        }

        public async Task THEN_ResponseContainsTheFollowingPostIdentifiers(HttpResponseMessage response, params string[] postIdentifiers)
        {
            await AssertResponseContainsTheFollowingPostIdentifers(response, postIdentifiers);
        }

        internal async Task THEN_NoPostsExist(string username)
        {
            await AssertNoPostsExist(username);
        }

        internal async Task THEN_UserHasFollowingPostIdentifiers(string username, params string[] postIdentifiers)
        {
            foreach (var postId in postIdentifiers)
            {
                await _asserter.SendAndAssertGETRequest($"/users/{username}/posts/{postId}", HttpStatusCode.OK);
            }
        }

        internal async Task THEN_UserDoesNotHaveFollowingPostIdentifiers(string username, params string[] postIdentifiers)
        {
            foreach (var postId in postIdentifiers)
            {
                await _asserter.SendAndAssertGETRequest($"/users/{username}/posts/{postId}", HttpStatusCode.NotFound);
            }
        }

        private async Task AssertNoPostsExist(string username)
        {
            var response = await _asserter.SendAndAssertGETRequest($"/users/{username}/posts", HttpStatusCode.OK);
            await AssertResponseContainsNoPosts(response);
        }

        private async Task AssertUserHasFollowingPosts(string username, string[] posts)
        {
            var response = await _asserter.SendAndAssertGETRequest($"/users/{username}/posts", HttpStatusCode.OK);
            await AssertResponseContainsTheFollowingPosts(response, posts);
        }

        public async Task AssertResponseContainsNoPosts(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("[]", content);
        }

        public async Task AssertResponseContainsTheFollowingPosts(HttpResponseMessage response, string[] posts)
        {
            var content = await response.Content.ReadAsStringAsync();
            var stuff = content.Split("},{");
            Assert.AreEqual(posts.Count(), stuff.Count());
        }

        public async Task AssertResponseContainsTheFollowingPostIdentifers(HttpResponseMessage response, string[] postIdentifiers)
        {
            var content = await response.Content.ReadAsStringAsync();

            foreach (var postId in postIdentifiers)
            {
                Assert.IsTrue(content.Contains($"\"id\":\"{postId}\""));
            }
        }

        private async Task DeleteAllPosts(string username)
        {
            await _asserter.SendAndAssertDELETERequest($"/users/{username}/posts?pass=z0BnkB7E2ET01qaN", HttpStatusCode.NoContent);
        }

        private async Task<IEnumerable<string>> CreateTheFollowingPosts(string username, string[] postContents)
        {
            var postIds = new List<string>();
            foreach (var postContent in postContents)
            {
                var json = new StringContent("{ \"content\": \"" + postContent + "\" }");
                var response = await _asserter.SendAndAssertPOSTRequest($"/users/{username}/posts", json, HttpStatusCode.Created);

                var postId = response.GetLocationUri().Split('/').Last();
                postIds.Add(postId);
            }

            return postIds;
        }

        private async Task AssertTheFollowingPostsExist(string username, string[] posts)
        {
            var response = await _asserter.SendAndAssertGETRequest($"/users/{username}/posts", HttpStatusCode.OK);
            await AssertResponseContainsTheFollowingPosts(response, posts);
        }
    }
}
