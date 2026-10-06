using System;
using System.Collections.Generic;
using System.Text;

namespace TaskAPI.Domain.Entity
{
    public class Task
    {
        public Task(string title, string description, DateTime createdAt, DateTime dueDate)
        {
            Title = title;
            Description = description;
            CreatedAt = createdAt;
            DueDate = dueDate;
        }
        private Task() { } // for EF Core nullability

        public int Id { get;  set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTimeOffset DeletedAt { get; set; }
    }
}
