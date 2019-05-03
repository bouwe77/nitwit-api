using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Tests
{
    public abstract class TestsBase
    {
        protected void THEN_ResponseHasStatusCode(HttpResponseMessage response, HttpStatusCode expectedStatusCode)
        {
            Assert.AreEqual(expectedStatusCode, response.StatusCode);
        }

        protected void THEN_ResponseHasLocationHeader(HttpResponseMessage response, string expectedLocationUri)
        {
            if (response.Headers.TryGetValues("Location", out var values))
            {
                string locationUri = values.FirstOrDefault();
                Assert.AreEqual(expectedLocationUri, locationUri);
            }
            else
            {
                Assert.Fail("Location header not found");
            }
        }
    }
}
