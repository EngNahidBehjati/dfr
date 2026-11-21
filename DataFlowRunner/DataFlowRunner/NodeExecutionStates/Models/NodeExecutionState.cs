namespace DataFlowRunner.NodeExecutionStates.Models;

internal enum NodeExecutionState
{
	/// <summary>
	/// Ready to run (default state)
	/// </summary>
	Idle,

	/// <summary>
	/// Node is waiting for upstream nodes to be succeeded
	/// </summary>
	Pending,

	/// <summary>
	/// Node is currently running
	/// </summary>
	Running,

	/// <summary>
	/// Node execution completed successfully
	/// </summary>
	Succeeded,

	/// <summary>
	/// Node execution failed
	/// </summary>
	Failed,

	/// <summary>
	/// Node execution was canceled (by user or system)
	/// </summary>
	Canceled,

	/// <summary>
	/// Node is currently canceling
	/// </summary>
	Canceling
}
