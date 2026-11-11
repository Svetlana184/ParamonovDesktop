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
    public class UserService : BaseHttpService
    {
        public async Task<UserProfile> GetUser(int id)
        {
            using (var request = CreateRequest(HttpMethod.Get, BaseUrl + "/api/user/" + id))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK) return null;
                string text = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UserProfile>(text, JsonOptions);
            }
        }
    }
}
