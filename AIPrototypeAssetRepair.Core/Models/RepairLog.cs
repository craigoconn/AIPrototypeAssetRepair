using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPrototypeAssetRepair.Models
{
    public class RepairLog
    {
        public string EventId { get; set; }
        public string AssetId { get; set; }
        public string ContractorName { get; set; }
        public DateTime RepairStartedAt { get; set; }
        public DateTime RepairEndedAt { get; set; }
        public string Notes { get; set; }
    }
}
