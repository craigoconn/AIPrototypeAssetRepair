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
            List<RepairLog> similarLogs,
            List<(Contractor contractor, double score, List<RepairLog> logs)>? rankedContractors = null)
        {
            var sb = new StringBuilder();

            // 🔧 Asset and failure details
            sb.AppendLine("A maintenance event has been reported. Your task is to recommend the most suitable contractor from the list of top-ranked candidates.");
            sb.AppendLine("You should consider skill relevance, location, availability, past repair performance, and repair speed.");
            sb.AppendLine();
            sb.AppendLine("Asset Details:");
            sb.AppendLine($"- Type: {asset.AssetType}");
            sb.AppendLine($"- ID: {asset.AssetId}");
            sb.AppendLine($"- Location: {asset.Location}");
            sb.AppendLine($"- Failure Description: {currentEvent.FailureDescription}");
            sb.AppendLine($"- Reported At: {currentEvent.ReportedAt:yyyy-MM-dd HH:mm}");
            sb.AppendLine();

            // 🔧 Past repair logs for this event
            sb.AppendLine("Past Repairs for This Event:");
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
                sb.AppendLine("- None.");
            }
            sb.AppendLine();

            // 👷 Available top-ranked contractors
            sb.AppendLine("Top-Ranked Contractor Candidates:");
            if (rankedContractors != null && rankedContractors.Any())
            {
                int rank = 1;
                foreach (var (contractor, score, logs) in rankedContractors)
                {
                    var avgDuration = logs.Any()
                        ? logs.Average(log => (log.RepairEndedAt - log.RepairStartedAt).TotalMinutes)
                        : 0;

                    sb.AppendLine($"{rank}. {contractor.Name} (Score: {score:F2})");
                    sb.AppendLine($"   - Location: {contractor.BaseLocation}");
                    sb.AppendLine($"   - Availability: {contractor.Availability:yyyy-MM-dd HH:mm}");
                    sb.AppendLine($"   - Skills: {string.Join(", ", contractor.Skills)}");
                    sb.AppendLine($"   - Matching Repair Logs: {logs.Count}, Avg Duration: {avgDuration:F0} minutes");
                    rank++;
                }
            }
            else
            {
                sb.AppendLine("- None ranked.");
            }
            sb.AppendLine();

            // 🔍 Similar repair cases (historical logs)
            sb.AppendLine("Similar Past Repair Logs:");
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

            // 📢 Final instructions
            sb.AppendLine("Recommendation Instructions:");
            sb.AppendLine("1. Select the best contractor from the ranked candidates.");
            sb.AppendLine("2. Justify your choice based on skill match, proximity, availability, and repair history.");
            sb.AppendLine("3. Mention whether a single contractor or a team is needed.");
            sb.AppendLine("4. Avoid bullet points or JSON. Respond in plain text format.");

            return sb.ToString();
        }
    }
}
