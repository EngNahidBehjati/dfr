using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;

/// <summary>
/// Configuration options for Wolverine stubs in integration tests.
/// </summary>
internal sealed class WolverineStubOptions
{
	public IReadOnlySet<Guid> NodesWithoutExecutionReport { get; init; } = new HashSet<Guid>();

	public IReadOnlySet<Guid> NodesWithoutCancelReport { get; init; } = new HashSet<Guid>();

	public IReadOnlyDictionary<Guid, NodeErrorMessage> NodeFailureMessages { get; init; } =
		new Dictionary<Guid, NodeErrorMessage>();
}
