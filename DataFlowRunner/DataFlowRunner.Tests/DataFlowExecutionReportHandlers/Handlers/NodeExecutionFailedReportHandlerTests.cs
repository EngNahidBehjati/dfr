using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;
using DataFlowRunner.DataFlowExecutionReportHandlers.Handlers;
using DataFlowRunner.DataFlowSynchronization.Abstraction;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.Tests.Common.FakeClasses;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecutionReportHandlers.Handlers;

public sealed class NodeExecutionFailedReportHandlerTests
{
	private readonly INodeExecutionErrorRecorder _nodeExecutionErrorRecorder;
	private readonly IPendingDownstreamNodeCanceler _pendingDownstreamNodeCanceler;
	private readonly NodeExecutionFailedReportHandler _sut;

	public NodeExecutionFailedReportHandlerTests()
	{
		_nodeExecutionErrorRecorder = Substitute.For<INodeExecutionErrorRecorder>();
		_pendingDownstreamNodeCanceler = Substitute.For<IPendingDownstreamNodeCanceler>();
		ILogger<WolverineNodeExecutionReportsHandler> logger =
			Substitute.For<ILogger<WolverineNodeExecutionReportsHandler>>();
		IDataFlowActionLockExecutor dataFlowActionLockExecutor = new FakeActionLockExecutor();

		_sut = new NodeExecutionFailedReportHandler(
			_nodeExecutionErrorRecorder, _pendingDownstreamNodeCanceler, logger, dataFlowActionLockExecutor);
	}

	[Fact]
	public async Task HandleAsync_ShouldRecordFailureAndCancelDownstreamNodes_WhenReportIsProvided()
	{
		// Arrange
		var report = new NodeExecutionFailedReport
		{
			GraphExecutionId = Guid.NewGuid(),
			NodeId = Guid.NewGuid(),
			Error = new GraphExecutionError("Test error message", "Stack trace")
		};

		var executionId = new DataFlowExecutionId(report.GraphExecutionId);
		var nodeId = new NodeId(report.NodeId);

		// Act
		await _sut.HandleAsync(report, CancellationToken.None);

		// Assert
		_nodeExecutionErrorRecorder.Received(1)
			.RecordFailure(executionId, nodeId,
				Arg.Is<NodeErrorMessage>(e =>
					e.Message == report.Error.ErrorMessage && e.StackTrace == report.Error.StackTrace));

		await _pendingDownstreamNodeCanceler.Received(1)
			.CancelAsync(executionId, nodeId, CancellationToken.None);
	}

	[Fact]
	public async Task HandleAsync_ShouldThrowInvalidOperationException_WhenReportErrorIsNull()
	{
		// Arrange
		var report = new NodeExecutionFailedReport
		{
			GraphExecutionId = Guid.NewGuid(),
			NodeId = Guid.NewGuid(),
			Error = null!
		};

		// Act
		Func<Task> act = () => _sut.HandleAsync(report, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>();
		_nodeExecutionErrorRecorder.DidNotReceive()
			.RecordFailure(Arg.Any<DataFlowExecutionId>(), Arg.Any<NodeId>(), Arg.Any<NodeErrorMessage>());
		await _pendingDownstreamNodeCanceler.DidNotReceive()
			.CancelAsync(Arg.Any<DataFlowExecutionId>(), Arg.Any<NodeId>(), Arg.Any<CancellationToken>());
	}
}
