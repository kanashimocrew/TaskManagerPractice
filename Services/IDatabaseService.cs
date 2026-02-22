using TaskManager.Models;

namespace TaskManager.Services
{
    public interface IDatabaseService
    {
        
        Task<List<TaskItem>> GetTasksByDateAsync(DateTime date);
        Task<TaskItem> GetTaskAsync(int id);
        Task<int> SaveTaskAsync(TaskItem task);
        Task<int> DeleteTaskAsync(TaskItem task);

        
        Task<int> GetTaskCountForDateAsync(DateTime date);
        Task<Dictionary<DateTime, int>> GetTaskCountsForMonthAsync(int year, int month);
    }
}