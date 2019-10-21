using System;
using Dolores.Exceptions;
using Dolores.Http;
using Dolores.Requests;
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
    }
}
