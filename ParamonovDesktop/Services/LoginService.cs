using ParamonovDesktop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ParamonovDesktop.Services
{
    public class LoginService : BaseHttpService
    {
        public async Task<LoginResponse> SignIn(LoginRequest request)
        {
            using (var response = await Client.PostAsync(BaseUrl + "/login",
                JsonContent.Create(request)))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                string text = await response.Content.ReadAsStringAsync();
                var resp = JsonSerializer.Deserialize<LoginResponse>(text, JsonOptions);
                if (resp != null)
                    Token = resp.AccessToken;
                return resp;
            }
        }

        public async Task<string> Register(RegisterRequest request)
        {
            using (var response = await Client.PostAsync(BaseUrl + "/register",
                JsonContent.Create(request)))
            {
                if (response.StatusCode == HttpStatusCode.OK ||
                    response.StatusCode == HttpStatusCode.Created)
                    return null;

                string error = await response.Content.ReadAsStringAsync();
                return string.IsNullOrWhiteSpace(error)
                    ? "Ошибка регистрации: " + response.StatusCode
                    : error;
            }
        }
    }
}
