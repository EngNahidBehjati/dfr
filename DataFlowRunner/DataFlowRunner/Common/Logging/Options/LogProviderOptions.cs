namespace DataFlowRunner.Common.Logging.Options;

internal sealed class LogProviderOptions
{
	public const string SectionPath = "Logging:LogProvider";

	public LogProvider ProviderType { get; init; } = LogProvider.Console;

	public string? FolderPath { get; init; }

	public string EffectiveFilePath =>
		string.IsNullOrWhiteSpace(FolderPath)
			? "logs/log-.json"
			: $"{FolderPath}/log-.json";
}
