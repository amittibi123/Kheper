public class TaskService
{
    // רשימת המשימות המרכזית
    public List<(string Name, string Desc)> AllTasks { get; private set; } = new();

    // אירוע שמופעל כשמתווספת משימה
    public event Action? OnTaskAdded;

    public void AddTask(string name, string desc)
    {
        AllTasks.Add((name, desc));
        // מודיע לכל מי שמאזין (לדף ה-ToDo) שמשהו השתנה
        OnTaskAdded?.Invoke();
    }
}