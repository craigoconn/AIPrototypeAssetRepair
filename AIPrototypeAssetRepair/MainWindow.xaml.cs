using AIPrototypeAssetRepair.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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

        private void btnRankContractors_Click(object sender, RoutedEventArgs e)
        {
            var currentEvent = _repairEvents[_currentEventIndex];
            var fakeRanked = _repairService.FakeRankContractors(currentEvent);

            if (!fakeRanked.Any())
            {
                txtPrompt.Text = "⚠️ No suitable contractor found.";
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

            txtPrompt.Text = $"✔️ (Demo) Ranked {viewModels.Count} contractors for event {currentEvent.EventId}.";
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
    }
}