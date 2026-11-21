using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class ProcessorInfoProvider : IProcessorInfoProvider
{
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;

	public ProcessorInfoProvider(IDataFlowExecutionContextProvider dataFlowExecutionContextProvider)
	{
		_dataFlowExecutionContextProvider = dataFlowExecutionContextProvider ??
			throw new ArgumentNullException(nameof(dataFlowExecutionContextProvider));
	}

	public async Task<ProcessorInfo> GetProcessorInfoAsync(DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken = default)
	{
		DataFlowExecutionContext dataFlowExecutionContext = await _dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(dataFlowExecutionId, cancellationToken)
			.ConfigureAwait(false);

		return dataFlowExecutionContext.ProcessorInfo;
	}
}
