namespace Shared.Auth
{
    public interface IAuthTokenClient
    {
        Task<string> GetAccessTokenAsync();
    }
}
