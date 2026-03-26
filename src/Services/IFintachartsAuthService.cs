namespace FintachartsAPI.Services
{
    public interface IFintachartsAuthService
    {
        Task<string> GetTokenAsync();
    }
}