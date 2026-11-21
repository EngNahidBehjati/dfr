using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowSynchronization.Abstraction;
using DataFlowRunner.DataFlowSynchronization.Business;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowSynchronization.Business;

public class DataFlowActionLockExecutorTests
{
	private readonly DataFlowActionLockExecutor _sut;

	public DataFlowActionLockExecutorTests()
	{
		ISemaphoreStore semaphoreStore = Substitute.For<ISemaphoreStore>();
		semaphoreStore.GetSemaphore(Arg.Any<DataFlowExecutionId>(), Arg.Any<string>())
			.Returns(new SemaphoreSlim(1, 1));
		_sut = new DataFlowActionLockExecutor(semaphoreStore);
	}

	[Fact]
	public async Task Invoke_ShouldExecuteParallelMethodCallsSequentially_WhenMultipleCallsAreMadeParallel()
	{
		// Arrange
		var dataFlowExecutionId = DataFlowExecutionId.New();
		const string actionKey = "action";

		var maxConcurrencyDegree = 1;
		var concurrencyDegree = 0;

		Action action = () =>
		{
			Interlocked.Add(ref concurrencyDegree, 1);
			if (concurrencyDegree > maxConcurrencyDegree)
			{
				maxConcurrencyDegree = concurrencyDegree;
			}

			Task.Delay(10).Wait();
			Interlocked.Add(ref concurrencyDegree, -1);
		};

		// Act
		var tasks = Enumerable.Range(1, 10)
			.Select(_ => Task.Run(() => _sut.Invoke(dataFlowExecutionId, actionKey, action)))
			.ToList();

		await Task.WhenAll(tasks);

		// Assert
		maxConcurrencyDegree.Should().Be(1);
	}

	[Fact]
	public async Task InvokeAsync_ShouldExecuteParallelMethodCallsSequentially_WhenMultipleCallsAreMadeParallel()
	{
		// Arrange
		var dataFlowExecutionId = DataFlowExecutionId.New();
		const string actionKey = "action";

		var maxConcurrencyDegree = 1;
		var concurrencyDegree = 0;

		Func<Task> action = () =>
		{
			Interlocked.Add(ref concurrencyDegree, 1);
			if (concurrencyDegree > maxConcurrencyDegree)
			{
				maxConcurrencyDegree = concurrencyDegree;
			}

			Task.Delay(10).Wait();
			Interlocked.Add(ref concurrencyDegree, -1);
			return Task.CompletedTask;
		};

		// Act
		var tasks = Enumerable.Range(0, 10)
			.Select(_ => Task.Run(async () =>
				await _sut.InvokeAsync(dataFlowExecutionId, actionKey, action, CancellationToken.None)))
			.ToList();

		await Task.WhenAll(tasks);

		// Assert
		maxConcurrencyDegree.Should().Be(1);
	}
}
