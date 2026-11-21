namespace DataFlowRunner.Common.DependencyInjection;

public interface IDependencyInstaller
{
	void Install(IServiceCollection services);
}
