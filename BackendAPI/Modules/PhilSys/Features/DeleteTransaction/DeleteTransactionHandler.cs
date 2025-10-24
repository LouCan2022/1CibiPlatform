namespace PhilSys.Features.DeleteTransaction;

public record DeleteTransactionCommand(string HashToken) : ICommand<DeleteTransactionResult>;
public record DeleteTransactionResult(bool IsDeleted);
public class DeleteTransactionHandler : ICommandHandler<DeleteTransactionCommand, DeleteTransactionResult>
{
	private readonly ILogger<DeleteTransactionHandler> _logger;
	private readonly DeleteTransactionService _deleteTransactionService;

	public DeleteTransactionHandler(ILogger<DeleteTransactionHandler> logger,
		DeleteTransactionService deleteTransactionService)
	{
		_logger = logger;
		_deleteTransactionService = deleteTransactionService;
	}
	public async Task<DeleteTransactionResult> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
	{
		var deletedTransaction = await _deleteTransactionService.DeleteTransactionAsync(request.HashToken);

		return new DeleteTransactionResult(deletedTransaction);
	}
}
