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
    public class TaskService : BaseHttpService
    {
        public async Task<List<TaskItem>> GetAll()
        {
            using (var request = CreateRequest(HttpMethod.Get, BaseUrl + "/api/task"))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                string text = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<TaskItem>>(text, JsonOptions);
            }
        }

        public async Task Update(TaskItem item)
        {
            using (var request = CreateRequest(HttpMethod.Put,
                BaseUrl + "/api/task/" + item.IdTask))
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
