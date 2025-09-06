using Microsoft.EntityFrameworkCore;
using Termino.Domain.Entities;
using Termino.Domain.Enums;

namespace Termino.Data.Repositories;

public class TermRepository:ITermRepository
{

    private readonly TerminoDbContext _db;

    public TermRepository(TerminoDbContext db)=>_db=db;

    public async Task AddAsync(TermItem item, CancellationToken ct=default)
    {
        _db.Terms.Add(item); await _db.SaveChangesAsync(ct); 
    }

    public async Task UpdateAsync(TermItem item, CancellationToken ct = default)
    {
        if (item.Status == TermStatus.Completed)   // ???? ????????? “?????????”, ??? ?????????????? ?????? “??????”
        {
            item.Status = TermStatus.Pending;
            item.CompletedAt = null;
        }
        _db.Terms.Update(item);
        await _db.SaveChangesAsync(ct);
    }


    public async Task MarkCompletedAsync(Guid id, CancellationToken ct=default)
    {
        var e=await _db.Terms.FirstOrDefaultAsync(x=>x.Id==id, ct);

        if(e!=null)
        { 
            e.Status=TermStatus.Completed;

            e.CompletedAt=DateTimeOffset.Now;

            await _db.SaveChangesAsync(ct); 
        }
    }

    public async Task ReopenAsync(Guid id, DateTimeOffset newDue, CancellationToken ct=default)
    {
        var e=await _db.Terms.FirstOrDefaultAsync(x=>x.Id==id, ct);

        if(e!=null)
        { 
            e.Status=TermStatus.Pending; 

            e.CompletedAt=null;

            e.DueAt=newDue;

            e.UpdatedAt=DateTimeOffset.Now; 

            await _db.SaveChangesAsync(ct); 
        }
    }

    public Task<TermItem?> GetAsync(Guid id, CancellationToken ct=default)=> _db.Terms.AsNoTracking().FirstOrDefaultAsync(t=>t.Id==id, ct);
    public Task<List<TermItem>> GetAllAsync(CancellationToken ct=default)=> _db.Terms.AsNoTracking().OrderByDescending(t=>t.Status==TermStatus.Pending).ThenBy(t=>t.DueAt).ToListAsync(ct);
}