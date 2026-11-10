using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParamonovDesktop.Model
{
    public class RegisterRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }       //"customer" / "manager" / "developer"
        public int? IdTeam { get; set; }
        public int? IdCompany { get; set; }
        public string ShortDescription { get; set; }
    }
}
