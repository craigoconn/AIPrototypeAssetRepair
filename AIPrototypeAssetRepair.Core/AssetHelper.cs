using AIPrototypeAssetRepair.Models;
using System.Text;

namespace AIPrototypeAssetRepair
{
    public static class AssetHelper
    {
        public static string BuildRepairPrompt(
            Asset asset,
            List<Contractor> contractors,
            RepairEvent currentEvent,
            List<RepairLog> repairLogs,
            List<RepairLog> similarLogs)
        {
            var sb = new StringBuilder();

            // 🔧 Asset and failure details
            sb.AppendLine("Asset failure reported. Recommend the most suitable contractor(s) to assign, considering skills, location, availability, and historical performance.");
            sb.AppendLine();
            sb.AppendLine("Asset Details:");
            sb.AppendLine($"- Type: {asset.AssetType}");
            sb.AppendLine($"- ID: {asset.AssetId}");
            sb.AppendLine($"- Location: {asset.Location}");
            sb.AppendLine($"- Failure: {currentEvent.FailureDescription}");
            sb.AppendLine($"- Reported At: {currentEvent.ReportedAt:yyyy-MM-dd HH:mm}");
            sb.AppendLine();

            // Past repair logs
            sb.AppendLine("Past Repairs:");
            var pastLogs = repairLogs.Where(log => log.EventId == currentEvent.EventId).ToList();
            if (pastLogs.Any())
            {
                foreach (var log in pastLogs)
                {
                    sb.AppendLine($"- {log.ContractorName}: {log.RepairStartedAt:yyyy-MM-dd HH:mm} to {log.RepairEndedAt:yyyy-MM-dd HH:mm}. Notes: {log.Notes}");
                }
            }
            else
            {
                sb.AppendLine("- None for this event.");
            }
            sb.AppendLine();

            // Available contractors
            sb.AppendLine("Available Contractors:");
            foreach (var contractor in contractors)
            {
                sb.AppendLine($"- {contractor.Name}, Base: {contractor.BaseLocation}, Skills: {string.Join(", ", contractor.Skills)}, Available: {contractor.Availability:yyyy-MM-dd HH:mm}");
            }
            sb.AppendLine();

            // Similar repair cases
            sb.AppendLine("Similar Repair Cases:");
            if (similarLogs.Any())
            {
                foreach (var log in similarLogs)
                {
                    sb.AppendLine($"- {log.ContractorName}: {log.RepairStartedAt:yyyy-MM-dd} to {log.RepairEndedAt:yyyy-MM-dd}. Notes: {log.Notes}");
                }
            }
            else
            {
                sb.AppendLine("- None found.");
            }
            sb.AppendLine();

            // Final instruction for response format
            sb.AppendLine("Recommendation Instructions:");
            sb.AppendLine("Respond with:");
            sb.AppendLine("1. A brief recommendation of the best contractor(s).");
            sb.AppendLine("2. Key reasons based on skills, proximity, availability, and past experience.");
            sb.AppendLine("3. Indicate if a team is needed or one contractor is sufficient.");
            sb.AppendLine("Use plain text. Avoid bullet points or Markdown formatting.");
            //sb.AppendLine("Respond in JSON format with keys: contractor, reasons, teamRequired (true/false). Avoid extra commentary.");
            return sb.ToString();
        }
    }
}
