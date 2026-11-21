using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Business;

internal class NodeExecutionDurationResolver : INodeExecutionDurationResolver
{
	private readonly TimeProvider _timeProvider;

	public NodeExecutionDurationResolver(TimeProvider timeProvider)
	{
		_timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
	}

	public long? ResolveDurationMilliseconds(NodeExecutionTiming nodeExecutionTiming)
	{
		if (nodeExecutionTiming.StartedAt == null)
			return null;
		DateTimeOffset start = nodeExecutionTiming.StartedAt.Value;
		DateTimeOffset end = nodeExecutionTiming.CompletedAt ?? _timeProvider.GetUtcNow();
		return (long)(end - start).TotalMilliseconds;
	}
}
