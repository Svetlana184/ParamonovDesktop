using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParamonovDesktop.Model
{
    public class UserProfile
    {
        public int IdUser { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
        public int? IdTeam { get; set; }
        public int? IdCompany { get; set; }
    }
}
