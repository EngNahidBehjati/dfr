using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowDynamicParameters.Business.Store;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Business.Store;

public sealed class DynamicParametersStoreUpdaterTests
{
	private readonly IDataFlowDynamicParametersStore _dataFlowDynamicParametersStore;
	private readonly IDataFlowExecutionSequenceStore _dataFlowExecutionSequenceStore;
	private readonly DynamicParametersStoreUpdater _sut;

	public DynamicParametersStoreUpdaterTests()
	{
		_dataFlowDynamicParametersStore = Substitute.For<IDataFlowDynamicParametersStore>();
		_dataFlowExecutionSequenceStore = Substitute.For<IDataFlowExecutionSequenceStore>();
		_sut = new DynamicParametersStoreUpdater(
			_dataFlowDynamicParametersStore,
			_dataFlowExecutionSequenceStore);
	}

	[Fact]
	public void UpdateDataFlowDynamicParameters_ShouldPersistPerNodeParametersAndSequence_WhenCalled()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var node1Id = new NodeId(Guid.NewGuid());
		var node2Id = new NodeId(Guid.NewGuid());
		var node1Parameters = new List<DynamicParameter> { new() { Name = "param1", Value = "value1" } };
		var node2Parameters = new List<DynamicParameter> { new() { Name = "param2", Value = 123 } };
		var parametersByNode = new Dictionary<NodeId, List<DynamicParameter>>
		{
			[node1Id] = node1Parameters,
			[node2Id] = node2Parameters
		};

		// Act
		_sut.UpdateDataFlowDynamicParameters(executionId, parametersByNode);

		// Assert
		_dataFlowDynamicParametersStore.Received(1).SetDynamicParameters(
			executionId,
			Arg.Is<IReadOnlyCollection<NodeId>>(ids => ids.Single() == node1Id),
			node1Parameters);
		_dataFlowDynamicParametersStore.Received(1).SetDynamicParameters(
			executionId,
			Arg.Is<IReadOnlyCollection<NodeId>>(ids => ids.Single() == node2Id),
			node2Parameters);
		_dataFlowExecutionSequenceStore.Received(1).SetNextRunSequence(
			executionId,
			Arg.Is<IReadOnlyCollection<NodeId>>(ids =>
				ids.Count == 2 && ids.Contains(node1Id) && ids.Contains(node2Id)));
	}

	[Fact]
	public void UpdateDataFlowDynamicParameters_ShouldPersistEmptyList_WhenNodeHasEmptyParameters()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var parametersByNode = new Dictionary<NodeId, List<DynamicParameter>>
		{
			[nodeId] = []
		};

		// Act
		_sut.UpdateDataFlowDynamicParameters(executionId, parametersByNode);

		// Assert
		_dataFlowDynamicParametersStore.Received(1).SetDynamicParameters(
			executionId,
			Arg.Is<IReadOnlyCollection<NodeId>>(ids => ids.Single() == nodeId),
			Arg.Is<List<DynamicParameter>>(parameters => parameters.Count == 0));
	}
}
