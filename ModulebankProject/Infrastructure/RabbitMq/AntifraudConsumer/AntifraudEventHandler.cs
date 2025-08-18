using Microsoft.EntityFrameworkCore;
using ModulebankProject.Infrastructure.Data;

namespace ModulebankProject.Infrastructure.RabbitMq.AntifraudConsumer;

public class AntifraudEventHandler : IAntifraudEventHandler
{
    private readonly ModulebankDataContext _dbContext;
    private readonly ILogger<AntifraudEventHandler> _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public AntifraudEventHandler(ModulebankDataContext dbContext, ILogger<AntifraudEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(Guid clientId, bool isFreeze, CancellationToken cancellationToken)
    {
        var accounts = await _dbContext.Accounts
            .Where(x => x.OwnerId == clientId)
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var account in accounts)
        {
            account.Freezing = isFreeze;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Accounts of client " + clientId + " freezing is " + isFreeze);
    }
}