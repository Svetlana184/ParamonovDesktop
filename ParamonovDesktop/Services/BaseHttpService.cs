using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ParamonovDesktop.Services
{
    public abstract class BaseHttpService
    {
        protected readonly HttpClient Client = new HttpClient();

        protected readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        protected const string BaseUrl = "http://localhost:5192";

        public static string Token { get; set; }

        protected HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            if (!string.IsNullOrEmpty(Token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            }
            return request;
        }

        protected StringContent ToJson<T>(T obj)
        {
            string json = JsonSerializer.Serialize(obj, JsonOptions);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
