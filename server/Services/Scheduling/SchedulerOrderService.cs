using Server.Models.Dtos;

namespace Server.Services.Scheduling;

public class SchedulerOrderService : ISchedulerOrderService
{
    public async Task<SchedulerOrderResponse> GetRecommendedOrderAsync(SchedulerOrderRequest request)
    {
        // Validate input
        ValidateTasks(request.Tasks);
        
        // Build dependency graph
        var graph = BuildDependencyGraph(request.Tasks);
        
        // Detect cycles
        DetectCycles(graph);
        
        // Perform topological sort with tie-breaking
        var orderedTasks = PerformTopologicalSort(graph, request.Tasks, request.Strategy ?? OrderStrategy.DepsDueSjf);
        
        return new SchedulerOrderResponse
        {
            RecommendedOrder = orderedTasks,
            StrategyUsed = (request.Strategy ?? OrderStrategy.DepsDueSjf).ToString()
        };
    }
    
    private void ValidateTasks(List<SchedulerTaskDto> tasks)
    {
        // Check for duplicate titles
        var titles = tasks.Select(t => t.Title).ToList();
        var duplicates = titles.GroupBy(t => t).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Any())
        {
            throw new ArgumentException($"Duplicate task titles found: {string.Join(", ", duplicates)}");
        }
        
        // Check for self-dependencies and unknown dependencies
        var taskTitles = titles.ToHashSet();
        foreach (var task in tasks)
        {
            foreach (var dependency in task.Dependencies)
            {
                if (dependency == task.Title)
                {
                    throw new ArgumentException($"Task '{task.Title}' cannot depend on itself");
                }
                
                if (!taskTitles.Contains(dependency))
                {
                    throw new ArgumentException($"Task '{task.Title}' depends on unknown task '{dependency}'");
                }
            }
        }
    }
    
    private Dictionary<string, List<string>> BuildDependencyGraph(List<SchedulerTaskDto> tasks)
    {
        var graph = new Dictionary<string, List<string>>();
        
        // Initialize all tasks
        foreach (var task in tasks)
        {
            graph[task.Title] = new List<string>();
        }
        
        // Add dependencies
        foreach (var task in tasks)
        {
            foreach (var dependency in task.Dependencies)
            {
                graph[dependency].Add(task.Title);
            }
        }
        
        return graph;
    }
    
    private void DetectCycles(Dictionary<string, List<string>> graph)
    {
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();
        
        foreach (var node in graph.Keys)
        {
            if (!visited.Contains(node))
            {
                if (HasCycle(node, graph, visited, recursionStack))
                {
                    // Find and report a cycle
                    var cycle = FindCycle(graph);
                    throw new InvalidOperationException($"Circular dependency detected: {string.Join(" -> ", cycle)}");
                }
            }
        }
    }
    
    private bool HasCycle(string node, Dictionary<string, List<string>> graph, HashSet<string> visited, HashSet<string> recursionStack)
    {
        visited.Add(node);
        recursionStack.Add(node);
        
        foreach (var neighbor in graph[node])
        {
            if (!visited.Contains(neighbor))
            {
                if (HasCycle(neighbor, graph, visited, recursionStack))
                    return true;
            }
            else if (recursionStack.Contains(neighbor))
            {
                return true;
            }
        }
        
        recursionStack.Remove(node);
        return false;
    }
    
    private List<string> FindCycle(Dictionary<string, List<string>> graph)
    {
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();
        var path = new List<string>();
        
        foreach (var node in graph.Keys)
        {
            if (!visited.Contains(node))
            {
                if (FindCyclePath(node, graph, visited, recursionStack, path))
                {
                    return path;
                }
            }
        }
        
        return new List<string>();
    }
    
    private bool FindCyclePath(string node, Dictionary<string, List<string>> graph, HashSet<string> visited, HashSet<string> recursionStack, List<string> path)
    {
        visited.Add(node);
        recursionStack.Add(node);
        path.Add(node);
        
        foreach (var neighbor in graph[node])
        {
            if (!visited.Contains(neighbor))
            {
                if (FindCyclePath(neighbor, graph, visited, recursionStack, path))
                    return true;
            }
            else if (recursionStack.Contains(neighbor))
            {
                // Found cycle, trim path to show the cycle
                var cycleStart = path.IndexOf(neighbor);
                path.RemoveRange(0, cycleStart);
                path.Add(neighbor);
                return true;
            }
        }
        
        recursionStack.Remove(node);
        path.RemoveAt(path.Count - 1);
        return false;
    }
    
    private List<string> PerformTopologicalSort(Dictionary<string, List<string>> graph, List<SchedulerTaskDto> tasks, OrderStrategy strategy)
    {
        var taskMap = tasks.ToDictionary(t => t.Title, t => t);
        var inDegree = new Dictionary<string, int>();
        var result = new List<string>();
        
        // Calculate in-degrees
        foreach (var node in graph.Keys)
        {
            inDegree[node] = 0;
        }
        
        foreach (var node in graph.Keys)
        {
            foreach (var neighbor in graph[node])
            {
                inDegree[neighbor]++;
            }
        }
        
        // Use a priority queue for tie-breaking
        var queue = new List<string>();
        
        // Add all nodes with in-degree 0
        foreach (var node in graph.Keys)
        {
            if (inDegree[node] == 0)
            {
                queue.Add(node);
            }
        }
        
        while (queue.Any())
        {
            // Sort queue based on strategy
            queue = SortByStrategy(queue, taskMap, strategy);
            
            var current = queue[0];
            queue.RemoveAt(0);
            result.Add(current);
            
            // Update in-degrees of neighbors
            foreach (var neighbor in graph[current])
            {
                inDegree[neighbor]--;
                if (inDegree[neighbor] == 0)
                {
                    queue.Add(neighbor);
                }
            }
        }
        
        return result;
    }
    
    private List<string> SortByStrategy(List<string> tasks, Dictionary<string, SchedulerTaskDto> taskMap, OrderStrategy strategy)
    {
        return strategy switch
        {
            OrderStrategy.DepsDueSjf => tasks.OrderBy(t => taskMap[t].DueDate ?? DateOnly.MaxValue)
                                            .ThenBy(t => taskMap[t].EstimatedHours ?? double.MaxValue)
                                            .ThenBy(t => t)
                                            .ToList(),
            OrderStrategy.DepsDueFifo => tasks.OrderBy(t => taskMap[t].DueDate ?? DateOnly.MaxValue)
                                             .ThenBy(t => t)
                                             .ToList(),
            OrderStrategy.DepsOnly => tasks.OrderBy(t => t).ToList(),
            _ => tasks.OrderBy(t => t).ToList()
        };
    }
}
