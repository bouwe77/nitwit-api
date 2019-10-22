using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;
using Tests.Users;

namespace Tests.Auth
{
    [TestClass]
    public class AuthenticationTests : TestsBase
    {
        private readonly UserStepDefinitions _u;
        private readonly AuthenticationStepDefinitions _a;

        public AuthenticationTests()
        {
            _u = new UserStepDefinitions();
            _a = new AuthenticationStepDefinitions();
        }

        [TestMethod]
        public async Task AuthenticateExistingUser()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);

            // Act
            var response = await _a.WHEN_AuthenticatingUser(Constants.Username1);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.OK);
            THEN_ResponseBodyIsNotEmpty(response);
        }

        [TestMethod]
        public async Task AuthenticateExistingUserWithIncorrectPassword()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);
            var incorrectPassword = Constants.CorrectPassword + "!!!";

            // Act
            var response = await _a.WHEN_AuthenticatingUser(Constants.Username1, incorrectPassword);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Unauthorized);
            THEN_ResponseBodyIsNotEmpty(response);
        }

        [TestMethod]
        public async Task AuthenticateNonExistingUser()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);
            var nonExistingUsername = Constants.Username1 + "aaa";

            // Act
            var response = await _a.WHEN_AuthenticatingUser(nonExistingUsername);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Unauthorized);
            THEN_ResponseBodyIsNotEmpty(response);
        }
    }
}
