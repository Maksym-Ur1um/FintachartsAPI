namespace FintachartsAPI.Models
{
    public class AssetPrice
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public Asset? Asset { get; set; }
        public decimal Price { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}