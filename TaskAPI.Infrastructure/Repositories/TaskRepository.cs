using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Entities.Interfaces;
using TaskAPI.Infrastructure.Data;

namespace TaskAPI.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;
        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }
        public List<Entities.Entity.Task> GetAll() => _context.Tasks.ToList(); // bu metod, veritabanındaki tüm TaskAPI.Entities.Models.Entity.Task nesnelerini döndürür.
        public void Add(Entities.Entity.Task task) //bu metod, TaskAPI.Entities.Models.Entity.Task nesnesini alır ve veritabanına ekler. SaveChanges() metodu ile değişiklikleri kaydeder.
        {
            _context.Tasks.Add(task);
            _context.SaveChanges();
        }
        public Entities.Entity.Task Get(int id) //bu metod, verilen id'ye sahip TaskAPI.Entities.Models.Entity.Task nesnesini döndürür. Eğer nesne bulunamazsa null döner.
        {
            return _context.Tasks.FirstOrDefault(t => t.Id == id);
        }
        public void Update(Entities.Entity.Task task) //bu metod, verilen TaskAPI.Entities.Models.Entity.Task nesnesini günceller ve SaveChanges() metodu ile değişiklikleri kaydeder.
        {
            _context.Tasks.Update(task);
            _context.SaveChanges();
        }
        public void Delete(Entities.Entity.Task task) //bu metod, verilen TaskAPI.Entities.Models.Entity.Task nesnesini siler ve SaveChanges() metodu ile değişiklikleri kaydeder.
        {
            _context.Tasks.Remove(task);
            _context.SaveChanges();
        }
    }
}
