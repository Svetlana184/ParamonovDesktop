using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ParamonovDesktop.Model
{
    public partial class TechnicalSpecification
    {
        public int IdSpec { get; set; }

        public int IdProject { get; set; }

        public string Name { get; set; } = null!;

        public DateOnly? Deadline { get; set; }

        public string? Description { get; set; }
    }

}
