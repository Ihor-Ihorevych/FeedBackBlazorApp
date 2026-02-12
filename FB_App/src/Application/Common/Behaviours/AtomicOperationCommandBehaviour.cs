using FB_App.Application.Common.Extensions;
using FB_App.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace FB_App.Application.Common.Behaviours;
public class AtomicOperationCommandBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<AtomicOperationCommandBehaviour<TRequest, TResponse>> _logger;
    private readonly IApplicationDbContext _context;
    private const string CommandSuffix_ = "Command";
    private const string ErrorMessage_ = "Error occurred during atomic operation. Transaction has been rolled back. Errors: {Message}";
    public AtomicOperationCommandBehaviour(ILogger<AtomicOperationCommandBehaviour<TRequest, TResponse>> logger,
                                     IApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<TResponse> Handle(TRequest request,
                                    RequestHandlerDelegate<TResponse> next,
                                    CancellationToken cancellationToken)
    {
        if (!typeof(TRequest).Name.EndsWith(CommandSuffix_))
            return await next(cancellationToken);

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await next(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogIfLevel(LogLevel.Error, ErrorMessage_, ex.Message);
            throw;
        }
    }
}
