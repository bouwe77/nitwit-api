using Microsoft.VisualStudio.TestTools.UnitTesting;
using nitwitapi.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tests.Http;
using Tests.Users;

namespace Tests.Posts
{
    [TestClass]
    public class PostTests : TestsBase
    {
        private readonly PostStepDefinitions _p;
        private readonly UserStepDefinitions _u;

        public PostTests()
        {
            _p = new PostStepDefinitions();
            _u = new UserStepDefinitions();
        }

        [TestMethod]
        public async Task AddPost_CreatesPost()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);
            await _p.GIVEN_UserHasNoPosts(Constants.Username1);

            // Act
            var response = await _p.WHEN_PostIsAdded(Constants.Username1, "Lorem ipsum");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Created);
            await _p.THEN_UserHasFollowingPosts(Constants.Username1, "Lorem ipsum");
        }

        [TestMethod]
        public async Task AddPost_CreatesPost_UsernameDifferentCasing()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1.ToLower());
            await _p.GIVEN_UserHasNoPosts(Constants.Username1.ToLower());

            // Act
            var response = await _p.WHEN_PostIsAdded(Constants.Username1.ToUpper(), "Lorem ipsum");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Created);
            await _p.THEN_UserHasFollowingPosts(Constants.Username1, "Lorem ipsum");
        }

        [TestMethod]
        public async Task AddPost_CreatesPost_WhenContentHasMaxLength()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);
            await _p.GIVEN_UserHasNoPosts(Constants.Username1);
            var veryLongPost = new string('a', 240);

            // Act
            var response = await _p.WHEN_PostIsAdded(Constants.Username1, veryLongPost);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Created);
            await _p.THEN_UserHasFollowingPosts(Constants.Username1, veryLongPost);
        }

        [TestMethod]
        public async Task AddPost_ReturnsUnauthorized_WhenUsernameTooLong()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var veryLongUsername = new string('a', 101);

            // Act
            var response = await _p.WHEN_PostIsAdded(veryLongUsername, "Lorem ipsum");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task AddPost_ReturnsUnauthorized_WhenUsernameHasInvalidCharacters()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _p.WHEN_PostIsAdded("*5)%", "Lorem ipsum");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task AddPost_ReturnsBadRequest_WhenContentMissing()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var jsonString = "{ }";

            // Act
            var response = await _p.WHEN_PostIsAddedWithJsonString(Constants.Username1, jsonString);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task AddPost_ReturnsBadRequest_WhenPostIsNotAPost()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var jsonString = "{ \"this is\": \"no a post\" }";

            // Act
            var response = await _p.WHEN_PostIsAddedWithJsonString(Constants.Username1, jsonString);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task AddPost_ReturnsBadRequest_WhenContentEmpty()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var jsonString = "{ \"post\": \"\" }";

            // Act
            var response = await _p.WHEN_PostIsAddedWithJsonString(Constants.Username1, jsonString);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task AddPost_ReturnsBadRequest_WhenContentTooLong()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var veryLongPost = new string('a', 241);

            // Act
            var response = await _p.WHEN_PostIsAdded(Constants.Username1, veryLongPost);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task AddPost_ReturnsNotFound_WhenUsernameDoesNotExist()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _p.WHEN_PostIsAdded(Constants.Username1, "Lorem ipsum");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task GetAllPosts_ReturnsBadRequest_WhenUsernameTooLong()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var veryLongUsername = new string('a', 101);

            // Act
            var response = await _p.WHEN_AllPostsAreRequested(veryLongUsername);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task GetAllPosts_ReturnsBadRequest_WhenUsernameHasInvalidcharacters()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _p.WHEN_AllPostsAreRequested("!@y*");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task GetAllPosts_ReturnsNotFound_WhenUsernameDoesNotExist()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _p.WHEN_AllPostsAreRequested(Constants.Username1);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task GetAllPosts_ReturnsEmptyList_WhenUserHasNoPosts()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);
            await _p.GIVEN_UserHasNoPosts(Constants.Username1);

            // Act
            var response = await _p.WHEN_AllPostsAreRequested(Constants.Username1);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.OK);
            await _p.THEN_ResponseContainsNoPosts(response);
        }

        [TestMethod]
        public async Task GetAllPosts_ReturnsOnePost_WhenUserHasOnePost()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);
            await _p.GIVEN_UserHasFollowingPosts(Constants.Username1, "Lorem ipsum");

            // Act
            var response = await _p.WHEN_AllPostsAreRequested(Constants.Username1);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.OK);
            await _p.THEN_ResponseContainsTheFollowingPosts(response, "Lorem ipsum");
        }

        [TestMethod]
        public async Task GetAllPosts_ReturnsAllPosts_WhenUserHasMultiplePosts()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);
            await _p.GIVEN_UserHasFollowingPosts(Constants.Username1, "Lorem ipsum", "Hello world");

            // Act
            var response = await _p.WHEN_AllPostsAreRequested(Constants.Username1);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.OK);
            await _p.THEN_ResponseContainsTheFollowingPosts(response, "Lorem ipsum", "Hello world");
        }

        [TestMethod]
        public async Task GetOnePost_ReturnsBadRequest_WhenUsernameTooLong()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var veryLongUsername = new string('a', 101);

            // Act
            var response = await _p.WHEN_OnePostIsRequested(veryLongUsername, "post1");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task GetOnePost_ReturnsBadRequest_WhenUsernameHasInvalidCharacters()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _p.WHEN_OnePostIsRequested("!@y*", "post1");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task GetOnePost_ReturnsBadRequest_WhenPostIdTooLong()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var veryLongPostId = new string('a', 10);

            // Act
            var response = await _p.WHEN_OnePostIsRequested(Constants.Username1, veryLongPostId);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task GetOnePost_ReturnsNotFound_WhenUsernameDoesNotExist()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _p.WHEN_OnePostIsRequested(Constants.Username1, "post1");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task GetOnePost_ReturnsNotFound_WhenUserHasNoPosts()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);
            await _p.GIVEN_UserHasNoPosts(Constants.Username1);

            // Act
            var response = await _p.WHEN_OnePostIsRequested(Constants.Username1, "post1");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task GetOnePost_ReturnsNotFound_WhenPostDoesNotBelongToUser()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1, Constants.Username2);
            var user1PostIds = await _p.GIVEN_UserHasFollowingNumberOfPosts(Constants.Username1, 1);

            var token = JwtHandler.CreateJwtToken(Constants.Username2);
            var p2 = new PostStepDefinitions(new HttpRequestHandler(token));
            var user2PostIds = await p2.GIVEN_UserHasFollowingNumberOfPosts(Constants.Username2, 1);

            // Act
            var response = await _p.WHEN_OnePostIsRequested(Constants.Username1, user2PostIds.Single());

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task GetOnePost_ReturnsPost()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);
            var postIds = await _p.GIVEN_UserHasFollowingNumberOfPosts(Constants.Username1, 1);

            // Act
            var response = await _p.WHEN_OnePostIsRequested(Constants.Username1, postIds.Single());

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.OK);
            await _p.THEN_ResponseContainsTheFollowingPostIdentifiers(response, postIds.Single());
        }

        [TestMethod]
        public async Task GetOnePost_ReturnsPost_WhenUsernameHasDifferentCasing()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1.ToLower());
            var postIds = await _p.GIVEN_UserHasFollowingNumberOfPosts(Constants.Username1.ToLower(), 1);

            // Act
            var response = await _p.WHEN_OnePostIsRequested(Constants.Username1.ToUpper(), postIds.Single());

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.OK);
            await _p.THEN_ResponseContainsTheFollowingPostIdentifiers(response, postIds.Single());
        }

        [TestMethod]
        public async Task GetOnePost_ReturnsPost_WhenUsernameHasMaxLength()
        {
            // Arrange
            var veryLongUsername = new string('a', 100);
            await _u.GIVEN_ThereAreTheFollowingUsers(veryLongUsername);

            var token = JwtHandler.CreateJwtToken(veryLongUsername);
            var p = new PostStepDefinitions(new HttpRequestHandler(token));
            var postIds = await p.GIVEN_UserHasFollowingNumberOfPosts(veryLongUsername, 1);

            // Act
            var response = await p.WHEN_OnePostIsRequested(veryLongUsername, postIds.Single());

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.OK);
            await p.THEN_ResponseContainsTheFollowingPostIdentifiers(response, postIds.Single());
        }

        [TestMethod]
        public async Task GetOnePost_ReturnsPost_WhenPostIdentiferHasDifferentCasing()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);
            var postIds = await _p.GIVEN_UserHasFollowingNumberOfPosts(Constants.Username1, 1);
            var postIdWithDifferentCasing = postIds.Single().ToUpper();
            if (postIdWithDifferentCasing == postIds.Single())
                postIdWithDifferentCasing = postIds.Single().ToLower();

            // Act
            var response = await _p.WHEN_OnePostIsRequested(Constants.Username1, postIdWithDifferentCasing);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.OK);
            await _p.THEN_ResponseContainsTheFollowingPostIdentifiers(response, postIds.Single());
        }

        [TestMethod]
        public async Task DeleteAllPosts_ReturnsUnauthorized_WhenUsernameHasInvalidCharacters()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _p.WHEN_AllPostsAreDeleted("(*7%");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task DeleteAllPosts_ReturnsUnauthorized_WhenUsernameIsTooLong()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var veryLongUsername = new string('a', 101);

            // Act
            var response = await _p.WHEN_AllPostsAreDeleted(veryLongUsername);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task DeleteAllPosts_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _p.WHEN_AllPostsAreDeleted(Constants.Username1);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task DeleteAllPosts_ReturnsNoContent_WhenNoPostsExist()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);

            // Act
            var response = await _p.WHEN_AllPostsAreDeleted(Constants.Username1);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await _p.THEN_NoPostsExist(Constants.Username1);
        }

        [TestMethod]
        public async Task DeleteAllPosts_DeletesAllPosts_WhenPostsExist()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);
            await _p.GIVEN_UserHasFollowingPosts(Constants.Username1, "Lorem ipsum", "Hello world");

            // Act
            var response = await _p.WHEN_AllPostsAreDeleted(Constants.Username1);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await _p.THEN_NoPostsExist(Constants.Username1);
        }

        [TestMethod]
        public async Task DeleteAllPosts_DeletesAllPosts_WhenUsernameHasDifferentCasing()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1.ToLower());
            await _p.GIVEN_UserHasFollowingPosts(Constants.Username1.ToLower(), "Lorem ipsum", "Hello world");

            // Act
            var response = await _p.WHEN_AllPostsAreDeleted(Constants.Username1.ToUpper());

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await _p.THEN_NoPostsExist(Constants.Username1);
        }

        [TestMethod]
        public async Task DeleteAllPosts_DeletesAllPosts_WhenUsernameHasMaxLength()
        {
            // Arrange
            var veryLongUsername = new string('a', 100);
            var token = JwtHandler.CreateJwtToken(veryLongUsername);
            var p = new PostStepDefinitions(new HttpRequestHandler(token));

            await _u.GIVEN_ThereAreTheFollowingUsers(veryLongUsername);
            await p.GIVEN_UserHasFollowingPosts(veryLongUsername, "Lorem ipsum", "Hello world");

            // Act
            var response = await p.WHEN_AllPostsAreDeleted(veryLongUsername);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await p.THEN_NoPostsExist(veryLongUsername);
        }

        [TestMethod]
        public async Task DeletePost_ReturnsUnauthorized_WhenUsernameHasInvalidCharacters()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _p.WHEN_OnePostIsDeleted("(*7%", "post1");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task DeletePost_ReturnsUnauthorized_WhenUsernameIsTooLong()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var veryLongUsername = new string('a', 101);

            // Act
            var response = await _p.WHEN_OnePostIsDeleted(veryLongUsername, "post1");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task DeletePost_ReturnsBadRequest_WhenPostIdentifierIsTooLong()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var veryLongPostId = new string('a', 11);

            // Act
            var response = await _p.WHEN_OnePostIsDeleted(Constants.Username1, veryLongPostId);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task DeletePost_ReturnsMethodNotAllowed_WhenPostIdentifierIsEmpty()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _p.WHEN_OnePostIsDeleted(Constants.Username1, string.Empty);

            // Assert
            // An empty post identifier in the URL means: delete all posts. However, this is not allowed if you don't provide the "password".
            THEN_ResponseHasStatusCode(response, HttpStatusCode.MethodNotAllowed);
        }

        [TestMethod]
        public async Task DeletePost_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _p.WHEN_OnePostIsDeleted(Constants.Username1, "post1");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task DeletePost_ReturnsNoContent_WhenPostDoesNotExist()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);

            // Act
            var response = await _p.WHEN_OnePostIsDeleted(Constants.Username1, "post1");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await _p.THEN_NoPostsExist(Constants.Username1);
        }

        [TestMethod]
        public async Task DeletePost_DeletesNothing_WhenPostIdentifierBelongsToOtherUser()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1, Constants.Username2);

            var user1PostIds = await _p.GIVEN_UserHasFollowingNumberOfPosts(Constants.Username1, 1);

            var token2 = JwtHandler.CreateJwtToken(Constants.Username2);
            var p2 = new PostStepDefinitions(new HttpRequestHandler(token2));
            var user2PostIds = await p2.GIVEN_UserHasFollowingNumberOfPosts(Constants.Username2, 1);

            // Act
            var response = await _p.WHEN_OnePostIsDeleted(Constants.Username1, user2PostIds.Single());

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await _p.THEN_UserHasFollowingPostIdentifiers(Constants.Username1, user1PostIds.Single());
            await _p.THEN_UserHasFollowingPostIdentifiers(Constants.Username2, user2PostIds.Single());
        }

        [TestMethod]
        public async Task DeletePost_OnePostLeft_WhenOnePostOfMultipleIsDeleted()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);
            var postIds = await _p.GIVEN_UserHasFollowingNumberOfPosts(Constants.Username1, 2);

            // Act
            var response = await _p.WHEN_OnePostIsDeleted(Constants.Username1, postIds.First());

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await _p.THEN_UserDoesNotHaveFollowingPostIdentifiers(Constants.Username1, postIds.First());
            await _p.THEN_UserHasFollowingPostIdentifiers(Constants.Username1, postIds.Last());
        }

        [TestMethod]
        public async Task DeletePost_DeletesPost_WhenUsernameHasDifferentCasing()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1.ToLower());
            var postIds = await _p.GIVEN_UserHasFollowingNumberOfPosts(Constants.Username1.ToLower(), 1);

            // Act
            var response = await _p.WHEN_OnePostIsDeleted(Constants.Username1.ToUpper(), postIds.Single());

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await _p.THEN_NoPostsExist(Constants.Username1);
        }

        [TestMethod]
        public async Task DeletePost_DeletesAllPosts_WhenUsernameHasMaxLength()
        {
            // Arrange
            var veryLongUsername = new string('a', 100);
            var token = JwtHandler.CreateJwtToken(veryLongUsername);
            var p = new PostStepDefinitions(new HttpRequestHandler(token));
            await _u.GIVEN_ThereAreTheFollowingUsers(veryLongUsername);
            var postIds = await p.GIVEN_UserHasFollowingNumberOfPosts(veryLongUsername, 1);

            // Act
            var response = await p.WHEN_OnePostIsDeleted(veryLongUsername, postIds.Single());

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await p.THEN_NoPostsExist(veryLongUsername);
        }


        [TestMethod]
        public async Task DeletePost_DeletesPost_WhenPostIdentifierHasDifferentCasing()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);
            var postIds = await _p.GIVEN_UserHasFollowingNumberOfPosts(Constants.Username1, 1);
            var postIdWithDifferentCasing = postIds.Single().ToUpper();
            if (postIdWithDifferentCasing == postIds.Single())
                postIdWithDifferentCasing = postIds.Single().ToLower();

            // Act
            var response = await _p.WHEN_OnePostIsDeleted(Constants.Username1, postIdWithDifferentCasing);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await _p.THEN_NoPostsExist(Constants.Username1);
        }
    }
}
