namespace TaskManager.Models
{
    public enum TaskPriority
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    public enum TaskStatus
    {
        New = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }
}