using SQLite;
using TaskManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskManager.Services
{
    public class DatabaseService : IDatabaseService
    {
        private SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            InitializeDatabase();
        }

        private async void InitializeDatabase()
        {
            if (_database == null)
            {
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "tasks.db3");
                _database = new SQLiteAsyncConnection(dbPath);
                await _database.CreateTableAsync<TaskItem>();
            }
        }

        public async Task InitializeDatabaseAsync()
        {
            if (_database != null)
                return;

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "tasks.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<TaskItem>();
        }

        public async Task<List<TaskItem>> GetTasksAsync()
        {
            await InitializeDatabaseAsync();
            return await _database.Table<TaskItem>().ToListAsync();
        }

        public async Task<List<TaskItem>> GetTasksByDateAsync(DateTime date)
        {
            await InitializeDatabaseAsync();
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            return await _database.Table<TaskItem>()
                .Where(t => t.DueDate >= startDate && t.DueDate < endDate)
                .ToListAsync();
        }

        public async Task<TaskItem> GetTaskAsync(int id)
        {
            await InitializeDatabaseAsync();
            return await _database.Table<TaskItem>()
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveTaskAsync(TaskItem task)
        {
            await InitializeDatabaseAsync();

            if (task.Id != 0)
            {
                return await _database.UpdateAsync(task);
            }
            else
            {
                return await _database.InsertAsync(task);
            }
        }

        public async Task<int> DeleteTaskAsync(TaskItem task)
        {
            await InitializeDatabaseAsync();
            return await _database.DeleteAsync(task);
        }

        public async Task<int> GetTaskCountForDateAsync(DateTime date)
        {
            await InitializeDatabaseAsync();
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            return await _database.Table<TaskItem>()
                .Where(t => t.DueDate >= startDate && t.DueDate < endDate)
                .CountAsync();
        }

        public async Task<Dictionary<DateTime, int>> GetTaskCountsForMonthAsync(int year, int month)
        {
            await InitializeDatabaseAsync();
            var result = new Dictionary<DateTime, int>();

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var tasks = await _database.Table<TaskItem>()
                .Where(t => t.DueDate >= startDate && t.DueDate < endDate)
                .ToListAsync();

            // Группируем задачи по датам
            var groupedTasks = tasks.GroupBy(t => t.DueDate.Date);

            foreach (var group in groupedTasks)
            {
                result[group.Key] = group.Count();
            }

            return result;
        }
    }
}