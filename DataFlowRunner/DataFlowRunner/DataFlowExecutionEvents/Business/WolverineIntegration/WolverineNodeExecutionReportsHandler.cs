using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Reports;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;

namespace DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration;

public sealed class WolverineNodeExecutionReportsHandler
{
	private readonly IDataFlowExecutionInitializedReportHandler _dataFlowExecutionInitializedReportHandler;
	private readonly INodeExecutionSucceededReportHandler _nodeExecutionSucceededReportHandler;
	private readonly INodeExecutionFailedReportHandler _nodeExecutionFailedReportHandler;
	private readonly INodeExecutionCanceledReportHandler _nodeExecutionCanceledReportHandler;
	private readonly IGraphInitializationNotRequestedReportHandler _graphInitializationNotRequestedReportHandler;
	private readonly IWolverineExecutionReportMapper _executionReportMapper;

	public WolverineNodeExecutionReportsHandler(
		IDataFlowExecutionInitializedReportHandler dataFlowExecutionInitializedReportHandler,
		INodeExecutionSucceededReportHandler nodeExecutionSucceededReportHandler,
		INodeExecutionFailedReportHandler nodeExecutionFailedReportHandler,
		INodeExecutionCanceledReportHandler nodeExecutionCanceledReportHandler,
		IGraphInitializationNotRequestedReportHandler graphInitializationNotRequestedReportHandler,
		IWolverineExecutionReportMapper executionReportMapper)
	{
		ArgumentNullException.ThrowIfNull(dataFlowExecutionInitializedReportHandler);
		ArgumentNullException.ThrowIfNull(nodeExecutionSucceededReportHandler);
		ArgumentNullException.ThrowIfNull(nodeExecutionFailedReportHandler);
		ArgumentNullException.ThrowIfNull(nodeExecutionCanceledReportHandler);
		ArgumentNullException.ThrowIfNull(graphInitializationNotRequestedReportHandler);
		ArgumentNullException.ThrowIfNull(executionReportMapper);
		_dataFlowExecutionInitializedReportHandler = dataFlowExecutionInitializedReportHandler;
		_nodeExecutionSucceededReportHandler = nodeExecutionSucceededReportHandler;
		_nodeExecutionFailedReportHandler = nodeExecutionFailedReportHandler;
		_nodeExecutionCanceledReportHandler = nodeExecutionCanceledReportHandler;
		_graphInitializationNotRequestedReportHandler = graphInitializationNotRequestedReportHandler;
		_executionReportMapper = executionReportMapper;
	}

	public Task HandleAsync(
		DataFlowExecutionInitializedWolverineReport wolverineReport,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(wolverineReport);
		DataFlowExecutionInitializedReport report = _executionReportMapper.ToReport(wolverineReport);
		return _dataFlowExecutionInitializedReportHandler.HandleAsync(report, CancellationToken.None);
	}

	public Task HandleAsync(
		GraphInitializationNotRequestedWolverineReport wolverineReport,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(wolverineReport);
		GraphInitializationNotRequestedReport report = _executionReportMapper.ToReport(wolverineReport);
		return _graphInitializationNotRequestedReportHandler.HandleAsync(report, CancellationToken.None);
	}

	public Task HandleAsync(
		NodeExecutionSucceededWolverineReport wolverineReport,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(wolverineReport);
		NodeExecutionSucceededReport report = _executionReportMapper.ToReport(wolverineReport);
		return _nodeExecutionSucceededReportHandler.HandleAsync(report, CancellationToken.None);
	}

	public Task HandleAsync(
		NodeExecutionFailedWolverineReport wolverineReport,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(wolverineReport);
		NodeExecutionFailedReport report = _executionReportMapper.ToReport(wolverineReport);
		return _nodeExecutionFailedReportHandler.HandleAsync(report, CancellationToken.None);
	}

	public Task HandleAsync(
		NodeExecutionCanceledWolverineReport wolverineReport,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(wolverineReport);
		NodeExecutionCanceledReport report = _executionReportMapper.ToReport(wolverineReport);
		return _nodeExecutionCanceledReportHandler.HandleAsync(report, CancellationToken.None);
	}
}
