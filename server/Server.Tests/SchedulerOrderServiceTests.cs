using Server.Models.Dtos;
using Server.Services.Scheduling;
using Xunit;

namespace Server.Tests;

public class SchedulerOrderServiceTests
{
    private readonly ISchedulerOrderService _schedulerService;

    public SchedulerOrderServiceTests()
    {
        _schedulerService = new SchedulerOrderService();
    }

    [Fact]
    public async Task GetRecommendedOrder_SimpleChain_ReturnsCorrectOrder()
    {
        // Arrange
        var request = new SchedulerOrderRequest
        {
            Tasks = new List<SchedulerTaskDto>
            {
                new() { Title = "A", EstimatedHours = 5, DueDate = new DateOnly(2025, 10, 25) },
                new() { Title = "B", EstimatedHours = 3, DueDate = new DateOnly(2025, 10, 26), Dependencies = new List<string> { "A" } },
                new() { Title = "C", EstimatedHours = 8, DueDate = new DateOnly(2025, 10, 27), Dependencies = new List<string> { "B" } }
            }
        };

        // Act
        var result = await _schedulerService.GetRecommendedOrderAsync(request);

        // Assert
        Assert.Equal(new[] { "A", "B", "C" }, result.RecommendedOrder);
        Assert.Equal("DepsDueSjf", result.StrategyUsed);
    }

    [Fact]
    public async Task GetRecommendedOrder_WithTieBreakers_ReturnsCorrectOrder()
    {
        // Arrange
        var request = new SchedulerOrderRequest
        {
            Tasks = new List<SchedulerTaskDto>
            {
                new() { Title = "Task1", EstimatedHours = 10, DueDate = new DateOnly(2025, 10, 30) },
                new() { Title = "Task2", EstimatedHours = 5, DueDate = new DateOnly(2025, 10, 30) },
                new() { Title = "Task3", EstimatedHours = 8, DueDate = new DateOnly(2025, 10, 30) }
            }
        };

        // Act
        var result = await _schedulerService.GetRecommendedOrderAsync(request);

        // Assert
        // Should be ordered by estimated hours (shortest first), then by title
        Assert.Equal("Task2", result.RecommendedOrder[0]);
        Assert.Equal("Task3", result.RecommendedOrder[1]);
        Assert.Equal("Task1", result.RecommendedOrder[2]);
    }

    [Fact]
    public async Task GetRecommendedOrder_WithNullDueDate_HandlesCorrectly()
    {
        // Arrange
        var request = new SchedulerOrderRequest
        {
            Tasks = new List<SchedulerTaskDto>
            {
                new() { Title = "Task1", EstimatedHours = 5, DueDate = null },
                new() { Title = "Task2", EstimatedHours = 3, DueDate = new DateOnly(2025, 10, 25) }
            }
        };

        // Act
        var result = await _schedulerService.GetRecommendedOrderAsync(request);

        // Assert
        // Task2 should come first (has due date), then Task1 (null due date treated as last)
        Assert.Equal("Task2", result.RecommendedOrder[0]);
        Assert.Equal("Task1", result.RecommendedOrder[1]);
    }

    [Fact]
    public async Task GetRecommendedOrder_DuplicateTitles_ThrowsException()
    {
        // Arrange
        var request = new SchedulerOrderRequest
        {
            Tasks = new List<SchedulerTaskDto>
            {
                new() { Title = "Task1" },
                new() { Title = "Task1" }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _schedulerService.GetRecommendedOrderAsync(request));
    }

    [Fact]
    public async Task GetRecommendedOrder_SelfDependency_ThrowsException()
    {
        // Arrange
        var request = new SchedulerOrderRequest
        {
            Tasks = new List<SchedulerTaskDto>
            {
                new() { Title = "Task1", Dependencies = new List<string> { "Task1" } }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _schedulerService.GetRecommendedOrderAsync(request));
    }

    [Fact]
    public async Task GetRecommendedOrder_UnknownDependency_ThrowsException()
    {
        // Arrange
        var request = new SchedulerOrderRequest
        {
            Tasks = new List<SchedulerTaskDto>
            {
                new() { Title = "Task1", Dependencies = new List<string> { "UnknownTask" } }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _schedulerService.GetRecommendedOrderAsync(request));
    }

    [Fact]
    public async Task GetRecommendedOrder_CircularDependency_ThrowsException()
    {
        // Arrange
        var request = new SchedulerOrderRequest
        {
            Tasks = new List<SchedulerTaskDto>
            {
                new() { Title = "A", Dependencies = new List<string> { "B" } },
                new() { Title = "B", Dependencies = new List<string> { "C" } },
                new() { Title = "C", Dependencies = new List<string> { "A" } }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _schedulerService.GetRecommendedOrderAsync(request));
        Assert.Contains("Circular dependency detected", exception.Message);
    }

    [Fact]
    public async Task GetRecommendedOrder_DifferentStrategies_ReturnsCorrectOrder()
    {
        // Arrange
        var tasks = new List<SchedulerTaskDto>
        {
            new() { Title = "TaskA", EstimatedHours = 10, DueDate = new DateOnly(2025, 10, 30) },
            new() { Title = "TaskB", EstimatedHours = 5, DueDate = new DateOnly(2025, 10, 25) },
            new() { Title = "TaskC", EstimatedHours = 8, DueDate = new DateOnly(2025, 10, 30) }
        };

        // Test DepsDueSjf strategy
        var requestSjf = new SchedulerOrderRequest { Tasks = tasks, Strategy = OrderStrategy.DepsDueSjf };
        var resultSjf = await _schedulerService.GetRecommendedOrderAsync(requestSjf);
        Assert.Equal("DepsDueSjf", resultSjf.StrategyUsed);

        // Test DepsDueFifo strategy
        var requestFifo = new SchedulerOrderRequest { Tasks = tasks, Strategy = OrderStrategy.DepsDueFifo };
        var resultFifo = await _schedulerService.GetRecommendedOrderAsync(requestFifo);
        Assert.Equal("DepsDueFifo", resultFifo.StrategyUsed);

        // Test DepsOnly strategy
        var requestDeps = new SchedulerOrderRequest { Tasks = tasks, Strategy = OrderStrategy.DepsOnly };
        var resultDeps = await _schedulerService.GetRecommendedOrderAsync(requestDeps);
        Assert.Equal("DepsOnly", resultDeps.StrategyUsed);
    }
}
