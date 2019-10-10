using Dolores.Http;
using Dolores.Requests;
using Dolores.Responses;
using System;
using System.Linq;

namespace nitwitapi.Controllers
{
    public class AuthenticationController : ControllerBase
    {
        public Response Authenticate()
        {
            // Deserialize
            Authentication authentication;
            try
            {
                authentication = Request.MessageBody.DeserializeJson<Authentication>();
            }
            catch (Exception)
            {
                var response = new Response(HttpStatusCode.BadRequest);
                response.Json(new { Message = "Request body is not a valid Authentication" });
                return response;
            }

            // Validate
            if (authentication == null || !IsUsernameValid(authentication.Username))
                return new Response(HttpStatusCode.Unauthorized);

            using (var repo = CreateUserRepository())
            {
                var user = repo.Find(u => u.Name.Equals(authentication.Username, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (user == null)
                    return new Response(HttpStatusCode.Unauthorized);

                try
                {
                    var passwordCorrect = BCrypt.Net.BCrypt.Verify(authentication.Password, user.PasswordHash);
                    if (!passwordCorrect) return new Response(HttpStatusCode.Unauthorized);
                }
                catch
                {
                    return new Response(HttpStatusCode.Unauthorized);
                }
            }

            return new Response(HttpStatusCode.Ok);
        }
    }
}
