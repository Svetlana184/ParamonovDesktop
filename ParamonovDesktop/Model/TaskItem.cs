using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParamonovDesktop.Model
{
    public class TaskItem
    {
        public int IdTask { get; set; }
        public int IdTaskList { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public decimal? PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public decimal? Cost { get; set; }
        public int? IdDeveloper { get; set; }
    }
}
