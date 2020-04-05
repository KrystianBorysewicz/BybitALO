using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace BybitAloConsole
{
    public static class HttpRequestMessageExtensions
    {
        public static async Task Authenticate(this HttpRequestMessage httpRequestMessage,
            string apiKey, string apiSecret)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(httpRequestMessage.RequestUri.Query))
            {
                NameValueCollection currentQueryParameters =
                    HttpUtility.ParseQueryString(httpRequestMessage.RequestUri.Query);
                queryParams =
                    currentQueryParameters.AllKeys.ToDictionary(paramKey => paramKey,
                        paramKey => currentQueryParameters[paramKey]);
            }

            if (httpRequestMessage.Content != null)
            {
                var contentJson = JObject.Parse(await httpRequestMessage.Content.ReadAsStringAsync());
                foreach (var (key, value) in contentJson)
                {
                    queryParams.Add(key, value.ToString());
                }
            }

            long expires = AuthenticationService.CurrentTimeMillis() - 1000;
            queryParams.Add("api_key", apiKey);
            queryParams.Add("timestamp", expires.ToString());
            queryParams.Add("recv_window", 5000.ToString());

            string paramsUrlEncoded =
                string.Join("&", queryParams.OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"));

            paramsUrlEncoded = string.Join("&", paramsUrlEncoded,
                $"sign={AuthenticationService.CreateSignature(apiSecret, paramsUrlEncoded)}");

            httpRequestMessage.RequestUri =
                new Uri($"{httpRequestMessage.RequestUri.GetLeftPart(UriPartial.Path)}?{paramsUrlEncoded}");
        }
    }
}