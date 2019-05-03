using Dolores.Http;
using Dolores.Responses;
using System.IO;
using System.Text;

namespace nitwitapi.Controllers
{
    internal class HomeController : ControllerBase
    {
        public Response Home()
        {
            var response = new Response(HttpStatusCode.Ok, "text/plain")
            {
                MessageBody = new MemoryStream(Encoding.UTF8.GetBytes("NITWIT API"))
            };

            return response;
        }
    }
}
