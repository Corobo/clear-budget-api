using Microsoft.EntityFrameworkCore;
using TransactionsService.Models.DB;
using TransactionsService.Repositories.Data;

namespace TransactionsService.Repositories.Impl 
{ 

public class TransactionRepository : ITransactionRepository
{
    private readonly TransactionsDbContext _context;

    public TransactionRepository(TransactionsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transaction>> GetAllByUserIdAsync(string userId)
    {
        return await _context.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, string userId)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        transaction.Id = Guid.NewGuid();
        transaction.CreatedAt = DateTime.UtcNow;

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return transaction;
    }

    public async Task<bool> UpdateAsync(Transaction transaction)
    {
        var existing = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == transaction.Id && t.UserId == transaction.UserId);

        if (existing == null) return false;

        existing.Amount = transaction.Amount;
        existing.Description = transaction.Description;
        existing.CategoryId = transaction.CategoryId;
        existing.Type = transaction.Type;
        existing.Date = transaction.Date;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null) return false;

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        return true;
    }
}
}