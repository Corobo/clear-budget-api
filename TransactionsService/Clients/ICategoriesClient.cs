namespace TransactionsService.Clients
{
    public interface ICategoriesClient
    {
        Task<IEnumerable<Guid>> GetAllCategoryIdsAsync(CancellationToken cancellationToken = default);
        Task<string> GetAccessTokenAsync();
    }
}
