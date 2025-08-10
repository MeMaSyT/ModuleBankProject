using Microsoft.EntityFrameworkCore;
using ModulebankProject.Infrastructure.Data;
using Npgsql;

namespace ModulebankProject.Infrastructure.AccrueInterest
{
    public class AccrueInterestHandler
    {
        private readonly ModulebankDataContext _dataContext;

        // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
        public AccrueInterestHandler(ModulebankDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task Handle(Guid accountId)
        {
            await using var transaction = await _dataContext.Database.BeginTransactionAsync();
            try
            {
                await _dataContext.Database.ExecuteSqlRawAsync(
                    "CALL accrue_interest({0})",
                    accountId);

                await transaction.CommitAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == "P0001")
            {
                await transaction.RollbackAsync();
                Console.WriteLine(ex.MessageText);
            }
            catch
            {
                await transaction.RollbackAsync();
            }
        }
    }
}
