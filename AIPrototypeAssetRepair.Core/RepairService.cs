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
        var githubToken = "token"; 

        _client = new ChatCompletionsClient(
            endpoint: new Uri("https://models.inference.ai.azure.com"),
            new AzureKeyCredential(githubToken))
            .AsIChatClient("grok-3-mini");

        _assets = JsonLoader.LoadJsonList<Asset>("data/assets.json");
        _contractors = JsonLoader.LoadJsonList<Contractor>("data/contractors.json");
        _events = JsonLoader.LoadJsonList<RepairEvent>("data/repairevent.json");
        _logs = JsonLoader.LoadJsonList<RepairLog>("data/repairlog.json");
    }

    public async Task<string> GetBestContractorRecommendationAsync()
    {
        var asset = _assets.First();
        var currentEvent = _events.First();
        var similarLogs = new List<RepairLog>(); // or implement your similarity search

        var prompt = AssetHelper.BuildRepairPrompt(asset, _contractors, currentEvent, _logs, similarLogs);
        var response = await SendPromptToAIAsync(prompt);
        return response.Trim();
    }
    public List<Asset> LoadAssets() => JsonLoader.LoadJsonList<Asset>("data/assets.json");
    public List<Contractor> LoadContractors() => JsonLoader.LoadJsonList<Contractor>("data/contractors.json");
    public List<RepairEvent> LoadRepairEvents() => JsonLoader.LoadJsonList<RepairEvent>("data/repairevent.json");
    public List<RepairLog> LoadRepairLogs() => JsonLoader.LoadJsonList<RepairLog>("data/repairlog.json");

    public List<RepairLog> FindSimilarLogs(RepairEvent ev, List<RepairLog> allLogs)
    {
        // Return mock similarity results, or use Semantic Kernel in future
        return allLogs
            .Where(log => log.Notes.Contains("motor", StringComparison.OrdinalIgnoreCase))
            .Take(3)
            .ToList();
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
}
