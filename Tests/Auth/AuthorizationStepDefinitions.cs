using System.Net.Http;
using System.Threading.Tasks;
using Tests.Http;

namespace Tests.Auth
{
    public class AuthorizationStepDefinitions
    {
        public async Task<HttpResponseMessage> WHEN_GettingProtectedUrl(string url, string token)
        {
            var httpRequestHandler = new HttpRequestHandler(token);
            return await httpRequestHandler.SendGETRequest(url);
        }
    }
}
