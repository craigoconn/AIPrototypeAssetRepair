using AIPrototypeAssetRepair.Models;

namespace AIPrototypeAssetRepair.Core
{
    public class AgentContext
    {
        public Asset Asset { get; set; }
        public RepairEvent CurrentEvent { get; set; }
        public List<Contractor> Contractors { get; set; } = new();
        public List<RepairLog> RepairLogs { get; set; } = new();
        public List<RepairLog> SimilarLogs { get; set; } = new();

        // Agents can store notes here (e.g. insights, analysis, intermediate outputs)
        public Dictionary<string, string> Notes { get; set; } = new();

        // Optional: Can store prompts or additional debugging information
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
