namespace FintachartsAPI.Models
{
    public class Asset
    {
        public Guid Id { get; set; }
        public Guid FintachartsId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
    }
}