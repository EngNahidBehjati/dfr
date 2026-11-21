using DataFlowRunner.DataFlowDynamicParameters.Business.Validation;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Business.Validation;

public sealed class DynamicParameterTypeValidatorTests
{
	private readonly DynamicParameterTypeValidator _sut;

	public DynamicParameterTypeValidatorTests()
	{
		_sut = new DynamicParameterTypeValidator(new DynamicParameterJsonValueConverter());
	}

	[Theory]
	[InlineData(42L, "INTEGER", false)]
	[InlineData(3.14, "FLOAT", false)]
	[InlineData(42L, "FLOAT", false)]
	[InlineData("text", "TEXT", false)]
	[InlineData(true, "BOOLEAN", false)]
	[InlineData("2024-01-15", "DATE", false)]
	[InlineData("12:30:00", "TIME", false)]
	[InlineData("2024-01-15T12:30:00", "DATETIME", false)]
	[InlineData("550e8400-e29b-41d4-a716-446655440000", "UUID", false)]
	public void IsConvertible_ShouldReturnTrue_WhenScalarValueMatchesDeclaredDataType(object value, string dataType,
		bool isList)
	{
		// Act
		var actual = _sut.IsConvertible(value, dataType, isList);

		// Assert
		actual.Should().BeTrue();
	}

	[Theory]
	[InlineData("not-a-number", "INTEGER", false)]
	[InlineData(42L, "TEXT", false)]
	[InlineData(3.14, "INTEGER", false)]
	[InlineData("not-a-guid", "UUID", false)]
	public void IsConvertible_ShouldReturnFalse_WhenScalarValueDoesNotMatchDeclaredDataType(object value, string dataType,
		bool isList)
	{
		// Act
		var actual = _sut.IsConvertible(value, dataType, isList);

		// Assert
		actual.Should().BeFalse();
	}

	[Fact]
	public void IsConvertible_ShouldReturnTrue_WhenListElementsMatchDeclaredDataType()
	{
		// Arrange
		var value = new List<object?> { 1L, 2L, 3L };

		// Act
		var actual = _sut.IsConvertible(value, "INTEGER", isList: true);

		// Assert
		actual.Should().BeTrue();
	}

	[Fact]
	public void IsConvertible_ShouldReturnFalse_WhenListContainsNonConvertibleElement()
	{
		// Arrange
		var value = new List<object?> { 1L, "two" };

		// Act
		var actual = _sut.IsConvertible(value, "INTEGER", isList: true);

		// Assert
		actual.Should().BeFalse();
	}

	[Fact]
	public void IsConvertible_ShouldReturnFalse_WhenScalarValueIsProvidedForListDataType()
	{
		// Act
		var actual = _sut.IsConvertible(42L, "INTEGER", isList: true);

		// Assert
		actual.Should().BeFalse();
	}

	[Fact]
	public void IsConvertible_ShouldReturnFalse_WhenValueIsNull()
	{
		// Act
		var actual = _sut.IsConvertible(null, "TEXT", isList: false);

		// Assert
		actual.Should().BeFalse();
	}

	[Fact]
	public void IsConvertible_ShouldReturnFalse_WhenDataTypeIsUnknown()
	{
		// Act
		var actual = _sut.IsConvertible("value", "UNKNOWN", isList: false);

		// Assert
		actual.Should().BeFalse();
	}
}
