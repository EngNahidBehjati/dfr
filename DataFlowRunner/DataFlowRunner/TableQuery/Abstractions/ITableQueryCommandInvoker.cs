namespace DataFlowRunner.TableQuery.Abstractions;

internal interface ITableQueryCommandInvoker<in TCommand, TReport>
	where TCommand : IQueryCommand
	where TReport : IQueryReport
{
	Task<TReport> InvokeQueryCommandAsync(
		TCommand command,
		CancellationToken cancellationToken);
}
