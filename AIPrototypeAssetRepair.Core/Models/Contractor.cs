namespace AIPrototypeAssetRepair.Models
{
    public class Contractor
    {
        public string Name { get; set; }
        public string BaseLocation { get; set; }
        public List<string> Skills { get; set; }
        public DateTime Availability { get; set; }
    }
}
