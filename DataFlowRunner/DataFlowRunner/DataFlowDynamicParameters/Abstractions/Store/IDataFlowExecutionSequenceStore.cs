using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;

internal interface IDataFlowExecutionSequenceStore
{
	/// <summary>
	/// Records a new submission batch for <paramref name="executionId"/> and stamps each node in
	/// <paramref name="nodes"/> with the same monotonically increasing per-execution version.
	/// </summary>
	void SetNextRunSequence(DataFlowExecutionId executionId, IReadOnlyCollection<NodeId> nodes);

	/// <summary>
	/// Returns the last recorded submission version for <paramref name="nodeId"/> in
	/// <paramref name="executionId"/>, or 0 if the node has never been submitted.
	/// </summary>
	long GetNodeRunSequence(DataFlowExecutionId executionId, NodeId nodeId);
}
