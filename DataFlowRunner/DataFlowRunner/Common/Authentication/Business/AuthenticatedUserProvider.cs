using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.Common.Authentication.Business;

internal sealed class AuthenticatedUserProvider : IAuthenticatedUserProvider
{
	public AuthenticatedUserProvider(IHttpContextAccessor httpContextAccessor,
		ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	private readonly IHttpContextAccessor _httpContextAccessor;

	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public Task<UserId> GetCurrentUserId(CancellationToken cancellationToken)
	{
		Guid userId = new Guid("e1185264-5717-4565-b3fc-2c963f36af12");
		return Task.FromResult(new UserId(userId));
	}
}
