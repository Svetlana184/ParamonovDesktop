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
    public class ProjectService : BaseHttpService
    {
        public async Task<List<Project>> GetProjects()
        {
            using (var request = CreateRequest(HttpMethod.Get, BaseUrl + "/api/project"))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                string text = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Project>>(text, JsonOptions);
            }
        }

        public async Task<Project> GetProject(int id)
        {
            using (var request = CreateRequest(HttpMethod.Get, BaseUrl + "/api/project/" + id))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                string text = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Project>(text, JsonOptions);
            }
        }
    }
}
