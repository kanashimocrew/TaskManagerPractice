using TaskManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskManager.Services
{
    public interface IDatabaseService
    {
        // Инициализация базы данных
        Task InitializeDatabaseAsync();

        // Операции с задачами
        Task<List<TaskItem>> GetTasksAsync();
        Task<List<TaskItem>> GetTasksByDateAsync(DateTime date);
        Task<TaskItem> GetTaskAsync(int id);
        Task<int> SaveTaskAsync(TaskItem task);
        Task<int> DeleteTaskAsync(TaskItem task);

        // Методы для календаря
        Task<int> GetTaskCountForDateAsync(DateTime date);
        Task<Dictionary<DateTime, int>> GetTaskCountsForMonthAsync(int year, int month);
    }
}