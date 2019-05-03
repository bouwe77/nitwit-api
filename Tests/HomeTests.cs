using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;
using Tests.Http;

namespace Tests
{
    [TestClass]
    public class HomeTests
    {
        private readonly HttpAsserter _asserter;

        public HomeTests()
        {
            _asserter = new HttpAsserter();
        }

        [TestMethod]
        public async Task OkResponse_WhenHomeRouteRequested()
        {
            var response = await _asserter.SendAndAssertGETRequest("", HttpStatusCode.OK);
        }
    }
}