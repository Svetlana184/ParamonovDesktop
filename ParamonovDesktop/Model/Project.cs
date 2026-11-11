using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParamonovDesktop.Model
{
    public class Project
    {
        public int IdProject { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int IdCustomer { get; set; }
        public int? IdTeam { get; set; }

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case "draft": return "Черновик";
                    case "in_progress": return "В работе";
                    case "pending_approval": return "Требует одобрения";
                    case "completed": return "Закончен";
                    case "closed": return "Закрыт";
                    case "frozen": return "Заморожен";
                    default: return "Неизвестно";
                }
            }
        }
        public bool CanEditStatus
        {
            get { return Status != "completed" && Status != "closed"; }
        }
    }
}
