using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParamonovDesktop.Model
{
    public class Invoice
    {
        public int IdInvoice { get; set; }
        public string Name { get; set; }
        public decimal Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? IdProject { get; set; }
        public decimal TotalCost { get; set; }
    }
}
