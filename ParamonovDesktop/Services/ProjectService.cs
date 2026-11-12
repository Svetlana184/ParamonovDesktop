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
                if (response.StatusCode != HttpStatusCode.OK) return null;
                string text = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Project>>(text, JsonOptions);
            }
        }

        public async Task<List<Project>> GetMyProjects()
        {
            using (var request = CreateRequest(HttpMethod.Get, BaseUrl + "/api/project/my"))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK) return null;
                string text = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Project>>(text, JsonOptions);
            }
        }

        public async Task<Project> GetProject(int id)
        {
            using (var request = CreateRequest(HttpMethod.Get, BaseUrl + "/api/project/" + id))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK) return null;
                string text = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Project>(text, JsonOptions);
            }
        }

        public async Task<Project> CreateProject(Project item)
        {
            using (var request = CreateRequest(HttpMethod.Post, BaseUrl + $"/api/project"))
            {
                request.Content = ToJson(item);
                using (var response = await Client.SendAsync(request))
                {
                    if (response.StatusCode != HttpStatusCode.Created &&
                        response.StatusCode != HttpStatusCode.OK) return null;
                    string text = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Project>(text, JsonOptions);
                }
            }
        }

        public async Task<Project?> CreateProjectDraft(int customerId, string? name, string? description)
        {
            var dto = new { idCustomer = customerId, name = name, description = description };
            using (var request = CreateRequest(HttpMethod.Post, BaseUrl + "/api/project/draft"))
            {
                request.Content = ToJson(dto);
                using (var response = await Client.SendAsync(request))
                {
                    if (response.StatusCode != HttpStatusCode.OK &&
                        response.StatusCode != HttpStatusCode.Created) return null;
                    string text = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Project>(text, JsonOptions);
                }
            }
        }

        public async Task<bool> Update(Project item)
        {
            using (var request = CreateRequest(HttpMethod.Put,
                BaseUrl + "/api/project/" + item.IdProject))
            {
                request.Content = ToJson(item);
                using (var response = await Client.SendAsync(request))
                {
                    return response.StatusCode == HttpStatusCode.NoContent ||
                           response.StatusCode == HttpStatusCode.OK;
                }
            }
        }

        public async Task SubmitProject(int id)
        {
            using (var request = CreateRequest(HttpMethod.Post,
                BaseUrl + "/api/project/" + id + "/submit"))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK &&
                    response.StatusCode != HttpStatusCode.NoContent)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception("Не удалось отправить проект (" + response.StatusCode + "):\n" + error);
                }
            }
        }

        public async Task SendInvoice(int id)
        {
            using (var request = CreateRequest(HttpMethod.Post,
                BaseUrl + "/api/project/" + id + "/send-invoice"))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK &&
                    response.StatusCode != HttpStatusCode.NoContent)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception("Не удалось отправить счёт (" + response.StatusCode + "):\n" + error);
                }
            }
        }

        public async Task FinishApproval(int id)
        {
            using (var request = CreateRequest(HttpMethod.Post,
                BaseUrl + "/api/project/" + id + "/finish-approval"))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK &&
                    response.StatusCode != HttpStatusCode.NoContent)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception("Не удалось сохранить изменения (" + response.StatusCode + "):\n" + error);
                }
            }
        }

        public async Task Close(int id, bool permanent)
        {
            string url = BaseUrl + "/api/project/" + id + "/close?permanent=" +
                         (permanent ? "true" : "false");

            using (var request = CreateRequest(HttpMethod.Post, url))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK &&
                    response.StatusCode != HttpStatusCode.NoContent)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception("Не удалось закрыть проект (" + response.StatusCode + "):\n" + error);
                }
            }
        }

        public async Task Freeze(int id)
        {
            using (var request = CreateRequest(HttpMethod.Post,
                BaseUrl + "/api/project/" + id + "/freeze"))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK &&
                    response.StatusCode != HttpStatusCode.NoContent)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception("Не удалось заморозить проект (" + response.StatusCode + "):\n" + error);
                }
            }
        }

        public async Task Unfreeze(int id)
        {
            using (var request = CreateRequest(HttpMethod.Post,
                BaseUrl + "/api/project/" + id + "/unfreeze"))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK &&
                    response.StatusCode != HttpStatusCode.NoContent)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception("Не удалось разморозить проект (" + response.StatusCode + "):\n" + error);
                }
            }
        }
    }
}
