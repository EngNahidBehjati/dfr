namespace DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;

internal static class BuildUrlHelper
{
	public static string BuildRunDataFlowUrl(Guid executionId, long dataFlowVersion)
	{
		return $"api/data-flow-executions/{executionId}/run?dataFlowVersion={dataFlowVersion}";
	}

	public static string BuildRunDataFlowNodeUrl(Guid executionId, Guid nodeId, long dataFlowVersion)
	{
		return $"api/data-flow-executions/{executionId}/run/{nodeId}?dataFlowVersion={dataFlowVersion}";
	}

	public static string BuildCancelDataFlowUrl(Guid executionId)
	{
		return $"api/data-flow-executions/{executionId}/cancel";
	}

	public static string BuildCancelDataFlowNodeUrl(Guid executionId, Guid nodeId)
	{
		return $"api/data-flow-executions/{executionId}/cancel/{nodeId}";
	}

	public static string BuildInvalidateDataFlowUrl(Guid executionId)
	{
		return $"api/data-flow-executions/{executionId}/invalidate";
	}

	public static string BuildInvalidateDataFlowNodeUrl(Guid executionId, Guid nodeId)
	{
		return $"api/data-flow-executions/{executionId}/invalidate/{nodeId}";
	}
}
