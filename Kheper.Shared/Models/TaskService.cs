namespace Kheper.Shared.Models;

public class TaskService
{
    
    public List<(string Name, string Desc, DateTime? TaskTime)> AllTasks { get; private set; } = new();

    
    public event Action? OnTaskAdded;

    public void AddTask(string name, string desc, DateTime? taskTime = null)
    {
        AllTasks.Add((name, desc, taskTime));
        
        OnTaskAdded?.Invoke();
    }
}