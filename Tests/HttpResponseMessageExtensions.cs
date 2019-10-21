using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tests
{
    public static class HttpResponseMessageExtensions
    {
        public static string GetLocationUri(this HttpResponseMessage response)
        {
            string locationUri = null;
            if (response.Headers.TryGetValues("Location", out var values))
            {
                locationUri = values.FirstOrDefault();
            }

            return locationUri;
        }

        public static async Task<string> GetResponseBody(this HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();
            return body;
        }
    }
}
