namespace DataFlowRunner.ExecutionManagement.Options;

internal sealed class RetryExecutionInvokeTimeoutOptions
{
	public const string TimeoutKey = "RetryExecutionInvokeTimeoutInSeconds";
	public TimeSpan TimeoutInSeconds { get; set; }
}
