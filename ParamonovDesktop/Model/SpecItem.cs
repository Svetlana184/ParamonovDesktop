using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParamonovDesktop.Model
{
    public class SpecItem
    {
        public int IdItem { get; set; }
        public int IdSpec { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsNew { get; set; }
    }
}
