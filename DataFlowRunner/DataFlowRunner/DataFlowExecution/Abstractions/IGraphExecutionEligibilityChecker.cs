using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface IGraphExecutionEligibilityChecker
{
	bool IsGraphExecutable(IReadOnlyGraph graph, DataFlowExecutionId dataFlowExecutionId);
}
