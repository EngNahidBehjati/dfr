namespace DataFlowRunner.Common.Authentication.Models;

internal sealed record AmConfig
{
	public required Uri BaseUrl { get; set; }
	public required string EngineVersion { get; set; }
	public string EngineCertHash { get; set; } = string.Empty;
}
