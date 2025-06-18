using System.Linq.Expressions;

namespace Devlooped.WhatsApp;

static class BlobRepository
{

}

class BlobRepository<T> : IDocumentRepository<T> where T : class
{
    static readonly string documentType = typeof(T).FullName?.Replace('+', '.') ?? typeof(T).Name;
    static readonly string documentVersion;
    static readonly int documentMajorVersion;
    static readonly int documentMinorVersion;

    static BlobRepository()
    {
        var version = (typeof(T).Assembly.GetName().Version ?? new Version(1, 0));
        documentVersion = version.ToString(2);
        documentMajorVersion = version.Major;
        documentMinorVersion = version.Minor;
    }

    public string TableName => throw new NotImplementedException();

    public Task<bool> DeleteAsync(T entity, CancellationToken cancellation = default) => throw new NotImplementedException();
    public Task<bool> DeleteAsync(string partitionKey, string rowKey, CancellationToken cancellation = default) => throw new NotImplementedException();

    public Task<T?> GetAsync(string partitionKey, string rowKey, CancellationToken cancellation = default) => throw new NotImplementedException();
    public Task<T?> GetAsync(T entity, CancellationToken cancellation = default) => throw new NotImplementedException();
    public Task<T> PutAsync(T entity, CancellationToken cancellation = default) => throw new NotImplementedException();
    public Task PutAsync(IEnumerable<T> entities, CancellationToken cancellation = default) => throw new NotImplementedException();


    public IAsyncEnumerable<T> EnumerateAsync(Expression<Func<IDocumentEntity, bool>> predicate, CancellationToken cancellation = default) => throw new NotSupportedException();
    public IAsyncEnumerable<T> EnumerateAsync(string? partitionKey = null, CancellationToken cancellation = default) => throw new NotSupportedException();
}