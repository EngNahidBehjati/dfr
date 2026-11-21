using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowSnapshot.Abstractions;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.Tests.Common;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public class DataFlowExecutionContextProviderTests
{
	private readonly IDataFlowExecutionContextContainer _dataFlowExecutionContextContainer;
	private readonly IDataFlowExecutionContextCreator _dataFlowExecutionContextCreator;
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly DataFlowExecutionContextProvider _sut;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public DataFlowExecutionContextProviderTests()
	{
		_nodeExecutionStateUpdater = Substitute.For<INodeExecutionStateUpdater>();
		_dataFlowExecutionContextContainer = Substitute.For<IDataFlowExecutionContextContainer>();
		_dataFlowExecutionContextCreator = Substitute.For<IDataFlowExecutionContextCreator>();
		_languageDictionaryProvider =  LanguageDictionaryProvider.Instance;
		_sut = new DataFlowExecutionContextProvider(
			_dataFlowExecutionContextContainer, _dataFlowExecutionContextCreator
			, _nodeExecutionStateUpdater, _languageDictionaryProvider);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowExecutionContextContainerIsNull()
	{
		// Arrange
		IDataFlowExecutionContextContainer dataFlowExecutionContextContainer = null!;

		// Act
		Action act = () =>
		{
			_ = new DataFlowExecutionContextProvider(
				dataFlowExecutionContextContainer, _dataFlowExecutionContextCreator,
				_nodeExecutionStateUpdater, _languageDictionaryProvider);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowExecutionContextContainer");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowExecutionContextCreatorIsNull()
	{
		// Arrange
		IDataFlowExecutionContextCreator dataFlowExecutionContextCreator = null!;

		// Act
		Action act = () =>
		{
			_ = new DataFlowExecutionContextProvider(
				_dataFlowExecutionContextContainer, dataFlowExecutionContextCreator,
				_nodeExecutionStateUpdater, _languageDictionaryProvider);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowExecutionContextCreator");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeExecutionStateUpdaterIsNull()
	{
		// Arrange
		INodeExecutionStateUpdater nodeExecutionStateUpdater = null!;

		// Act
		Action act = () =>
		{
			_ = new DataFlowExecutionContextProvider(
				_dataFlowExecutionContextContainer, _dataFlowExecutionContextCreator
				, nodeExecutionStateUpdater, _languageDictionaryProvider);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("nodeExecutionStateUpdater");
	}

	[Fact]
	public async Task UpdateDataFlowExecutionContextAsync_ShouldUpdateNodeStatesThenContext_WhenCalled()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());

		IGraph newGraph = Substitute.For<IGraph>();
		var dataFlowExecutionContext = new DataFlowExecutionContext
		{
			Graph = newGraph,
			DataFlowExecutionId = dataFlowExecutionId,
			DataFlowVersion = new DataFlowVersion(21),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		var nodes = new List<Node>
		{
			new() { Id = NodeId.New() },
			new() { Id = NodeId.New() }
		};
		newGraph.GetAllNodes().Returns(nodes);
		_dataFlowExecutionContextCreator
			.CreateDataFlowExecutionContextAsync(dataFlowExecutionId.Value, CancellationToken.None, 21)
			.Returns(dataFlowExecutionContext);

		// Act
		await _sut.UpdateDataFlowExecutionContextAsync(dataFlowExecutionId, CancellationToken.None,
				new DataFlowVersion(21));

		// Assert

		// update node states before context to make sure all states in the running context are already added
		Received.InOrder(() =>
		{
			foreach (Node node in nodes)
			{
				_nodeExecutionStateUpdater.TryAddNodeExecutionState(dataFlowExecutionId, node.Id);
			}

			_dataFlowExecutionContextContainer.AddOrUpdateDataFlowExecutionContext(dataFlowExecutionContext);
		});
	}

	[Fact]
	public async Task UpdateDataFlowExecutionContextAsync_ShouldUpdateContextWithNullVersion_WhenVersionNotProvided()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());

		IGraph newGraph = CreateGraph();
		var dataFlowExecutionContext = new DataFlowExecutionContext
		{
			Graph = newGraph,
			DataFlowExecutionId = dataFlowExecutionId,
			DataFlowVersion = new DataFlowVersion(21),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		_dataFlowExecutionContextCreator
			.CreateDataFlowExecutionContextAsync(dataFlowExecutionId.Value, CancellationToken.None)
			.Returns(dataFlowExecutionContext);

		// Act
		await _sut.UpdateDataFlowExecutionContextAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		_dataFlowExecutionContextContainer.Received(1).AddOrUpdateDataFlowExecutionContext(dataFlowExecutionContext);
	}

	[Fact]
	public async Task GetRunningDataFlowExecutionContextAsync_ShouldReturnContext_WhenContextExists()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		IGraph graph = Substitute.For<IGraph>();
		var executionContext = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = dataFlowExecutionId,
			DataFlowVersion = new DataFlowVersion(20),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		_dataFlowExecutionContextContainer.TryGetDataFlowExecutionContext(dataFlowExecutionId,
				out Arg.Any<DataFlowExecutionContext?>())
			.Returns(callInfo =>
			{
				callInfo[1] = executionContext;
				return true;
			});

		// Act
		DataFlowExecutionContext result =
			await _sut.GetRunningDataFlowExecutionContextAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(executionContext);
	}

	[Fact]
	public async Task GetRunningDataFlowExecutionContextAsync_ShouldThrowDataFlowExecutionNotFoundException_WhenContextDoesNotExist()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());

		_dataFlowExecutionContextContainer.TryGetDataFlowExecutionContext(dataFlowExecutionId,
				out Arg.Any<DataFlowExecutionContext?>())
			.Returns(callInfo =>
			{
				callInfo[1] = null;
				return false;
			});

		// Act
		Func<Task<DataFlowExecutionContext>> act = () =>
			_sut.GetRunningDataFlowExecutionContextAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DataFlowExecutionNotFoundException>();
	}

	[Fact]
	public void TryGetRunningDataFlowExecutionContextAsync_ShouldReturnTrueAndFillContext_WhenContextExists()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var executionContext = new DataFlowExecutionContext
		{
			Graph = Substitute.For<IGraph>(),
			DataFlowExecutionId = dataFlowExecutionId,
			DataFlowVersion = new DataFlowVersion(20),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		_dataFlowExecutionContextContainer.TryGetDataFlowExecutionContext(dataFlowExecutionId,
				out Arg.Any<DataFlowExecutionContext?>())
			.Returns(callInfo =>
			{
				callInfo[1] = executionContext;
				return true;
			});

		// Act
		var result = _sut.TryGetRunningDataFlowExecutionContext(dataFlowExecutionId,
			out DataFlowExecutionContext? dataFlowExecutionContext);

		// Assert
		result.Should().BeTrue();
		dataFlowExecutionContext.Should().BeEquivalentTo(executionContext);
	}

	[Fact]
	public void TryGetRunningDataFlowExecutionContextAsync_ShouldReturnFalse_WhenContextDoesNotExist()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());

		_dataFlowExecutionContextContainer.TryGetDataFlowExecutionContext(dataFlowExecutionId,
				out Arg.Any<DataFlowExecutionContext?>())
			.Returns(callInfo =>
			{
				callInfo[1] = null;
				return false;
			});

		// Act
		var result = _sut.TryGetRunningDataFlowExecutionContext(dataFlowExecutionId,
			out DataFlowExecutionContext? dataFlowExecutionContext);

		// Assert
		result.Should().BeFalse();
		dataFlowExecutionContext.Should().BeNull();
	}

	private static IGraph CreateGraph()
	{
		IGraph graph = Substitute.For<IGraph>();
		HashSet<Node> graphNodes =
		[
			new()
			{
				Id = new NodeId(Guid.NewGuid())
			},
			new()
			{
				Id = new NodeId(Guid.NewGuid())
			},
			new()
			{
				Id = new NodeId(Guid.NewGuid())
			}
		];

		graph.GetAllNodes().Returns(graphNodes);
		return graph;
	}
}
