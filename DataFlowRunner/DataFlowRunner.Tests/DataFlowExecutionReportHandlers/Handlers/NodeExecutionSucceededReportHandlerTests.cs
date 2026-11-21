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

public class NodeExecutionSucceededReportHandlerTests
{
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly IChildNodesExecutor _childNodesExecutor;
	private readonly NodeExecutionSucceededReportHandler _sut;

	public NodeExecutionSucceededReportHandlerTests()
	{
		_nodeExecutionStateUpdater = Substitute.For<INodeExecutionStateUpdater>();
		_childNodesExecutor = Substitute.For<IChildNodesExecutor>();
		ILogger<WolverineNodeExecutionReportsHandler> logger =
			Substitute.For<ILogger<WolverineNodeExecutionReportsHandler>>();
		IDataFlowActionLockExecutor dataFlowActionLockExecutor = new FakeActionLockExecutor();

		_sut = new NodeExecutionSucceededReportHandler(_nodeExecutionStateUpdater, _childNodesExecutor, logger,
			dataFlowActionLockExecutor);
	}

	[Fact]
	public async Task OnSucceededAsync_ShouldSetNodeStateToSucceededAndRunReadyChildNodes_WhenReportIsProvided()
	{
		// Arrange
		var report = new NodeExecutionSucceededReport
		{
			GraphExecutionId = Guid.NewGuid(),
			NodeId = Guid.NewGuid()
		};

		var expectedExecutionId = new DataFlowExecutionId(report.GraphExecutionId);
		var expectedNodeId = new NodeId(report.NodeId);

		// Act
		await _sut.HandleAsync(report, CancellationToken.None);

		// Assert
		_nodeExecutionStateUpdater.Received(1)
			.ChangeNodeExecutionState(expectedExecutionId, expectedNodeId, NodeExecutionState.Succeeded);
		await _childNodesExecutor.Received(1)
			.RunReadyChildNodes(expectedExecutionId, expectedNodeId, CancellationToken.None);
	}
}
