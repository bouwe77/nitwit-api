using Dolores.Http;
using Dolores.Requests;
using Dolores.Responses;
using nitwitapi.Jwt;
using System;
using System.IO;
using System.Linq;
using System.Text;

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
                return new Response(HttpStatusCode.Unauthorized);
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

            // Authentication successful, send an OK response containing the JWT token.
            var jwtToken = JwtHandler.CreateJwtToken();
            var response = new Response(HttpStatusCode.Ok)
            {
                MessageBody = new MemoryStream(Encoding.UTF8.GetBytes(jwtToken))
            };

            return response;
        }
    }
}
