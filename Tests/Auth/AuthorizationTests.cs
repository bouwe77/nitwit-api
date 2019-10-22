using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;
using Tests.Users;

namespace Tests.Auth
{
    [TestClass]
    public class AuthorizationTests : TestsBase
    {
        private readonly UserStepDefinitions _u;
        private readonly AuthenticationStepDefinitions _a;
        private readonly AuthorizationStepDefinitions _z;
        private readonly string[] _protectedUrls;

        public AuthorizationTests()
        {
            _u = new UserStepDefinitions();
            _a = new AuthenticationStepDefinitions();
            _z = new AuthorizationStepDefinitions();

            _protectedUrls = new[]
            {
                $"/users/{Constants.Username1}/timeline"
            };
        }

        [TestMethod]
        public async Task UserIsAuthorizedToAccessProtectedUrls_AfterAuthenticatingSuccessfully()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);

            var token = await _a.GIVEN_UserIsAuthenticated(Constants.Username1);

            foreach (var protectedUrl in _protectedUrls)
            {
                // Act
                var response = await _z.WHEN_GettingProtectedUrl(protectedUrl, token);

                // Assert
                THEN_ResponseHasStatusCode(response, HttpStatusCode.OK);
            }
        }

        [TestMethod]
        public async Task UserIsNotAuthorizedToAccessProtectedUrls_WhenNotAuthenticated()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers(Constants.Username1);

            var token = "hacketyhack";

            foreach (var protectedUrl in _protectedUrls)
            {
                // Act
                var response = await _z.WHEN_GettingProtectedUrl(protectedUrl, token);

                // Assert
                THEN_ResponseHasStatusCode(response, HttpStatusCode.Unauthorized);
            }
        }
    }
}
