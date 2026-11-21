using DataFlowRunner.DataFlowDynamicParameters.Abstractions;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Dto;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Tests.Common;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public class DataFlowExecutionFacadeTests
{
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly IDataFlowExecutor _dataFlowExecutor;
	private readonly IDataFlowInvalidator _dataFlowInvalidator;
	private readonly IDataFlowCanceller _dataFlowCanceller;
	private readonly IDataFlowVersionConflictHandler _dataFlowVersionConflictHandler;
	private readonly DataFlowExecutionFacade _sut;

	public DataFlowExecutionFacadeTests()
	{
		_dataFlowExecutionContextProvider = Substitute.For<IDataFlowExecutionContextProvider>();
		_dataFlowExecutor = Substitute.For<IDataFlowExecutor>();
		_dataFlowInvalidator = Substitute.For<IDataFlowInvalidator>();
		_dataFlowCanceller = Substitute.For<IDataFlowCanceller>();
		_dataFlowVersionConflictHandler = Substitute.For<IDataFlowVersionConflictHandler>();
		_sut = new DataFlowExecutionFacade(_dataFlowExecutionContextProvider, _dataFlowExecutor, _dataFlowInvalidator,
			_dataFlowCanceller, _dataFlowVersionConflictHandler);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenContextProviderIsNull()
	{
		// Arrange
		IDataFlowExecutionContextProvider executionContextProvider = null!;

		// Act
		Action act = () =>
		{
			_ = new DataFlowExecutionFacade(executionContextProvider, _dataFlowExecutor, _dataFlowInvalidator,
				_dataFlowCanceller, _dataFlowVersionConflictHandler);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowExecutionContextProvider");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenExecutorIsNull()
	{
		// Arrange
		IDataFlowExecutor dataFlowExecutor = null!;

		// Act
		Action act = () =>
		{
			_ = new DataFlowExecutionFacade(_dataFlowExecutionContextProvider, dataFlowExecutor, _dataFlowInvalidator,
				_dataFlowCanceller, _dataFlowVersionConflictHandler);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowExecutor");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenInvalidatorIsNull()
	{
		// Arrange
		IDataFlowInvalidator dataFlowInvalidator = null!;

		// Act
		Action act = () =>
		{
			_ = new DataFlowExecutionFacade(_dataFlowExecutionContextProvider, _dataFlowExecutor, dataFlowInvalidator,
				_dataFlowCanceller, _dataFlowVersionConflictHandler);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowInvalidator");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowCancellerIsNull()
	{
		// Arrange
		IDataFlowCanceller dataFlowCanceller = null!;

		// Act
		Action act = () =>
		{
			_ = new DataFlowExecutionFacade(_dataFlowExecutionContextProvider, _dataFlowExecutor, _dataFlowInvalidator,
				dataFlowCanceller, _dataFlowVersionConflictHandler);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowCanceller");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowVersionConflictHandlerIsNull()
	{
		// Arrange
		IDataFlowVersionConflictHandler dataFlowVersionConflictHandler = null!;

		// Act
		Action act = () =>
		{
			_ = new DataFlowExecutionFacade(_dataFlowExecutionContextProvider, _dataFlowExecutor, _dataFlowInvalidator,
				_dataFlowCanceller, dataFlowVersionConflictHandler);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public async Task RunDataFlow_ShouldHandleVersionConflict_Whenever()
	{
		// Arrange
		var executionGuid = Guid.NewGuid();
		var expectedExecutionId = new DataFlowExecutionId(executionGuid);
		var flowVersion = new DataFlowVersion(1);

		DataFlowExecutionContext dataFlowExecutionContext = new()
		{
			Graph = Substitute.For<IGraph>(),
			DataFlowExecutionId = expectedExecutionId,
			DataFlowVersion = flowVersion,
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		var runDataFlowDto = new RunDataFlowDto
		{
			DynamicParameters = new List<DynamicParameterExecutionDto>
			{
				new() { Name = "age", Value = 10 }
			}
		};

		_dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(expectedExecutionId, CancellationToken.None)
			.Returns(dataFlowExecutionContext);

		// Act
		await _sut.RunDataFlow(executionGuid, 1, runDataFlowDto, CancellationToken.None);

		// Assert
		await _dataFlowVersionConflictHandler.Received(1)
			.HandleVersionConflictAsync(expectedExecutionId, flowVersion, Arg.Any<CancellationToken>());
		await _dataFlowExecutor.Received(1).RunDataFlowAsync(dataFlowExecutionContext,
			Arg.Is<List<DynamicParameter>>(parameters =>
				parameters.Count == 1 &&
				parameters.Any(p => p.Name == "age" && Equals(p.Value, 10))),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RunDataFlowNode_ShouldRunNodeWithUpdatedContext_Whenever()
	{
		// Arrange
		var dataFlowExecutionId = DataFlowExecutionId.New();
		var nodeId = NodeId.New();
		var dataFlowVersion = new DataFlowVersion(9);
		CancellationToken cancellationToken = CancellationToken.None;

		DataFlowExecutionContext dataFlowExecutionContext = new()
		{
			Graph = Substitute.For<IGraph>(),
			DataFlowExecutionId = dataFlowExecutionId,
			DataFlowVersion = dataFlowVersion,
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		var runDataFlowNodeDto = new RunDataFlowNodeDto
		{
			DynamicParameters = new List<DynamicParameterExecutionDto>
			{
				new() { Name = "age", Value = 10 }
			}
		};

		_dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(dataFlowExecutionId, cancellationToken)
			.Returns(dataFlowExecutionContext);

		// Act
		await _sut.RunDataFlowNode(dataFlowExecutionId.Value, nodeId.Value, 9, runDataFlowNodeDto, cancellationToken);

		// Assert
		await _dataFlowVersionConflictHandler.Received(1)
			.HandleVersionConflictAsync(dataFlowExecutionId, dataFlowVersion, Arg.Any<CancellationToken>());
		await _dataFlowExecutor.Received(1).RunNodeAsync(dataFlowExecutionContext,
			nodeId,
			Arg.Is<List<DynamicParameter>>(parameters =>
				parameters.Count == 1 &&
				parameters.Any(p => p.Name == "age" && Equals(p.Value, 10))),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task InvalidateDataFlow_ShouldInvalidateDataFlowWithRunningContext_WhenDataFlowExist()
	{
		// Arrange
		var executionGuid = Guid.NewGuid();
		var expectedExecutionId = new DataFlowExecutionId(executionGuid);
		CancellationToken cancellationToken = CancellationToken.None;
		DataFlowExecutionContext dataFlowExecutionContext = new()
		{
			Graph = Substitute.For<IGraph>(),
			DataFlowExecutionId = expectedExecutionId,
			DataFlowVersion = new DataFlowVersion(),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		_dataFlowExecutionContextProvider.TryGetRunningDataFlowExecutionContext(expectedExecutionId,
				out Arg.Any<DataFlowExecutionContext?>())
			.Returns(callInfo =>
			{
				callInfo[1] = dataFlowExecutionContext;
				return true;
			});

		// Act
		await _sut.InvalidateDataFlow(executionGuid, cancellationToken);

		// Assert
		await _dataFlowInvalidator.Received(1)
			.InvalidateDataFlowAsync(dataFlowExecutionContext, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task InvalidateDataFlow_ShouldNotInvalidateDataFlow_WhenDataFlowNotExist()
	{
		// Arrange
		var executionGuid = Guid.NewGuid();
		var expectedExecutionId = new DataFlowExecutionId(executionGuid);
		CancellationToken cancellationToken = CancellationToken.None;

		_dataFlowExecutionContextProvider.TryGetRunningDataFlowExecutionContext(expectedExecutionId,
			out Arg.Any<DataFlowExecutionContext?>())
			.Returns(false);

		// Act
		await _sut.InvalidateDataFlow(executionGuid, cancellationToken);

		// Assert
		await _dataFlowInvalidator.DidNotReceive()
			.InvalidateDataFlowAsync(Arg.Any<DataFlowExecutionContext>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task InvalidateDataFlowNode_ShouldInvalidateNodeAndDownstreamWithRunningContext_WhenDataFlowExist()
	{
		// Arrange
		var executionGuid = Guid.NewGuid();
		var nodeGuid = Guid.NewGuid();
		var expectedExecutionId = new DataFlowExecutionId(executionGuid);
		var expectedNodeId = new NodeId(nodeGuid);
		CancellationToken cancellationToken = CancellationToken.None;
		DataFlowExecutionContext dataFlowExecutionContext = new()
		{
			Graph = Substitute.For<IGraph>(),
			DataFlowExecutionId = expectedExecutionId,
			DataFlowVersion = new DataFlowVersion(),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		_dataFlowExecutionContextProvider.TryGetRunningDataFlowExecutionContext(expectedExecutionId,
				out Arg.Any<DataFlowExecutionContext?>())
			.Returns(callInfo =>
			{
				callInfo[1] = dataFlowExecutionContext;
				return true;
			});

		// Act
		await _sut.InvalidateDataFlowNode(executionGuid, nodeGuid, cancellationToken);

		// Assert
		await _dataFlowInvalidator.Received(1)
			.InvalidateNodeAndDownstreamAsync(dataFlowExecutionContext, expectedNodeId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task InvalidateDataFlowNode_ShouldNotInvalidateNodeAndDownstream_WhenDataFlowNotExist()
	{
		// Arrange
		var executionGuid = Guid.NewGuid();
		var nodeGuid = Guid.NewGuid();
		var expectedExecutionId = new DataFlowExecutionId(executionGuid);
		CancellationToken cancellationToken = CancellationToken.None;

		_dataFlowExecutionContextProvider.TryGetRunningDataFlowExecutionContext(expectedExecutionId,
			out Arg.Any<DataFlowExecutionContext?>())
			.Returns(false);

		// Act
		await _sut.InvalidateDataFlowNode(executionGuid, nodeGuid, cancellationToken);

		// Assert
		await _dataFlowInvalidator.DidNotReceive()
			.InvalidateNodeAndDownstreamAsync(Arg.Any<DataFlowExecutionContext>(), Arg.Any<NodeId>(),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CancelDataFlowNode_ShouldCancelNodeAndDownstreamWithRunningContext_WhenDataFlowExist()
	{
		// Arrange
		var executionGuid = Guid.NewGuid();
		var nodeGuid = Guid.NewGuid();
		var expectedExecutionId = new DataFlowExecutionId(executionGuid);
		var expectedNodeId = new NodeId(nodeGuid);
		CancellationToken cancellationToken = CancellationToken.None;
		DataFlowExecutionContext dataFlowExecutionContext = new()
		{
			Graph = Substitute.For<IGraph>(),
			DataFlowExecutionId = expectedExecutionId,
			DataFlowVersion = new DataFlowVersion(),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		_dataFlowExecutionContextProvider.TryGetRunningDataFlowExecutionContext(expectedExecutionId,
				out Arg.Any<DataFlowExecutionContext?>())
			.Returns(callInfo =>
			{
				callInfo[1] = dataFlowExecutionContext;
				return true;
			});

		// Act
		await _sut.CancelDataFlowNode(executionGuid, nodeGuid, cancellationToken);

		// Assert
		await _dataFlowCanceller.Received(1)
			.CancelNodeAndDownstreamAsync(dataFlowExecutionContext, expectedNodeId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CancelDataFlowNode_ShouldNotCancelNodeAndDownstream_WhenDataFlowNotExist()
	{
		// Arrange
		var executionGuid = Guid.NewGuid();
		var nodeGuid = Guid.NewGuid();
		var expectedExecutionId = new DataFlowExecutionId(executionGuid);
		CancellationToken cancellationToken = CancellationToken.None;

		_dataFlowExecutionContextProvider.TryGetRunningDataFlowExecutionContext(expectedExecutionId,
			out Arg.Any<DataFlowExecutionContext?>())
			.Returns(false);

		// Act
		await _sut.CancelDataFlowNode(executionGuid, nodeGuid, cancellationToken);

		// Assert
		await _dataFlowCanceller.DidNotReceive()
			.CancelNodeAndDownstreamAsync(Arg.Any<DataFlowExecutionContext>(), Arg.Any<NodeId>(),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CancelDataFlow_ShouldCancelDataFlowWithRunningContext_WhenDataFlowExist()
	{
		// Arrange
		var executionGuid = Guid.NewGuid();
		var expectedExecutionId = new DataFlowExecutionId(executionGuid);
		CancellationToken cancellationToken = CancellationToken.None;
		DataFlowExecutionContext dataFlowExecutionContext = new()
		{
			Graph = Substitute.For<IGraph>(),
			DataFlowExecutionId = expectedExecutionId,
			DataFlowVersion = new DataFlowVersion(),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		_dataFlowExecutionContextProvider.TryGetRunningDataFlowExecutionContext(expectedExecutionId,
				out Arg.Any<DataFlowExecutionContext?>())
			.Returns(callInfo =>
			{
				callInfo[1] = dataFlowExecutionContext;
				return true;
			});

		// Act
		await _sut.CancelDataFlow(executionGuid, cancellationToken);

		// Assert
		await _dataFlowCanceller.Received(1)
			.CancelDataFlowAsync(dataFlowExecutionContext, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CancelDataFlow_ShouldNotCancelDataFlow_WhenDataFlowNotExist()
	{
		// Arrange
		var executionGuid = Guid.NewGuid();
		var expectedExecutionId = new DataFlowExecutionId(executionGuid);
		CancellationToken cancellationToken = CancellationToken.None;

		_dataFlowExecutionContextProvider.TryGetRunningDataFlowExecutionContext(expectedExecutionId,
				out Arg.Any<DataFlowExecutionContext?>())
			.Returns(false);

		// Act
		await _sut.CancelDataFlow(executionGuid, cancellationToken);

		// Assert
		await _dataFlowCanceller.DidNotReceive()
			.CancelDataFlowAsync(Arg.Any<DataFlowExecutionContext>(), Arg.Any<CancellationToken>());
	}
}
