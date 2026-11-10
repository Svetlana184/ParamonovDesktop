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

        public async Task UpdateItem(SpecItem item)
        {
            using (var request = CreateRequest(HttpMethod.Put,
                BaseUrl + "/api/specificationitem/" + item.IdItem))
            {
                request.Content = ToJson(item);
                using (var response = await Client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}
