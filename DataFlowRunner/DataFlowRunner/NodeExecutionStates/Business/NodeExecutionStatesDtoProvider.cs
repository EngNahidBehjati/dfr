using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Business;

internal sealed class NodeExecutionStatesDtoProvider : INodeExecutionStatesDtoProvider
{
	private readonly INodeExecutionStateProvider _nodeExecutionStateProvider;
	private readonly INodeExecutionStateMapper _nodeExecutionStateMapper;
	private readonly INodeExecutionTimingStore _nodeExecutionTimingStore;
	private readonly INodeExecutionDurationResolver _nodeExecutionDurationResolver;

	public NodeExecutionStatesDtoProvider(INodeExecutionStateProvider nodeExecutionStateProvider,
		INodeExecutionStateMapper nodeExecutionStateMapper, INodeExecutionTimingStore nodeExecutionTimingStore,
		INodeExecutionDurationResolver nodeExecutionDurationResolver)
	{
		_nodeExecutionStateProvider = nodeExecutionStateProvider ?? throw new ArgumentNullException(nameof(nodeExecutionStateProvider));
		_nodeExecutionStateMapper = nodeExecutionStateMapper ?? throw new ArgumentNullException(nameof(nodeExecutionStateMapper));
		_nodeExecutionTimingStore = nodeExecutionTimingStore ?? throw new ArgumentNullException(nameof(nodeExecutionTimingStore));
		_nodeExecutionDurationResolver = nodeExecutionDurationResolver ?? throw new ArgumentNullException(nameof(nodeExecutionDurationResolver));
	}

	public Task<DataFlowNodeExecutionStatesDto> GetAllNodeExecutionStatesAsync(Guid dataFlowExecutionId,
		CancellationToken cancellationToken)
	{
		var executionId = new DataFlowExecutionId(dataFlowExecutionId);
		IReadOnlyDictionary<NodeId, NodeExecutionState> nodeStatesById = _nodeExecutionStateProvider
			.GetAllNodeExecutionStates(executionId);

		var stateDto = nodeStatesById
			.Select(stateById =>
				CreateNodeExecutionStateById(executionId, stateById.Key, stateById.Value))
			.ToList();

		return Task.FromResult(new DataFlowNodeExecutionStatesDto(stateDto));
	}

	private NodeExecutionStateByIdDto CreateNodeExecutionStateById(DataFlowExecutionId dataFlowExecutionId,
		NodeId nodeId, NodeExecutionState nodeExecutionState)
	{
		NodeExecutionTiming nodeExecutionTiming = _nodeExecutionTimingStore
			.GetNodeExecutionTime(dataFlowExecutionId, nodeId);

		return new NodeExecutionStateByIdDto
		{
			NodeId = nodeId.Value,
			NodeExecutionStateDto =
				_nodeExecutionStateMapper.NodeExecutionStateToDto(nodeExecutionState),
			DurationMilliseconds = _nodeExecutionDurationResolver.ResolveDurationMilliseconds(nodeExecutionTiming)
		};
	}
}

