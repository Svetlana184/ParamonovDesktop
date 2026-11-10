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
    public class DirectoryService : BaseHttpService
    {
        public async Task<List<CompanyItem>> GetCompanies()
        {
            using (var request = CreateRequest(HttpMethod.Get, BaseUrl + "/api/company"))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK) return null;

                string text = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<CompanyItem>>(text, JsonOptions);
            }
        }

        public async Task<List<TeamItem>> GetTeams()
        {
            using (var request = CreateRequest(HttpMethod.Get, BaseUrl + "/api/team"))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK) return null;

                string text = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<TeamItem>>(text, JsonOptions);
            }
        }
    }
}
