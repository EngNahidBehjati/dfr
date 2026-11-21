using DataFlowRunner.DataFlowDynamicParameters.Business.RunDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Exceptions;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Business.RunDynamicParameters;

public sealed class RunDynamicParametersValidatorTests
{
	private readonly RunDynamicParametersValidator _sut;

	public RunDynamicParametersValidatorTests()
	{
		_sut = new RunDynamicParametersValidator(LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public void EnsureProvidedParametersAreValid_ShouldNotThrow_WhenAllParametersExist()
	{
		// Arrange
		ResponseDataFlowDynamicParametersDto fetchResponse = CreateFetchResponse(
			new ResponseDataFlowDynamicParameterDto("id-1", "param1", true, "TEXT", false, null));
		IReadOnlyCollection<DynamicParameter> provided = [new() { Name = "param1", Value = "a" }];

		// Act
		Action act = () => _sut.EnsureProvidedParametersAreValid(fetchResponse, provided);

		// Assert
		act.Should().NotThrow();
	}

	[Fact]
	public void EnsureProvidedParametersAreValid_ShouldThrowDynamicParameterNotFilledException_WhenParameterIsMissing()
	{
		// Arrange
		ResponseDataFlowDynamicParametersDto fetchResponse = CreateFetchResponse(
			new ResponseDataFlowDynamicParameterDto("id-1", "param1", true, "TEXT", false, null),
			new ResponseDataFlowDynamicParameterDto("id-2", "param2", true, "INTEGER", false, null));
		IReadOnlyCollection<DynamicParameter> provided = [new() { Name = "param1", Value = "a" }];

		// Act
		Action act = () => _sut.EnsureProvidedParametersAreValid(fetchResponse, provided);

		// Assert
		act.Should().Throw<DynamicParameterNotFilledException>()
			.WithMessage("*param2*");
	}

	[Fact]
	public void EnsureProvidedParametersAreValid_ShouldNotThrow_WhenOptionalParameterValueIsNull()
	{
		// Arrange
		ResponseDataFlowDynamicParametersDto fetchResponse = CreateFetchResponse(
			new ResponseDataFlowDynamicParameterDto("id-1", "param1", false, "TEXT", false, null));
		IReadOnlyCollection<DynamicParameter> provided = [new() { Name = "param1", Value = null }];

		// Act
		Action act = () => _sut.EnsureProvidedParametersAreValid(fetchResponse, provided);

		// Assert
		act.Should().NotThrow();
	}

	[Fact]
	public void EnsureProvidedParametersAreValid_ShouldThrowDynamicParameterNotFilledException_WhenRequiredParameterValueIsNull()
	{
		// Arrange
		ResponseDataFlowDynamicParametersDto fetchResponse = CreateFetchResponse(
			new ResponseDataFlowDynamicParameterDto("id-1", "param1", true, "TEXT", false, null));
		IReadOnlyCollection<DynamicParameter> provided = [new() { Name = "param1", Value = null }];

		// Act
		Action act = () => _sut.EnsureProvidedParametersAreValid(fetchResponse, provided);

		// Assert
		act.Should().Throw<DynamicParameterNotFilledException>()
			.WithMessage("*param1*");
	}

	private static ResponseDataFlowDynamicParametersDto CreateFetchResponse(
		params ResponseDataFlowDynamicParameterDto[] parameters)
	{
		return new ResponseDataFlowDynamicParametersDto(
		[
			new ResponseNodeDynamicParametersDto(Guid.NewGuid(), parameters)
		]);
	}
}
