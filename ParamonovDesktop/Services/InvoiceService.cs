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
    public class InvoiceService : BaseHttpService
    {
        public async Task<Invoice> GetInvoice(int id)
        {
            using (var request = CreateRequest(HttpMethod.Get, BaseUrl + "/api/invoice/" + id))
            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK) return null;
                string text = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Invoice>(text, JsonOptions);
            }
        }

        public async Task<bool> Update(Invoice item)
        {
            using (var request = CreateRequest(HttpMethod.Put,
                BaseUrl + "/api/invoice/" + item.IdInvoice))
            {
                request.Content = ToJson(item);
                using (var response = await Client.SendAsync(request))
                {
                    return response.StatusCode == HttpStatusCode.NoContent ||
                           response.StatusCode == HttpStatusCode.OK;
                }
            }
        }
    }
}
