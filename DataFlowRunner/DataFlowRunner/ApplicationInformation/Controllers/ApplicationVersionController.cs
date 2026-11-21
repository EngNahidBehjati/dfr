using Asp.Versioning;
using DataFlowRunner.ApplicationInformation.Abstraction;
using DataFlowRunner.ApplicationInformation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.ApplicationInformation.Controllers;

[ApiController]
[Route("api/version")]
[ApiVersion("1")]
public sealed class ApplicationVersionController : ControllerBase
{
	private readonly IApplicationVersionGetter _applicationVersionGetter;

	public ApplicationVersionController(IApplicationVersionGetter applicationVersionGetter)
	{
		_applicationVersionGetter = applicationVersionGetter
		                                ?? throw new ArgumentNullException(nameof(applicationVersionGetter));
	}

	[HttpGet]
	[AllowAnonymous]
	public ApplicationVersionDto GetVersion()
	{
		return new ApplicationVersionDto(
			_applicationVersionGetter.Version);
	}
}
