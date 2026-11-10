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
    public class ChangeService : BaseHttpService
    {
        public async Task<List<ChangeRequest>> GetAll()
        {
            using (var request = CreateRequest(HttpMethod.Get,
                BaseUrl + "/api/changerequest"))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                string text = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ChangeRequest>>(text, JsonOptions);
            }
        }

        public async Task Create(ChangeRequest item)
        {
            using (var request = CreateRequest(HttpMethod.Post,
                BaseUrl + "/api/changerequest"))
            {
                request.Content = ToJson(item);
                using (var response = await Client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public async Task Update(ChangeRequest item)
        {
            using (var request = CreateRequest(HttpMethod.Put,
                BaseUrl + "/api/changerequest/" + item.IdChange))
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
