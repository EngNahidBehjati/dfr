using DataFlowRunner.TableQuery.Business;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace DataFlowRunner.Tests.TableQuery.Business;

public sealed class QueryParametersExtractorTests
{
	private readonly QueryParametersExtractor _sut = new();

	[Fact]
	public void GetParameters_ShouldExcludeIncludeSchemaKeyIgnoringCase_WhenIncludeSchemaProvided()
	{
		// Arrange
		var query = new QueryCollection(new Dictionary<string, StringValues>
		{
			{ "includeSchema", "true" },
			{ "filter", "x gt 1" },
			{ "IncludeSchema", "false" },
			{ "sort", "name asc" }
		});

		// Act
		IReadOnlyDictionary<string, string> actual = _sut.GetParameters(query);

		// Assert
		actual.Should().BeEquivalentTo(new Dictionary<string, string>
		{
			{ "filter", "x gt 1" },
			{ "sort", "name asc" }
		});
	}

	[Fact]
	public void GetParameters_ShouldReturnEmpty_WhenOnlyIncludeSchemaProvided()
	{
		// Arrange
		var query = new QueryCollection(new Dictionary<string, StringValues>
		{
			{ "includeSchema", "true" }
		});

		// Act
		IReadOnlyDictionary<string, string> actual = _sut.GetParameters(query);

		// Assert
		actual.Should().BeEmpty();
	}
}

