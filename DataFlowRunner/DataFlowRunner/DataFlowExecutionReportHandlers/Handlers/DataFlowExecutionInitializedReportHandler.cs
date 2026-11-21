using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;

namespace DataFlowRunner.DataFlowExecutionReportHandlers.Handlers;

internal sealed class DataFlowExecutionInitializedReportHandler : IDataFlowExecutionInitializedReportHandler
{
	private readonly INodeExecutorEngineGraphInitializationTracker _nodeExecutorEngineGraphInitializationTracker;

	public DataFlowExecutionInitializedReportHandler(INodeExecutorEngineGraphInitializationTracker nodeExecutorEngineGraphInitializationTracker)
	{
		_nodeExecutorEngineGraphInitializationTracker = nodeExecutorEngineGraphInitializationTracker ?? throw new ArgumentNullException(nameof(nodeExecutorEngineGraphInitializationTracker));
	}

	public Task HandleAsync(DataFlowExecutionInitializedReport report, CancellationToken cancellationToken)
	{
		if (report.IsSucceeded)
		{
			var dataFlowExecutionId = new DataFlowExecutionId(report.GraphExecutionId);
			_nodeExecutorEngineGraphInitializationTracker.TrackDataFlowInitialization(dataFlowExecutionId);
		}

		return Task.CompletedTask;
	}
}
