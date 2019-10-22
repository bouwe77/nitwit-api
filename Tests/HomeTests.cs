using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;
using Tests.Http;

namespace Tests
{
    [TestClass]
    public class HomeTests
    {
        private readonly HttpRequestHandler _httpRequestHandler;

        public HomeTests()
        {
            _httpRequestHandler = new HttpRequestHandler();
        }

        [TestMethod]
        public async Task OkResponse_WhenHomeRouteRequested()
        {
            var response = await _httpRequestHandler.SendAndAssertGETRequest("", HttpStatusCode.OK);
        }
    }
}