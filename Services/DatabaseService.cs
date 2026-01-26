using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SQLite;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class DatabaseService : IDatabaseService
    {
        private SQLiteAsyncConnection _database;
        private bool _isInitialized = false;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private async Task InitializeAsync()
        {
            if (_isInitialized) return;

            await _semaphore.WaitAsync();
            try
            {
                if (_isInitialized) return;

                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "tasks.db3");
                _database = new SQLiteAsyncConnection(dbPath);

                
                await _database.CreateTableAsync<TaskItem>();

                _isInitialized = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<TaskItem>> GetTasksByDateAsync(DateTime date)
        {
            await InitializeAsync();

            try
            {
                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                
                var allTasks = await _database.Table<TaskItem>().ToListAsync();

                return allTasks
                    .Where(t => t.DueDate >= startDate &&
                               t.DueDate < endDate &&
                               !t.IsDeleted)
                    .OrderBy(t => t.DueDate)
                    .ThenByDescending(t => (int)t.Priority) 
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTasksByDateAsync: {ex.Message}");
                return new List<TaskItem>();
            }
        }

        public async Task<TaskItem> GetTaskAsync(int id)
        {
            await InitializeAsync();

            try
            {
                return await _database.Table<TaskItem>()
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTaskAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<int> SaveTaskAsync(TaskItem task)
        {
            await InitializeAsync();

            try
            {
                if (task.Id != 0)
                {
                    task.UpdatedAt = DateTime.Now;
                    return await _database.UpdateAsync(task);
                }
                else
                {
                    task.CreatedAt = DateTime.Now;
                    return await _database.InsertAsync(task);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveTaskAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> DeleteTaskAsync(TaskItem task)
        {
            await InitializeAsync();

            try
            {
                
                task.IsDeleted = true;
                task.UpdatedAt = DateTime.Now;
                return await _database.UpdateAsync(task);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteTaskAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetTaskCountForDateAsync(DateTime date)
        {
            await InitializeAsync();

            try
            {
                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                var allTasks = await _database.Table<TaskItem>().ToListAsync();

                return allTasks
                    .Count(t => t.DueDate >= startDate &&
                               t.DueDate < endDate &&
                               !t.IsDeleted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTaskCountForDateAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task<Dictionary<DateTime, int>> GetTaskCountsForMonthAsync(int year, int month)
        {
            await InitializeAsync();

            try
            {
                var result = new Dictionary<DateTime, int>();
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1);

                var allTasks = await _database.Table<TaskItem>().ToListAsync();

                var tasksInMonth = allTasks
                    .Where(t => t.DueDate >= startDate &&
                               t.DueDate < endDate &&
                               !t.IsDeleted)
                    .ToList();

                foreach (var task in tasksInMonth)
                {
                    var dateKey = task.DueDate.Date;
                    if (result.ContainsKey(dateKey))
                    {
                        result[dateKey]++;
                    }
                    else
                    {
                        result[dateKey] = 1;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTaskCountsForMonthAsync: {ex.Message}");
                return new Dictionary<DateTime, int>();
            }
        }
    }
}