using System.Net.Http.Json;
using DataFlowRunner.Integration.Tests.Common;
using DataFlowRunner.TableQuery.Models;
using FluentAssertions;

namespace DataFlowRunner.Integration.Tests.TableQuery.Controllers.Assertions;

internal static class TableQueryResponseAssertions
{
	public static async Task AssertMatchesAsync(
		HttpResponseMessage apiResponse,
		TableQueryResponseDto expectedDto)
	{
		apiResponse.Should().NotBeNull();
		apiResponse.Should().BeSuccessful();

		TableQueryResponseDto? actualDto =
			await apiResponse.Content.ReadFromJsonAsync<TableQueryResponseDto>(ApiSerializationDefaults.Options);

		actualDto.Should().NotBeNull();
		actualDto.TableSchema.Should().BeEquivalentTo(expectedDto.TableSchema);

		actualDto.Rows.Should().HaveSameCount(expectedDto.Rows);

		for (var i = 0; i < expectedDto.Rows.Count; i++)
		{
			RowDto expectedRow = expectedDto.Rows[i];
			RowDto actualRow = actualDto.Rows[i];

			actualRow.Cells.Keys.Should().BeEquivalentTo(expectedRow.Cells.Keys);

			foreach (var (key, expectedValue) in expectedRow.Cells)
			{
				actualRow.Cells.TryGetValue(key, out var actualValue).Should().BeTrue();
				(actualValue?.ToString() ?? string.Empty)
					.Should()
					.Be(expectedValue?.ToString() ?? string.Empty, $"cell '{key}' should match");
			}
		}
	}
}
