namespace FintachartsAPI.Configuration
{
    public class FintachartsOptions
    {
        public string BaseUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public const string SectionName = "FintachartsApi";
    }
}