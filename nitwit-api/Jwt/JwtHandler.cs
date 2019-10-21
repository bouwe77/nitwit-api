using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using System;
using System.Collections.Generic;

namespace nitwitapi.Jwt
{
    public class JwtHandler
    {
        public static bool IsAuthorized(string token)
        {
            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);

                var json = decoder.Decode(token, Secret.Password, verify: true);

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

        public static string CreateJwtToken()
        {
            var payload = new Dictionary<string, object>
            {
                { "exp", DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds() },
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
