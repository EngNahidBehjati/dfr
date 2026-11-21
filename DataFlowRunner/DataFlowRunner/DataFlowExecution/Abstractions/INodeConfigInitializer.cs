using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface INodeConfigInitializer
{
	Task EnsureNodeConfigsLoadedAsync(DataFlowExecutionId dataFlowExecutionId, IReadOnlyCollection<Node> nodeIds,
		CancellationToken cancellationToken);
}
