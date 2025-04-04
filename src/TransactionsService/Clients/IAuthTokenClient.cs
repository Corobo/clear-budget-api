namespace TransactionsService.Clients
{
    public interface IAuthTokenClient
    {
        Task<string> GetAccessTokenAsync();
    }
}
