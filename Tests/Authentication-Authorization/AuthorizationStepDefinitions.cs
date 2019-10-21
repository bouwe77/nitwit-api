using System.Net.Http;
using System.Threading.Tasks;
using Tests.Http;

namespace Tests.Authentication_Authorization
{
    public class AuthorizationStepDefinitions
    {
        private HttpAsserter _asserter;

        public AuthorizationStepDefinitions()
        {
            _asserter = new HttpAsserter();
        }

        public async Task<HttpResponseMessage> WHEN_GettingProtectedUrl(string url, string token)
        {
            return await _asserter.SendGETRequest(url, token);
        }
    }
}
