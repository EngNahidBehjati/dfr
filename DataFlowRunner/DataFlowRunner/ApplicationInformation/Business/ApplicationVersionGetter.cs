using DataFlowRunner.ApplicationInformation.Abstraction;
using DataFlowRunner.Common;

namespace DataFlowRunner.ApplicationInformation.Business;

internal sealed class ApplicationVersionGetter : IApplicationVersionGetter
{
	public string Version { get; } = typeof(IAssemblyMarker)
		.Assembly
		.GetName()
		.Version?
		.ToString() ?? "0.0.0.0";
}
