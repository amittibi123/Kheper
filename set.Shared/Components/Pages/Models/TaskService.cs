public class TaskService
{
    
    public List<(string Name, string Desc)> AllTasks { get; private set; } = new();

    
    public event Action? OnTaskAdded;

    public void AddTask(string name, string desc)
    {
        AllTasks.Add((name, desc));
        
        OnTaskAdded?.Invoke();
    }
}