using Microsoft.VisualStudio.TestTools.UnitTesting;
using nitwitapi;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Tests.Users;

namespace Tests.Authentication_Authorization
{
    [TestClass]
    public class AuthorizationTests : TestsBase
    {
        private readonly UserStepDefinitions _u;
        private readonly AuthenticationStepDefinitions _a;
        private readonly AuthorizationStepDefinitions _z;
        private string[] _protectedUrls;

        public AuthorizationTests()
        {
            _u = new UserStepDefinitions();
            _a = new AuthenticationStepDefinitions();
            _z = new AuthorizationStepDefinitions();

            _protectedUrls = new[]
            {
                "/users/john/timeline"
            };
        }

        [TestMethod]
        public async Task UserIsAuthorizedToAccessProtectedUrls_AfterAuthenticatingSuccessfully()
        {
            if (!Auth.Enabled)
                return;

            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers("john");
            var authResponse = await _a.GIVEN_UserIsAuthenticated("john");
            var token = await authResponse.GetResponseBody();

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
            if (!Auth.Enabled)
                return;

            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers("john");
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
