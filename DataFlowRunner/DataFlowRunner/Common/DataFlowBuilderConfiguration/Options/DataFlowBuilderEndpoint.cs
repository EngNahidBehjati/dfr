namespace DataFlowRunner.Common.DataFlowBuilderConfiguration.Options;

public sealed class DataFlowBuilderEndpoint
{
	public required string BaseUrl { get; set; }
	public string EngineCertHash { get; set; } = string.Empty;
}
