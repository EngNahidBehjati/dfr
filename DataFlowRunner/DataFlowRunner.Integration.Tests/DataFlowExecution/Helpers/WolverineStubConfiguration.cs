using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Commands;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Reports;
using DataFlowRunner.NodeExecutionStates.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Wolverine.Runtime.Stubs;
using Wolverine.Tracking;

namespace DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;

/// <summary>
/// Configures Wolverine stubs for integration tests, allowing control over node execution and cancellation behaviors.
/// </summary>
internal sealed class WolverineStubConfiguration
{
	private readonly WebApplicationFactory<Program> _factory;

	public WolverineStubConfiguration(WebApplicationFactory<Program> factory)
	{
		_factory = factory ?? throw new ArgumentNullException(nameof(factory));
	}

	/// <summary>
	/// Configures Wolverine stubs with the specified options.
	/// </summary>
	public void ConfigureWolverineStubs(WolverineStubOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		_factory.Services.WolverineStubs(stubs =>
		{
			ConfigureExecuteNodeCommandStub(stubs, options);
			ConfigureInitializeDataFlowExecutionCommandStub(stubs);
			ConfigureCancelNodeCommandStub(stubs, options);
		});
	}

	private static void ConfigureExecuteNodeCommandStub(IStubHandlers stubs, WolverineStubOptions options)
	{
		stubs.Stub<ExecuteNodeWolverineCommand>(async (wolverineCommand, context, _, _) =>
		{
			if (options.NodesWithoutExecutionReport.Contains(wolverineCommand.NodeId))
			{
				return;
			}

			if (options.NodeFailureMessages.TryGetValue(wolverineCommand.NodeId, out NodeErrorMessage? errorMessage))
			{
				await context.PublishAsync(CreateFailedReport(wolverineCommand, errorMessage)).ConfigureAwait(false);
				return;
			}

			await context.PublishAsync(CreateSucceededReport(wolverineCommand)).ConfigureAwait(false);
		});
	}

	private static void ConfigureCancelNodeCommandStub(IStubHandlers stubs, WolverineStubOptions options)
	{
		stubs.Stub<CancelNodeWolverineCommand>(async (wolverineCommand, context, _, _) =>
		{
			if (options.NodesWithoutCancelReport.Contains(wolverineCommand.NodeId))
			{
				return;
			}

			var canceledWolverineReport = new NodeExecutionCanceledWolverineReport
			{
				GraphExecutionId = wolverineCommand.GraphExecutionId,
				NodeId = wolverineCommand.NodeId,
				IsSucceeded = true,
				Error = null
			};
			await context.PublishAsync(canceledWolverineReport).ConfigureAwait(false);
		});
	}

	private static void ConfigureInitializeDataFlowExecutionCommandStub(IStubHandlers stubs)
	{
		stubs.Stub<InitializeDataFlowExecutionWolverineCommand, DataFlowExecutionInitializedWolverineReport>(
			wolverineCommand => new DataFlowExecutionInitializedWolverineReport
			{
				GraphExecutionId = wolverineCommand.GraphExecutionId,
				IsSucceeded = true
			});
	}

	private static NodeExecutionFailedWolverineReport CreateFailedReport(
		ExecuteNodeWolverineCommand wolverineCommand,
		NodeErrorMessage errorMessage)
	{
		var failedWolverineReport = new NodeExecutionFailedWolverineReport
		{
			GraphExecutionId = wolverineCommand.GraphExecutionId,
			NodeId = wolverineCommand.NodeId,
			Error = new GraphExecutionError(errorMessage.Message, errorMessage.StackTrace ?? "")
		};
		return failedWolverineReport;
	}

	private static NodeExecutionSucceededWolverineReport CreateSucceededReport(
		ExecuteNodeWolverineCommand wolverineCommand)
	{
		var succeededWolverineReport = new NodeExecutionSucceededWolverineReport
		{
			GraphExecutionId = wolverineCommand.GraphExecutionId,
			NodeId = wolverineCommand.NodeId
		};
		return succeededWolverineReport;
	}
}
