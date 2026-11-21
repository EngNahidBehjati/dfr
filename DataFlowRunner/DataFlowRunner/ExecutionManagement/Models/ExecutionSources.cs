namespace DataFlowRunner.ExecutionManagement.Models;

public static class ExecutionSources
{
	public static readonly HashSet<string> ValidExecutionSources =
	[
		"DEFAULT",
		"PROCESS",
		"DATA_FLOW_LIST",
		"API",
		"DATA_FLOW_PAGE",
		"PACKAGE",
		"DATA_FLOW_EXECUTOR_PLUGIN",
		"PAP",
		"LAP",
		"WEB_DATA_FLOW_PAGE",
		"PACKAGE_API"
	];

	public static readonly HashSet<string> RetryExecutionValidExecutionSources =
	[
		"PROCESS",
		"DATA_FLOW_LIST",
		"API"
	];
}
