namespace DataFlowRunner.NodeExecutionStates.Models;

internal sealed class DataFlowExecutionStatus
{
	public bool Failed { get; }
	public string? FailureMessage { get; }

	public static readonly DataFlowExecutionStatus DefaultStatus = new();

	public DataFlowExecutionStatus(string message)
	{
		Failed = true;
		FailureMessage = message;
	}

	private DataFlowExecutionStatus()
	{
		Failed = false;
		FailureMessage = null;
	}
}
