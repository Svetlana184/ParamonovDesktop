using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParamonovDesktop.Model
{
    public class ChangeRequest
    {
        public int IdChange { get; set; }
        public int IdSpecItem { get; set; }
        public int? IdTask { get; set; }
        public int? IdInvoice { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Status { get; set; }
        public int IdCreator { get; set; }
        public int? IdApprover { get; set; }
    }
}
