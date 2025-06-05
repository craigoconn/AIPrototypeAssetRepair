using AIPrototypeAssetRepair.Models;
using System.Windows;
using System.Windows.Controls;

namespace AIPrototypeAssetRepair
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RepairService _repairService;
        private string _currentPrompt = "";
        private List<RepairEvent> _repairEvents;
        private int _currentEventIndex = 0;
        public MainWindow()
        {
            InitializeComponent();
            _repairService = new RepairService(); // or inject dependencies if you want
            _repairEvents = _repairService.LoadRepairEvents();
            ShowCurrentEventInfo(_repairEvents[_currentEventIndex]);
        }

        private void btnGeneratePrompt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnGeneratePrompt.IsEnabled = false;
                txtPrompt.Text = "Generating prompt...";

                if (_repairEvents == null || !_repairEvents.Any())
                {
                    txtPrompt.Text = "❌ No repair events loaded.";
                    return;
                }

                if (lstRankedContractors.ItemsSource == null || lstRankedContractors.Items.Count == 0)
                {
                    txtPrompt.Text = "⚠️ Please rank contractors before generating the prompt.";
                    return;
                }

                var eventLog = _repairEvents[_currentEventIndex];
                var topRanked = lstRankedContractors.Items
                    .Cast<RankedContractorViewModel>()
                    .Take(3)
                    .ToList();

                _currentPrompt = _repairService.BuildPromptForEvent(eventLog, topRanked);

                txtPrompt.Text = _currentPrompt;
                btnRunAI.IsEnabled = true;

                Title = $"Asset Repair AI Demo - Event {eventLog.EventId} ({_currentEventIndex + 1}/{_repairEvents.Count})";
            }
            catch (Exception ex)
            {
                txtPrompt.Text = $"❌ Error during prompt generation: {ex.Message}";
            }
            finally
            {
                btnGeneratePrompt.IsEnabled = true;
            }
        }

        private async void btnRunAI_Click(object sender, RoutedEventArgs e)
        {
            btnRunAI.IsEnabled = false;
            txtOutput.Text = "Calling AI model... please wait.";

            try
            {
                var result = await _repairService.SendPromptToAIAsync(_currentPrompt);
                txtOutput.Text = result;
            }
            catch (Exception ex)
            {
                txtOutput.Text = $"Error: {ex.Message}";
            }
            finally
            {
                btnRunAI.IsEnabled = true;
            }
        }

        private void lstRankedContractors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstRankedContractors.SelectedItem is RankedContractorViewModel selected)
            {
                lstContractorLogs.ItemsSource = selected.Logs;
            }
        }

        private async void btnRankContractors_Click(object sender, RoutedEventArgs e)
        {
            var currentEvent = _repairEvents[_currentEventIndex];
            var dialogue = GenerateFakeAIReasoning(currentEvent);

            txtAIReasoning.Text = "";
            foreach (var (question, answer) in dialogue)
            {
                txtAIReasoning.Text += $"🧠 {question}\n";
                await Task.Delay(800);
                txtAIReasoning.Text += $"{answer}\n\n";
                await Task.Delay(800);
            }

            // Now do the actual (fake) ranking
            var fakeRanked = _repairService.FakeRankContractors(currentEvent);
            if (!fakeRanked.Any())
            {
                txtPrompt.Text += "⚠️ No suitable contractor found.";
                lstRankedContractors.ItemsSource = null;
                lstContractorLogs.ItemsSource = null;
                btnRunAI.IsEnabled = false;
                return;
            }

            var viewModels = fakeRanked.Select(r => new RankedContractorViewModel
            {
                Contractor = r.contractor,
                Score = r.score,
                Logs = r.logs
            }).ToList();

            lstRankedContractors.ItemsSource = viewModels;
            btnRunAI.IsEnabled = true;
        }


        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEventIndex < _repairEvents.Count - 1)
            {
                _currentEventIndex++;
                ClearUIForNewEvent();
                ShowCurrentEventInfo(_repairEvents[_currentEventIndex]);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEventIndex > 0)
            {
                _currentEventIndex--;
                ClearUIForNewEvent();
                ShowCurrentEventInfo(_repairEvents[_currentEventIndex]);
            }
        }
        private void ClearUIForNewEvent()
        {
            txtPrompt.Text = "";
            txtOutput.Text = "";
            lstRankedContractors.ItemsSource = null;
            lstContractorLogs.ItemsSource = null;
            btnRunAI.IsEnabled = false;
        }

        private void ShowCurrentEventInfo(RepairEvent ev)
        {
            txtCurrentEventInfo.Text = $"Event ID: {ev.EventId}\n" +
                                       $"Asset ID: {ev.AssetId}\n" +
                                       $"Reported: {ev.ReportedAt:g}\n" +
                                       $"Failure: {ev.FailureDescription}";
            Title = $"Asset Repair AI Demo - Event {ev.EventId} ({_currentEventIndex + 1}/{_repairEvents.Count})";
        }

        private List<(string Question, string Answer)> GenerateFakeAIReasoning(RepairEvent ev)
        {
            var asset = _repairService.LoadAssets().FirstOrDefault(a => a.AssetId == ev.AssetId);
            var contractors = _repairService.LoadContractors();
            var logs = _repairService.LoadRepairLogs();

            var availableContractors = contractors
                .Where(c => c.BaseLocation == asset?.Location)
                .Select(c => c.Name)
                .ToList();

            var similarLogs = logs
                .Where(l => l.AssetId == ev.AssetId && l.Notes.Contains("motor", StringComparison.OrdinalIgnoreCase))
                .Take(2)
                .ToList();

            return new List<(string, string)>
    {
        ("1. Find me all available contractors based on location.",
         $"=> Found {availableContractors.Count} contractor(s) in {asset?.Location}: {string.Join(", ", availableContractors)}"),

        ("2. Check the incident logs for any similar issues for this asset.",
         $"=> {similarLogs.Count} matching past repair log(s) found with similar failure descriptions."),

        ("3. Check relevant expertise.",
         "=> Filtering contractors by skill tags relevant to pumps, motors, electrical faults."),

        ("4. Rank and score the contractors and give me recommendations.",
         "=> Done. Ranking complete based on log history and estimated repair efficiency.")
    };
        }

    }
}