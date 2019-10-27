using Dolores.Exceptions;
using Dolores.Http;
using Dolores.Requests;
using nitwitapi.Jwt;
using System;

namespace nitwitapi.Extensions
{
    public static class RequestExtensions
    {
        private static Logger _logger = new Logger();

        public static string CheckAuthorization(this Request request, string expectedUsername = null)
        {
            if (!Auth.Enabled)
            {
                _logger.Log("LET OP auth is NIET enabled!");
                return null;
            }

            if (request == null)
                throw Unauthorized();

            // Check for the Authorization header.
            var authorizationHeader = request.GetHeaderValue(HttpRequestHeaderFields.Authorization);
            if (string.IsNullOrWhiteSpace(authorizationHeader))
                throw Unauthorized();

            // Remove the "Bearer" part from the Authorization token.
            string jwtToken;
            if (authorizationHeader.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase))
                jwtToken = authorizationHeader.Replace("Bearer", string.Empty, StringComparison.OrdinalIgnoreCase);
            else
                jwtToken = authorizationHeader;

            jwtToken = jwtToken.Trim();

            // Check authorization for the given JWT token.
            if (!JwtHandler.IsAuthorized(jwtToken, out string usernameFromToken, expectedUsername))
                throw Unauthorized();

            return usernameFromToken;
        }

        private static Exception Unauthorized(string message = "Unauthorized")
        {
            return new HttpException(message, HttpStatusCode.Unauthorized);
        }
    }
}
