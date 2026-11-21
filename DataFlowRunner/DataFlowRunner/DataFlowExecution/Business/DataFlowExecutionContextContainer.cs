using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class DataFlowExecutionContextContainer : IDataFlowExecutionContextContainer
{
	private readonly ConcurrentDictionary<DataFlowExecutionId, DataFlowExecutionContext> _dataFlowExecutionContexts = new();

	public bool TryGetDataFlowExecutionContext(DataFlowExecutionId dataFlowExecutionId,
		[NotNullWhen(true)] out DataFlowExecutionContext? dataFlow)
	{
		return _dataFlowExecutionContexts.TryGetValue(dataFlowExecutionId, out dataFlow);
	}

	public void AddOrUpdateDataFlowExecutionContext(DataFlowExecutionContext dataFlowExecutionContext)
	{
		_dataFlowExecutionContexts[dataFlowExecutionContext.DataFlowExecutionId] = dataFlowExecutionContext;
	}
}
