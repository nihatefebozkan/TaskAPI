using System;
using System.Collections.Generic;
using System.Text;

namespace TaskAPI.Entities.Entity
{
    public class Task
    {
        public Task(string title, string description, DateTime createdAt, DateTime dueDate)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Başlık Boş Olamaz.");
            }
            if (title.Length > 100)
            {
                throw new ArgumentException("Başlık 100 karakterden uzun olamaz.");
            }
            if (dueDate < createdAt)
            {
                throw new ArgumentException("Bitiş tarihi oluşturma tarihinden önce olamaz.");
            }
            Title = title;
            Description = description;
            CreatedAt = createdAt;
            DueDate = dueDate;
        }

        public bool isOverdue()
        {
            return DateTime.Now > DueDate; //tarih kontrolü yapar ve true veya false döndürür
        }

        public int Id { get;  set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }

    }
}
