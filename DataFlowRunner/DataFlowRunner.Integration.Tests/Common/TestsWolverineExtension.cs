using Wolverine;

namespace DataFlowRunner.Integration.Tests.Common;

internal sealed class TestsWolverineExtension : IWolverineExtension
{
	public void Configure(WolverineOptions options)
	{
		options.LocalQueue("localQ")
			.Sequential();

		options.PublishAllMessages()
			.ToLocalQueue("localQ");
	}
}
