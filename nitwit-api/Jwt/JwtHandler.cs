using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using System;
using System.Collections.Generic;

namespace nitwitapi.Jwt
{
    public class JwtHandler
    {
        private const string _usernameKey = "username";

        public static bool IsAuthorized(string token, string expectedUsername = null)
        {
            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);

                var json = decoder.Decode(token, Secret.Password, verify: true);

                // If a username is expected, check the payload contains that username.
                if (!string.IsNullOrWhiteSpace(expectedUsername))
                {
                    var payload = serializer.Deserialize<Dictionary<string, object>>(json);

                    if (!payload.TryGetValue(_usernameKey, out object usernameObject))
                        return false;

                    var actualUsername = usernameObject.ToString();
                    if (!expectedUsername.Equals(actualUsername, StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                return true;
            }
            catch (TokenExpiredException)
            {
                return false;
            }
            catch (InvalidTokenPartsException)
            {
                return false;
            }
            catch (SignatureVerificationException)
            {
                return false;
            }
        }

        public static string CreateJwtToken(string username)
        {
            var payload = new Dictionary<string, object>
            {
                { "exp", DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds() },
                { _usernameKey, username },
            };

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, Secret.Password);

            return token;
        }
    }
}
