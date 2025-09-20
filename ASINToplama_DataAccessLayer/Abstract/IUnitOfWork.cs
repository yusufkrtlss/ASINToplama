namespace ASINToplama_DataAccessLayer.Abstract
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
