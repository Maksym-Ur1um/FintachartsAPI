namespace FintachartsAPI.Configuration
{
    public class FintachartsOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string WssUrl { get; set; } = string.Empty;

        public const string SectionName = "FintachartsApi";
    }
}