using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;

public sealed class NodeAncestryParameterValueExtractorTests
{
	private readonly IDataFlowDynamicParametersStore _dataFlowDynamicParametersStore;
	private readonly IDataFlowExecutionSequenceStore _dataFlowExecutionSequenceStore;
	private readonly NodeAncestryParameterValueExtractor _sut;

	public NodeAncestryParameterValueExtractorTests()
	{
		_dataFlowDynamicParametersStore = Substitute.For<IDataFlowDynamicParametersStore>();
		_dataFlowExecutionSequenceStore = Substitute.For<IDataFlowExecutionSequenceStore>();
		_sut = new NodeAncestryParameterValueExtractor(_dataFlowDynamicParametersStore, _dataFlowExecutionSequenceStore);
	}

	// ── Topology helpers ──────────────────────────────────────────────────────

	/// <summary>
	/// A → B → C
	/// </summary>
	private static IReadOnlyGraph LinearGraph(NodeId a, NodeId b, NodeId c)
	{
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		graph.GetAllEdges().Returns(
		[
			Edge(a, b),
			Edge(b, c)
		]);
		return graph;
	}

	/// <summary>
	/// A → C
	/// B → C   (diamond without shared grandparent)
	/// </summary>
	private static IReadOnlyGraph DiamondGraph(NodeId a, NodeId b, NodeId c)
	{
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		graph.GetAllEdges().Returns(
		[
			Edge(a, c),
			Edge(b, c)
		]);
		return graph;
	}

	private static EdgeByNode Edge(NodeId from, NodeId to) =>
		new() { Id = new EdgeId(Guid.NewGuid()), FromNodeId = from, ToNodeId = to };

	private void SetupParams(DataFlowExecutionId executionId, NodeId nodeId, params DynamicParameter[] parameters) =>
		_dataFlowDynamicParametersStore
			.TryGetDynamicParameters(executionId, nodeId, out Arg.Any<IReadOnlyCollection<DynamicParameter>>())
			.Returns(callInfo =>
			{
				callInfo[2] = parameters;
				return true;
			});

	private void SetupWriteSequence(DataFlowExecutionId executionId, NodeId nodeId, long sequence) =>
		_dataFlowExecutionSequenceStore.GetNodeRunSequence(executionId, nodeId).Returns(sequence);

	// ── Tests ─────────────────────────────────────────────────────────────────

	[Fact]
	public void Extract_ShouldReturnNodeOwnParameters_WhenNodeHasAllParameters()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		graph.GetAllEdges().Returns([Edge(nodeA, nodeB)]);

		SetupParams(executionId, nodeB, new DynamicParameter { Name = "param1", Value = "own-value" });
		SetupParams(executionId, nodeA, new DynamicParameter { Name = "param1", Value = "parent-value" });

		// Act
		IReadOnlyCollection<DynamicParameter> result = _sut.Extract(executionId, nodeB, graph, ["param1"]);

		// Assert: own value wins over parent's
		result.Should().ContainSingle(p => p.Name == "param1" && Equals(p.Value, "own-value"));
	}

	[Fact]
	public void Extract_ShouldFallBackToDirectParent_WhenNodeDoesNotHaveParameter()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		graph.GetAllEdges().Returns([Edge(nodeA, nodeB)]);

		SetupParams(executionId, nodeB);
		SetupParams(executionId, nodeA, new DynamicParameter { Name = "param1", Value = "parent-value" });

		// Act
		IReadOnlyCollection<DynamicParameter> result = _sut.Extract(executionId, nodeB, graph, ["param1"]);

		// Assert
		result.Should().ContainSingle(p => p.Name == "param1" && Equals(p.Value, "parent-value"));
	}

	[Fact]
	public void Extract_ShouldFallBackToGrandparent_WhenNeitherNodeNorParentHaveParameter()
	{
		// Arrange: A → B → C, only A has param1
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		var nodeC = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = LinearGraph(nodeA, nodeB, nodeC);

		SetupParams(executionId, nodeC);
		SetupParams(executionId, nodeB);
		SetupParams(executionId, nodeA, new DynamicParameter { Name = "param1", Value = "grandparent-value" });

		// Act
		IReadOnlyCollection<DynamicParameter> result = _sut.Extract(executionId, nodeC, graph, ["param1"]);

		// Assert
		result.Should().ContainSingle(p => p.Name == "param1" && Equals(p.Value, "grandparent-value"));
	}

	[Fact]
	public void Extract_ShouldStopAtClosestAncestor_WhenBothParentAndGrandparentHaveParameter()
	{
		// Arrange: A → B → C; both A and B have param1
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		var nodeC = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = LinearGraph(nodeA, nodeB, nodeC);

		SetupParams(executionId, nodeC);
		SetupParams(executionId, nodeB, new DynamicParameter { Name = "param1", Value = "parent-value" });
		SetupParams(executionId, nodeA, new DynamicParameter { Name = "param1", Value = "grandparent-value" });

		// Act
		IReadOnlyCollection<DynamicParameter> result = _sut.Extract(executionId, nodeC, graph, ["param1"]);

		// Assert: parent is closer → its value wins regardless of write sequence
		result.Should().ContainSingle(p => p.Name == "param1" && Equals(p.Value, "parent-value"));
	}

	[Fact]
	public void Extract_ShouldReturnEmpty_WhenNoAncestorHasParameters()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		graph.GetAllEdges().Returns([Edge(nodeA, nodeB)]);

		SetupParams(executionId, nodeA);
		SetupParams(executionId, nodeB);

		// Act
		IReadOnlyCollection<DynamicParameter> result = _sut.Extract(executionId, nodeB, graph, ["param1"]);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void Extract_ShouldPickLastWrittenParent_WhenTwoParentsHaveConflictingValues()
	{
		// Arrange: A → C, B → C; both have param1; B was submitted last
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		var nodeC = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = DiamondGraph(nodeA, nodeB, nodeC);

		SetupParams(executionId, nodeC);
		SetupParams(executionId, nodeA, new DynamicParameter { Name = "param1", Value = "value-from-A" });
		SetupParams(executionId, nodeB, new DynamicParameter { Name = "param1", Value = "value-from-B" });

		SetupWriteSequence(executionId, nodeA, 1); // A submitted first
		SetupWriteSequence(executionId, nodeB, 2); // B submitted last → B wins

		// Act
		IReadOnlyCollection<DynamicParameter> result = _sut.Extract(executionId, nodeC, graph, ["param1"]);

		// Assert
		result.Should().ContainSingle(p => p.Name == "param1" && Equals(p.Value, "value-from-B"));
	}

	[Fact]
	public void Extract_ShouldPickSubmittedParent_WhenOneParentSubmittedAndOtherHasNotYet()
	{
		// Arrange: A submitted (sequence > 0); B not yet submitted (sequence = 0)
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		var nodeC = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = DiamondGraph(nodeA, nodeB, nodeC);

		SetupParams(executionId, nodeC);
		SetupParams(executionId, nodeA, new DynamicParameter { Name = "param1", Value = "value-from-A" });
		SetupParams(executionId, nodeB, new DynamicParameter { Name = "param1", Value = "value-from-B" });

		SetupWriteSequence(executionId, nodeA, 5); // A submitted
		SetupWriteSequence(executionId, nodeB, 0); // B not yet submitted → A wins

		// Act
		IReadOnlyCollection<DynamicParameter> result = _sut.Extract(executionId, nodeC, graph, ["param1"]);

		// Assert
		result.Should().ContainSingle(p => p.Name == "param1" && Equals(p.Value, "value-from-A"));
	}

	[Fact]
	public void Extract_ShouldMergeParametersFromDifferentLevels_WhenEachLevelHasDifferentNames()
	{
		// Arrange: A → B → C; C has param-c; B has param-b; A has param-a
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		var nodeC = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = LinearGraph(nodeA, nodeB, nodeC);

		SetupParams(executionId, nodeC, new DynamicParameter { Name = "param-c", Value = "c" });
		SetupParams(executionId, nodeB, new DynamicParameter { Name = "param-b", Value = "b" });
		SetupParams(executionId, nodeA, new DynamicParameter { Name = "param-a", Value = "a" });

		// Act
		IReadOnlyCollection<DynamicParameter> result = _sut.Extract(executionId, nodeC, graph, ["param-c", "param-b", "param-a"]);

		// Assert: all three params collected from their respective levels
		result.Should().HaveCount(3);
		result.Should().Contain(p => p.Name == "param-c" && Equals(p.Value, "c"));
		result.Should().Contain(p => p.Name == "param-b" && Equals(p.Value, "b"));
		result.Should().Contain(p => p.Name == "param-a" && Equals(p.Value, "a"));
	}

	[Fact]
	public void Extract_ShouldReturnEmpty_WhenExpectedParameterNamesIsEmpty()
	{
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		var nodeC = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = LinearGraph(nodeA, nodeB, nodeC);

		SetupParams(executionId, nodeC, new DynamicParameter { Name = "param-c", Value = "c" });
		SetupParams(executionId, nodeB, new DynamicParameter { Name = "param-b", Value = "b" });
		SetupParams(executionId, nodeA, new DynamicParameter { Name = "param-a", Value = "a" });

		IReadOnlyCollection<DynamicParameter> result = _sut.Extract(executionId, nodeC, graph, []);

		result.Should().BeEmpty();
		_dataFlowDynamicParametersStore.DidNotReceive()
			.TryGetDynamicParameters(Arg.Any<DataFlowExecutionId>(), Arg.Any<NodeId>(),
				out Arg.Any<IReadOnlyCollection<DynamicParameter>>());
	}

	[Fact]
	public void Extract_ShouldNotVisitSameNodeTwice_WhenGraphHasConvergingPaths()
	{
		// Arrange: A → B → D, A → C → D  (shared grandparent A)
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		var nodeC = new NodeId(Guid.NewGuid());
		var nodeD = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		graph.GetAllEdges().Returns(
		[
			Edge(nodeA, nodeB),
			Edge(nodeA, nodeC),
			Edge(nodeB, nodeD),
			Edge(nodeC, nodeD)
		]);

		SetupParams(executionId, nodeD);
		SetupParams(executionId, nodeB);
		SetupParams(executionId, nodeC);
		SetupParams(executionId, nodeA, new DynamicParameter { Name = "param1", Value = "a-value" });

		// Act
		IReadOnlyCollection<DynamicParameter> result = _sut.Extract(executionId, nodeD, graph, ["param1"]);

		// Assert: param1 found from A exactly once despite two convergent paths
		result.Should().ContainSingle(p => p.Name == "param1" && Equals(p.Value, "a-value"));
		_dataFlowDynamicParametersStore.Received(1)
			.TryGetDynamicParameters(executionId, nodeA, out Arg.Any<IReadOnlyCollection<DynamicParameter>>());
	}

	[Fact]
	public void Extract_ShouldReturnNodeOwnParameter_WhenNodeIsARootWithNoParents()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		graph.GetAllEdges().Returns([]);

		SetupParams(executionId, nodeId, new DynamicParameter { Name = "param1", Value = "root-value" });

		// Act
		IReadOnlyCollection<DynamicParameter> result = _sut.Extract(executionId, nodeId, graph, ["param1"]);

		// Assert
		result.Should().ContainSingle(p => p.Name == "param1" && Equals(p.Value, "root-value"));
	}

	[Fact]
	public void Extract_ShouldNotVisitGrandparent_WhenAllExpectedParametersFoundAtParentLevel()
	{
		// Arrange: A → B → C; B has the only expected parameter; A has an unrelated parameter.
		// With expectedParameterNames = ["param-b"], BFS must stop after level 1 (B) without querying A.
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		var nodeC = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = LinearGraph(nodeA, nodeB, nodeC);

		SetupParams(executionId, nodeC);
		SetupParams(executionId, nodeB, new DynamicParameter { Name = "param-b", Value = "b-value" });
		SetupParams(executionId, nodeA, new DynamicParameter { Name = "unrelated", Value = "a-value" });

		// Act
		IReadOnlyCollection<DynamicParameter> result = _sut.Extract(executionId, nodeC, graph, ["param-b"]);

		// Assert: expected parameter Extractd from B; A was never visited
		result.Should().ContainSingle(p => p.Name == "param-b" && Equals(p.Value, "b-value"));
		_dataFlowDynamicParametersStore.DidNotReceive()
			.TryGetDynamicParameters(executionId, nodeA, out Arg.Any<IReadOnlyCollection<DynamicParameter>>());
	}
}
