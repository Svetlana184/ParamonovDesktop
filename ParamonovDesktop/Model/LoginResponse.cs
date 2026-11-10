using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ParamonovDesktop.Model
{
    public class LoginResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
        public int IdUser { get; set; }
        public int? IdTeam { get; set; }
    }
}
