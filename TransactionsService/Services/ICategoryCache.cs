namespace TransactionsService.Services
{
    public interface ICategoryCache
    {
        Task InitializeAsync();
        bool IsValidCategory(Guid categoryId);
        Task RefreshAsync();
    }
}
