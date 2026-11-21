namespace DataFlowRunner.Common.DependencyInjection;

internal static class DependencyRegistrar
{
	public static void RegisterDependencies(this IServiceCollection services)
	{
		IEnumerable<IDependencyInstaller> installers = typeof(IAssemblyMarker)
			.Assembly
			.GetTypes()
			.Where(type => type is { IsAbstract: false, IsInterface: false } && typeof(IDependencyInstaller).IsAssignableFrom(type))
			.Select(type => Activator.CreateInstance(type, nonPublic: true))
			.Cast<IDependencyInstaller>();

		foreach (IDependencyInstaller installer in installers)
		{
			installer.Install(services);
		}
	}
}
