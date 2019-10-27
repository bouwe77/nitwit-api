using Dolores.Http;
using Dolores.Responses;

namespace nitwitapi.Extensions
{
    public static class ResponseExtensions
    {
        public static void SetEtagHeader(this Response response, string etag)
        {
            response.SetHeader(HttpResponseHeaderFields.ETag, etag);
            response.SetHeader("Access-Control-Expose-Headers", HttpResponseHeaderFields.ETag);
        }

        public static void AddAccessControlAllowOriginHeader(this Response response)
        {
            //LET OP: Op Azure zie ik deze headers niet, ze lijken te verdwijnen of zo... :-@
            response.SetHeader(HttpResponseHeaderFields.AccessControlAllowOrigin, "*");
            response.SetHeader("Access-Control-Expose-Headers", HttpResponseHeaderFields.AccessControlAllowOrigin);
        }
    }
}
