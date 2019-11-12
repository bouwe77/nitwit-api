using Data.Entities;
using Dolores.Http;
using Dolores.Requests;
using Dolores.Responses;
using nitwitapi.Extensions;
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
                var r1 = GetUnauthorizedResponse("Error deserializing JSON");
                r1.AddAccessControlAllowOriginHeader();
                return r1;
            }

            // Validate
            if (authentication == null || !IsUsernameValid(authentication.Username))
            {
                var r2 = GetUnauthorizedResponse("Invalid username");
                r2.AddAccessControlAllowOriginHeader();
                return r2;
            }

            User user;
            using (var repo = CreateUserRepository())
            {
                user = repo.Find(u => u.Name.Equals(authentication.Username, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (user == null)
                {
                    var r3 = GetUnauthorizedResponse("User not found");
                    r3.AddAccessControlAllowOriginHeader();
                    return r3;
                }

                try
                {
                    var passwordCorrect = BCrypt.Net.BCrypt.Verify(authentication.Password, user.PasswordHash);
                    if (!passwordCorrect)
                    {
                        var r4 = GetUnauthorizedResponse();
                        r4.AddAccessControlAllowOriginHeader();
                        return r4;
                    }
                }
                catch
                {
                    var r5 = GetUnauthorizedResponse();
                    r5.AddAccessControlAllowOriginHeader();
                    return r5;
                }
            }

            // Authentication successful, send an OK response containing the JWT token.
            var jwtToken = JwtHandler.CreateJwtToken(user.Name);
            var response = new Response(HttpStatusCode.Ok)
            {
                MessageBody = new MemoryStream(Encoding.UTF8.GetBytes(jwtToken))
            };

            response.AddAccessControlAllowOriginHeader();

            return response;
        }
    }
}
