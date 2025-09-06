using Termino.Domain.Entities;
namespace Termino.Data.Repositories;
public interface ITermRepository{
    Task AddAsync(TermItem item, CancellationToken ct=default);
    Task UpdateAsync(TermItem item, CancellationToken ct=default);
    Task MarkCompletedAsync(Guid id, CancellationToken ct=default);
    Task ReopenAsync(Guid id, DateTimeOffset newDue, CancellationToken ct=default);
    Task<TermItem?> GetAsync(Guid id, CancellationToken ct=default);
    Task<List<TermItem>> GetAllAsync(CancellationToken ct=default);
}