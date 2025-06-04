using System.Windows;

namespace AIPrototypeAssetRepair
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RepairService _repairService;
        private string _currentPrompt = "";
        public MainWindow()
        {
            InitializeComponent();
            _repairService = new RepairService(); // or inject dependencies if you want
        }

        private void btnGeneratePrompt_Click(object sender, RoutedEventArgs e)
        {
            // Load data
            var asset = _repairService.LoadAssets().First();
            var contractors = _repairService.LoadContractors();
            var eventLog = _repairService.LoadRepairEvents().First();
            var repairLogs = _repairService.LoadRepairLogs();
            var similarLogs = _repairService.FindSimilarLogs(eventLog, repairLogs);

            // Build prompt
            _currentPrompt = AssetHelper.BuildRepairPrompt(asset, contractors, eventLog, repairLogs, similarLogs);
            txtPrompt.Text = _currentPrompt;
            btnRunAI.IsEnabled = true;
        }

        private async void btnRunAI_Click(object sender, RoutedEventArgs e)
        {
            btnRunAI.IsEnabled = false;
            txtOutput.Text = "Calling AI model... please wait.";

            try
            {
                var result = await _repairService.GetBestContractorRecommendationAsync();
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
    }
}