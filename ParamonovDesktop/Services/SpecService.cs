using ParamonovDesktop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ParamonovDesktop.Services
{
    public class SpecService : BaseHttpService
    {
        /// <summary>ТЗ проекта. null, если ТЗ ещё не создано (404).</summary>
        public async Task<TechnicalSpecification> GetSpecByProject(int projectId)
        {
            using (var request = CreateRequest(HttpMethod.Get,
                BaseUrl + "/api/TechnicalSpecification/project/" + projectId))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK) return null;
                string text = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TechnicalSpecification>(text, JsonOptions);
            }
        }

        /// <summary>Пункты конкретного ТЗ.</summary>
        public async Task<List<SpecItem>> GetItemsBySpec(int specId)
        {
            using (var request = CreateRequest(HttpMethod.Get,
                BaseUrl + "/api/SpecificationItem/by-spec/" + specId))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK) return null;
                string text = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<SpecItem>>(text, JsonOptions);
            }
        }

        public async Task<List<SpecItem>> GetItems(int idSpec)
        {
            using (var request = CreateRequest(HttpMethod.Get,
                BaseUrl + "/api/specificationitem"))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                string text = await response.Content.ReadAsStringAsync();
                var all = JsonSerializer.Deserialize<List<SpecItem>>(text, JsonOptions);
                if (all == null)
                    return new List<SpecItem>();

                var result = new List<SpecItem>();
                foreach (var item in all)
                {
                    if (item.IdSpec == idSpec)
                        result.Add(item);
                }
                return result;
            }
        }

        public async Task<bool> UpdateItem(SpecItem item)
        {
            using (var request = CreateRequest(HttpMethod.Put, BaseUrl + "/api/specificationitem/" + item.IdItem))
            {
                request.Content = ToJson(item);
                using (var response = await Client.SendAsync(request))
                {
                    return response.StatusCode == HttpStatusCode.NoContent ||
                           response.StatusCode == HttpStatusCode.OK;
                }
            }
        }
        public async Task<bool> UpdateSpec(TechnicalSpecification item)
        {
            using (var request = CreateRequest(HttpMethod.Put,
                BaseUrl + "/api/TechnicalSpecification/" + item.IdSpec))
            {
                request.Content = ToJson(item);
                using (var response = await Client.SendAsync(request))
                {
                    return response.StatusCode == HttpStatusCode.NoContent ||
                           response.StatusCode == HttpStatusCode.OK;
                }
            }
        }
        public async Task<SpecItem> CreateItem(SpecItem item)
        {
            using (var request = CreateRequest(HttpMethod.Post, BaseUrl + "/api/specificationitem"))
            {
                request.Content = ToJson(item);
                using (var response = await Client.SendAsync(request))
                {
                    if (response.StatusCode != HttpStatusCode.Created &&
                        response.StatusCode != HttpStatusCode.OK) return null;
                    string text = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<SpecItem>(text, JsonOptions);
                }
            }
        }
        public async Task<bool> DeleteItem(int id)
        {
            using (var request = CreateRequest(HttpMethod.Delete, BaseUrl + "/api/specificationitem/" + id))
            using (var response = await Client.SendAsync(request))
            {
                return response.StatusCode == HttpStatusCode.NoContent ||
                       response.StatusCode == HttpStatusCode.OK;
            }
        }
    }
}
