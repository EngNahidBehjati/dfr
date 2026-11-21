using DataFlowRunner.Common.Exceptions.Constants;
using DataFlowRunner.Common.Exceptions.Models;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.Common.Exceptions;

public sealed class DataFlowBuilderException : Exception
{
	private static readonly string _dataFlowBuilderError =
		LanguageDictionaryProvider.Instance.DataFlowRunnerLanguage.DataFlowRunner.Common.DataFlowBuilderError();

	public DataFlowBuilderException(string message, ExternalErrorResponse externalErrorResponse) :
		base(message + "\n" + _dataFlowBuilderError + "\n\t- " + externalErrorResponse.Message)
	{
		if (externalErrorResponse.StackTrace != null)
		{
			Data[StackTraceConstants.DataFlowBuilder] = externalErrorResponse.StackTrace;
		}
	}

	public DataFlowBuilderException(string? message) : base(message)
	{
	}

	public DataFlowBuilderException(string? message, Exception? innerException)
		: base(message, innerException)
	{
	}
}
