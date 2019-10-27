using Dolores.Http;
using Dolores.Responses;
using nitwitapi.Extensions;
using System.IO;
using System.Text;

namespace nitwitapi.Controllers
{
    internal class HomeController : ControllerBase
    {
        public Response Home()
        {
            var response = new Response(HttpStatusCode.Ok, "text/html")
            {
                MessageBody = new MemoryStream(Encoding.UTF8.GetBytes("<h1>NITWIT API</h1> <a href=\"https://github.com/bouwe77/nitwit-api/blob/master/README.md\">API Documentation</a>"))
            };

            response.AddAccessControlAllowOriginHeader();

            return response;
        }
    }
}
