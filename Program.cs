using AIPrototypeAssetRepair;
using AIPrototypeAssetRepair.AIPrototypeAssetRepair.Helper;
using AIPrototypeAssetRepair.Helper;
using AIPrototypeAssetRepair.Models;
using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

var githubToken = "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";
if (string.IsNullOrEmpty(githubToken))
{
    var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
    githubToken = config["GITHUB_TOKEN"];
}

IChatClient client = new ChatCompletionsClient(
        endpoint: new Uri("https://models.inference.ai.azure.com"),
        new AzureKeyCredential(githubToken))
        .AsIChatClient("Phi-3.5-MoE-instruct");


// Load data from JSON
var assetList = JsonLoader.LoadJsonList<Asset>("data/assets.json");
var contractors = JsonLoader.LoadJsonList<Contractor>("data/contractors.json");
var currentEvents = JsonLoader.LoadJsonList<RepairEvent>("data/repairevent.json");
var repairLogs = JsonLoader.LoadJsonList<RepairLog>("data/repairlog.json");

// Pick one asset to process
var asset = assetList.First();
var currentEvent = currentEvents.First();
// Find similar past logs
var similarLogs = SimilarityHelper.FindMostSimilarLogs(currentEvent.FailureDescription, repairLogs);


// Build prompt and call AI
var prompt = AssetHelper.BuildRepairPrompt(asset, contractors, currentEvent, repairLogs, similarLogs); 
var response = await client.GetResponseAsync(prompt);
Console.WriteLine(response.Text);
