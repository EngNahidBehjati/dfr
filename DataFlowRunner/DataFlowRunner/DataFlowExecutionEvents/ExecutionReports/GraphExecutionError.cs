namespace DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;

public sealed record GraphExecutionError(string ErrorMessage, string StackTrace);
