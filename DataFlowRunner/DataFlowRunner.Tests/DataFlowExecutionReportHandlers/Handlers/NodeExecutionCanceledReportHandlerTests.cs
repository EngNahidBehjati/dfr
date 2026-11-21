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
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecutionReportHandlers.Handlers;

public sealed class NodeExecutionCanceledReportHandlerTests
{
	private readonly INodeExecutionErrorRecorder _nodeExecutionErrorRecorder;
	private readonly IPendingDownstreamNodeCanceler _pendingDownstreamNodeCanceler;
	private readonly NodeExecutionCanceledReportHandler _sut;

	public NodeExecutionCanceledReportHandlerTests()
	{
		_nodeExecutionErrorRecorder = Substitute.For<INodeExecutionErrorRecorder>();
		_pendingDownstreamNodeCanceler = Substitute.For<IPendingDownstreamNodeCanceler>();
		ILogger<WolverineNodeExecutionReportsHandler> logger =
			Substitute.For<ILogger<WolverineNodeExecutionReportsHandler>>();
		IDataFlowActionLockExecutor dataFlowActionLockExecutor = new FakeActionLockExecutor();

		_sut = new NodeExecutionCanceledReportHandler(
			_nodeExecutionErrorRecorder, _pendingDownstreamNodeCanceler, logger, dataFlowActionLockExecutor);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task HandleAsync_ShouldRecordCancellationAndCancelDownstreamNodes_WhenHandlingCanceledNodeReport(
		bool isSucceeded)
	{
		// Arrange
		var report = new NodeExecutionCanceledReport
		{
			GraphExecutionId = Guid.NewGuid(),
			NodeId = Guid.NewGuid(),
			IsSucceeded = isSucceeded,
			Error = null
		};

		var expectedExecutionId = new DataFlowExecutionId(report.GraphExecutionId);
		var expectedNodeId = new NodeId(report.NodeId);

		// Act
		await _sut.HandleAsync(report, CancellationToken.None);

		// Assert
		_nodeExecutionErrorRecorder.Received(1)
			.RecordCancellation(expectedExecutionId, expectedNodeId,
				Arg.Is<NodeErrorMessage>(e => e.Message == "The plugin execution was canceled by user."));

		await _pendingDownstreamNodeCanceler.Received(1)
			.CancelAsync(expectedExecutionId, expectedNodeId, CancellationToken.None);
	}
}
