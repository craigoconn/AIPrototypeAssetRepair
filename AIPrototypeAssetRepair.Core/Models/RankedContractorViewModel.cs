namespace AIPrototypeAssetRepair.Models
{
    public class RankedContractorViewModel
    {
        public Contractor Contractor { get; set; }
        public double Score { get; set; }
        public List<RepairLog> Logs { get; set; }
        public string DisplayText => $"{Contractor.Name} (Score: {Score:F2})";
    }
}
