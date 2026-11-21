using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using DataFlowRunner.DataFlowExecutionReportHandlers.Handlers;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecutionReportHandlers.Handlers;

public class DataFlowExecutionInitializedReportHandlerTests
{
	private readonly INodeExecutorEngineGraphInitializationTracker _nodeExecutorEngineGraphInitializationTracker;
	private readonly DataFlowExecutionInitializedReportHandler _sut;

	public DataFlowExecutionInitializedReportHandlerTests()
	{
		_nodeExecutorEngineGraphInitializationTracker = Substitute.For<INodeExecutorEngineGraphInitializationTracker>();
		_sut = new DataFlowExecutionInitializedReportHandler(_nodeExecutorEngineGraphInitializationTracker);
	}

	[Fact]
	public async Task HandleAsync_ShouldTrackDataFlowInitialization_WhenReportIsSucceeded()
	{
		// Arrange
		var graphExecutionId = Guid.NewGuid();
		var report = new DataFlowExecutionInitializedReport
		{
			GraphExecutionId = graphExecutionId,
			IsSucceeded = true,
			Error = null
		};
		var expectedExecutionId = new DataFlowExecutionId(graphExecutionId);

		// Act
		await _sut.HandleAsync(report, CancellationToken.None);

		// Assert
		_nodeExecutorEngineGraphInitializationTracker.Received(1)
			.TrackDataFlowInitialization(expectedExecutionId);
	}

	[Fact]
	public async Task HandleAsync_ShouldNotTrackDataFlowInitialization_WhenReportIsNotSucceeded()
	{
		// Arrange
		var report = new DataFlowExecutionInitializedReport
		{
			GraphExecutionId = Guid.NewGuid(),
			IsSucceeded = false,
			Error = new GraphExecutionError("Initialization failed", "Stack trace")
		};

		// Act
		await _sut.HandleAsync(report, CancellationToken.None);

		// Assert
		_nodeExecutorEngineGraphInitializationTracker.DidNotReceive()
			.TrackDataFlowInitialization(Arg.Any<DataFlowExecutionId>());
	}
}
