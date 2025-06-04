using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPrototypeAssetRepair.Models
{
    public class RepairEvent
    {
        public string EventId { get; set; }
        public string AssetId { get; set; }
        public string FailureDescription { get; set; }
        public DateTime ReportedAt { get; set; }

    }
}
