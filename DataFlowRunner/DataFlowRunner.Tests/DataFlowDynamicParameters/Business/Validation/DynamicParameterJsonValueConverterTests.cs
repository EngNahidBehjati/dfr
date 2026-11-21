using System.Text.Json;
using DataFlowRunner.DataFlowDynamicParameters.Business.Validation;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Business.Validation;

public sealed class DynamicParameterJsonValueConverterTests
{
	private readonly DynamicParameterJsonValueConverter _sut;

	public DynamicParameterJsonValueConverterTests()
	{
		_sut = new DynamicParameterJsonValueConverter();
	}

	[Fact]
	public void Convert_ShouldReturnLong_WhenJsonElementIsWholeNumber()
	{
		// Arrange
		JsonElement value = JsonSerializer.SerializeToElement(30);

		// Act
		var actual = _sut.Convert(value);

		// Assert
		actual.Should().Be(30L);
	}

	[Fact]
	public void Convert_ShouldReturnString_WhenJsonElementIsString()
	{
		// Arrange
		JsonElement value = JsonSerializer.SerializeToElement("hello");

		// Act
		var actual = _sut.Convert(value);

		// Assert
		actual.Should().Be("hello");
	}

	[Fact]
	public void Convert_ShouldReturnString_WhenJsonElementIsBool()
	{
		// Arrange
		JsonElement value = JsonSerializer.SerializeToElement(true);

		// Act
		var actual = _sut.Convert(value);

		// Assert
		actual.Should().Be(true);
	}

	[Fact]
	public void Convert_ShouldReturnList_WhenJsonElementIsArray()
	{
		// Arrange
		JsonElement value = JsonSerializer.SerializeToElement(new[] { 1, 2 });

		// Act
		var actual = _sut.Convert(value);

		// Assert
		actual.Should().BeEquivalentTo(new List<object?> { 1L, 2L });
	}
}
