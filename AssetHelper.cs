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

            // 📢 High-level instruction
            sb.AppendLine("An asset has gone offline. Recommend the best available contractor(s) based on location, skills, availability, and similar past repair cases.");
            sb.AppendLine("Respond in clear, structured markdown. Format your answer as a numbered list of reasons for selecting the contractor(s), followed by your final recommendation.");
            sb.AppendLine("Avoid repeating section headers like 'Location' or 'Skills'. Keep your explanation concise and focused.");
            sb.AppendLine();

            // 🛠️ Asset + event details
            sb.AppendLine("## 🔧 Asset Details");
            sb.AppendLine($"- **Asset Type**: {asset.AssetType}");
            sb.AppendLine($"- **Asset ID**: {asset.AssetId}");
            sb.AppendLine($"- **Location**: {asset.Location}");
            sb.AppendLine($"- **Failure**: {currentEvent.FailureDescription}");
            sb.AppendLine($"- **Reported At**: {currentEvent.ReportedAt:yyyy-MM-dd HH:mm}");
            sb.AppendLine();

            // 📜 Past repair logs
            var pastLogs = repairLogs.Where(log => log.EventId == currentEvent.EventId).ToList();
            sb.AppendLine("## 🧾 Past Repairs");
            if (pastLogs.Any())
            {
                foreach (var log in pastLogs)
                {
                    sb.AppendLine($"- {log.ContractorName} | {log.RepairStartedAt:yyyy-MM-dd HH:mm} → {log.RepairEndedAt:yyyy-MM-dd HH:mm} | Notes: {log.Notes}");
                }
            }
            else
            {
                sb.AppendLine("- No past repairs found for this event.");
            }
            sb.AppendLine();

            // 👷 Contractor list
            sb.AppendLine("## 👷 Available Contractors");
            foreach (var contractor in contractors)
            {
                sb.AppendLine($"- **Name**: {contractor.Name}");
                sb.AppendLine($"  - Base: {contractor.BaseLocation}");
                sb.AppendLine($"  - Skills: {string.Join(", ", contractor.Skills)}");
                sb.AppendLine($"  - Available: {contractor.Availability:yyyy-MM-dd HH:mm}");
            }
            sb.AppendLine();

            // 🔍 Similar repair logs
            sb.AppendLine("## 🧠 Similar Past Repair Cases");
            if (similarLogs.Any())
            {
                foreach (var log in similarLogs)
                {
                    sb.AppendLine($"- {log.ContractorName} | {log.RepairStartedAt:yyyy-MM-dd} → {log.RepairEndedAt:yyyy-MM-dd} | Notes: {log.Notes}");
                }
            }
            else
            {
                sb.AppendLine("- No similar cases found.");
            }
            sb.AppendLine();

            // 📝 Final instruction
            sb.AppendLine("## 📝 Recommendation");
            sb.AppendLine("Who should be assigned to this repair and why? Should a team be dispatched based on estimated repair time?");

            return sb.ToString();
        }
    }
}
