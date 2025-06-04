using AIPrototypeAssetRepair;
using AIPrototypeAssetRepair.Helper;
using AIPrototypeAssetRepair.Models;
using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
public class RepairService
{
    private IChatClient _client;
    private List<Asset> _assets;
    private List<Contractor> _contractors;
    private List<RepairEvent> _events;
    private List<RepairLog> _logs;

    public RepairService()
    {
        var githubToken = "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";

        _client = new ChatCompletionsClient(
            endpoint: new Uri("https://models.inference.ai.azure.com"),
            new AzureKeyCredential(githubToken))
            .AsIChatClient("grok-3-mini");

        _assets = JsonLoader.LoadJsonList<Asset>("data/assets.json");
        _contractors = JsonLoader.LoadJsonList<Contractor>("data/contractors.json");
        _events = JsonLoader.LoadJsonList<RepairEvent>("data/repairevent.json");
        _logs = JsonLoader.LoadJsonList<RepairLog>("data/repairlog.json");
    }

    public List<Asset> LoadAssets() => JsonLoader.LoadJsonList<Asset>("data/assets.json");
    public List<Contractor> LoadContractors() => JsonLoader.LoadJsonList<Contractor>("data/contractors.json");
    public List<RepairEvent> LoadRepairEvents() => JsonLoader.LoadJsonList<RepairEvent>("data/repairevent.json");
    public List<RepairLog> LoadRepairLogs() => JsonLoader.LoadJsonList<RepairLog>("data/repairlog.json");


    public string BuildPromptForEvent(
    RepairEvent eventLog,
    List<RankedContractorViewModel> topRankedViewModels)
    {
        var asset = _assets.FirstOrDefault(a => a.AssetId == eventLog.AssetId);
        if (asset == null) throw new InvalidOperationException($"Asset not found: {eventLog.AssetId}");

        var contractors = topRankedViewModels.Select(vm => vm.Contractor).ToList();
        var logs = _logs;
        var rankedTuples = topRankedViewModels.Select(vm => (vm.Contractor, vm.Score, vm.Logs)).ToList();
        var combinedSimilarLogs = topRankedViewModels.SelectMany(vm => vm.Logs).Distinct().ToList();

        return AssetHelper.BuildRepairPrompt(
            asset,
            contractors,
            eventLog,
            logs,
            combinedSimilarLogs,
            rankedTuples
        );
    }


    public async Task<string> SendPromptToAIAsync(string prompt)
    {
        try
        {
            var response = await _client.GetResponseAsync(prompt);

            if (string.IsNullOrWhiteSpace(response?.Text))
            {
                return "⚠️ No response received from the AI model. Please try again.";
            }

            return response.Text;
        }
        catch (Exception ex)
        {
            return $"❌ Error communicating with AI: {ex.Message}";
        }
    }


    public async Task<List<(Contractor contractor, double score, List<RepairLog> logs)>> RankContractorsByPromptedRelevanceAsync(
RepairEvent ev)
    {
        var asset = _assets.FirstOrDefault(a => a.AssetId == ev.AssetId);
        if (asset == null) return new();

        var candidates = _contractors
            .Where(c =>
                (c.BaseLocation == asset.Location ||
                 _logs.Any(l => l.ContractorName == c.Name && l.AssetId == asset.AssetId)))
            .ToList();

        var results = new List<(Contractor, double, List<RepairLog>)>();

        foreach (var contractor in candidates)
        {
            var contractorLogs = _logs
                .Where(l => l.ContractorName == contractor.Name && l.AssetId == ev.AssetId)
                .ToList();

            var matchingLogs = new List<(RepairLog log, double relevance)>();

            foreach (var log in contractorLogs)
            {
                var relevance = await ScoreLogRelevanceAsync(ev.FailureDescription, log.Notes);
                if (relevance > 0.5) // keep threshold flexible
                    matchingLogs.Add((log, relevance));
            }

            if (matchingLogs.Any())
            {
                var avgRelevance = matchingLogs.Average(x => x.relevance);
                var avgDuration = matchingLogs.Average(x =>
                    (x.log.RepairEndedAt - x.log.RepairStartedAt).TotalMinutes);
                var durationScore = 1 - NormalizeDuration(avgDuration);
                var finalScore = 0.7 * avgRelevance + 0.3 * durationScore;

                results.Add((contractor, finalScore, matchingLogs.Select(x => x.log).ToList()));
            }
        }

        return results.OrderByDescending(r => r.Item2).ToList();
    }

    public List<(Contractor contractor, double score, List<RepairLog> logs)> FakeRankContractors(RepairEvent ev)
    {
        var asset = _assets.FirstOrDefault(a => a.AssetId == ev.AssetId);
        if (asset == null) return new();

        var random = new Random();

        var results = _contractors
            .Where(c =>
                c.BaseLocation == asset.Location ||
                _logs.Any(l => l.ContractorName == c.Name && l.AssetId == ev.AssetId))
            .Select(c =>
            {
                var logs = _logs.Where(l => l.ContractorName == c.Name && l.AssetId == ev.AssetId).ToList();
                int logCount = logs.Count;

                // Base score is 0.5, boosted by log count and small random noise
                double score = 0.5 + Math.Min(0.4, logCount * 0.1) + random.NextDouble() * 0.1;

                return (c, score, logs);
            })
            .OrderByDescending(r => r.score)
            .ToList();

        return results;
    }

    private async Task<double> ScoreLogRelevanceAsync(string failureDescription, string logNotes)
    {
        var prompt = $@"
            Given the following failure description and a repair log entry, score their relevance on a scale from 0.0 to 1.0.
            Only return a numeric score, no explanation.

            Failure: ""{failureDescription}""
            Repair Log: ""{logNotes}""

            Relevance score:";

        var response = await SendPromptToAIAsync(prompt);

        if (double.TryParse(response, out double score))
        {
            return Math.Clamp(score, 0.0, 1.0);
        }

        return 0.0; // fallback if parse fails
    }


    private double NormalizeDuration(double minutes)
    {
        if (minutes <= 30) return 0;
        if (minutes >= 240) return 1;
        return (minutes - 30) / (240 - 30);
    }
}
