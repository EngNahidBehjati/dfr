using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Dto;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class DataFlowExecutionFacade : IDataFlowExecutionFacade
{
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly IDataFlowExecutor _dataFlowExecutor;
	private readonly IDataFlowInvalidator _dataFlowInvalidator;
	private readonly IDataFlowCanceller _dataFlowCanceller;
	private readonly IDataFlowVersionConflictHandler _dataFlowVersionConflictHandler;

	public DataFlowExecutionFacade(IDataFlowExecutionContextProvider dataFlowExecutionContextProvider,
		IDataFlowExecutor dataFlowExecutor,
		IDataFlowInvalidator dataFlowInvalidator,
		IDataFlowCanceller dataFlowCanceller,
		IDataFlowVersionConflictHandler dataFlowVersionConflictHandler)
	{
		_dataFlowExecutionContextProvider = dataFlowExecutionContextProvider ?? throw new ArgumentNullException(nameof(dataFlowExecutionContextProvider));
		_dataFlowExecutor = dataFlowExecutor ?? throw new ArgumentNullException(nameof(dataFlowExecutor));
		_dataFlowInvalidator = dataFlowInvalidator ?? throw new ArgumentNullException(nameof(dataFlowInvalidator));
		_dataFlowCanceller = dataFlowCanceller ?? throw new ArgumentNullException(nameof(dataFlowCanceller));
		_dataFlowVersionConflictHandler = dataFlowVersionConflictHandler ?? throw new ArgumentNullException(nameof(dataFlowVersionConflictHandler));
	}

	public async Task RunDataFlow(Guid dataFlowExecutionId,
		long dataFlowVersion,
		RunDataFlowDto runDataFlowDto,
		CancellationToken cancellationToken)
	{
		var flowExecutionId = new DataFlowExecutionId(dataFlowExecutionId);
		var flowVersion = new DataFlowVersion(dataFlowVersion);

		await _dataFlowVersionConflictHandler
			.HandleVersionConflictAsync(flowExecutionId, flowVersion, cancellationToken)
			.ConfigureAwait(false);

		DataFlowExecutionContext dataFlowExecutionContext = await _dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(flowExecutionId, cancellationToken)
			.ConfigureAwait(false);

		var dynamicParameters = runDataFlowDto.DynamicParameters
			.Select(parameter => new DynamicParameter { Name = parameter.Name, Value = parameter.Value })
			.ToList();

		await _dataFlowExecutor
			.RunDataFlowAsync(dataFlowExecutionContext, dynamicParameters, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task RunDataFlowNode(Guid dataFlowExecutionId, Guid nodeId, long dataFlowVersion,
		RunDataFlowNodeDto runDataFlowNodeDto,
		CancellationToken cancellationToken)
	{
		var executionId = new DataFlowExecutionId(dataFlowExecutionId);
		var version = new DataFlowVersion(dataFlowVersion);

		await _dataFlowVersionConflictHandler.HandleVersionConflictAsync(executionId, version, cancellationToken)
			.ConfigureAwait(false);

		DataFlowExecutionContext dataFlowExecutionContext = await _dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(executionId, cancellationToken)
			.ConfigureAwait(false);

		var dynamicParameters = runDataFlowNodeDto.DynamicParameters
			.Select(parameter => new DynamicParameter { Name = parameter.Name, Value = parameter.Value })
			.ToList();

		await _dataFlowExecutor
			.RunNodeAsync(dataFlowExecutionContext, new NodeId(nodeId), dynamicParameters, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task CancelDataFlow(Guid dataFlowExecutionId, CancellationToken cancellationToken)
	{
		if (_dataFlowExecutionContextProvider.TryGetRunningDataFlowExecutionContext(
			    new DataFlowExecutionId(dataFlowExecutionId), out DataFlowExecutionContext? dataFlowExecutionContext))
		{
			await _dataFlowCanceller.CancelDataFlowAsync(dataFlowExecutionContext, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	public async Task CancelDataFlowNode(Guid dataFlowExecutionId, Guid nodeId, CancellationToken cancellationToken)
	{
		if (_dataFlowExecutionContextProvider.TryGetRunningDataFlowExecutionContext(
			    new DataFlowExecutionId(dataFlowExecutionId), out DataFlowExecutionContext? dataFlowExecutionContext))
		{
			await _dataFlowCanceller
				.CancelNodeAndDownstreamAsync(dataFlowExecutionContext, new NodeId(nodeId), cancellationToken)
				.ConfigureAwait(false);
		}
	}

	public async Task InvalidateDataFlow(Guid dataFlowExecutionId, CancellationToken cancellationToken)
	{
		if (_dataFlowExecutionContextProvider.TryGetRunningDataFlowExecutionContext(
			    new DataFlowExecutionId(dataFlowExecutionId), out DataFlowExecutionContext? dataFlowExecutionContext))
		{
			await _dataFlowInvalidator.InvalidateDataFlowAsync(dataFlowExecutionContext, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	public async Task InvalidateDataFlowNode(Guid dataFlowExecutionId, Guid nodeId, CancellationToken cancellationToken)
	{
		if (_dataFlowExecutionContextProvider.TryGetRunningDataFlowExecutionContext(
			    new DataFlowExecutionId(dataFlowExecutionId), out DataFlowExecutionContext? dataFlowExecutionContext))
		{
			await _dataFlowInvalidator
				.InvalidateNodeAndDownstreamAsync(dataFlowExecutionContext, new NodeId(nodeId), cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
