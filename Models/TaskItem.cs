using System;
using SQLite;

namespace TaskManager.Models
{
    [Table("Tasks")]
    public class TaskItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Title { get; set; }

        public string Description { get; set; }

        [NotNull]
        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public TaskPriority Priority { get; set; }

        public TaskStatus Status { get; set; }

        public bool IsDeleted { get; set; }

        [Ignore]
        public bool IsSelected { get; set; }

        public TaskItem()
        {
            CreatedAt = DateTime.Now;
            Status = TaskStatus.New;
            Priority = TaskPriority.Medium;
            DueDate = DateTime.Now.Date.AddHours(9);
        }
    }
}