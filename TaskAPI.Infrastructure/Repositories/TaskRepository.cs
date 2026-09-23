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
        public async Task<List<Entities.Entity.Task>> GetAllAsync() => await _context.Tasks.ToListAsync(); // bu metod, veritabanındaki tüm TaskAPI.Entities.Models.Entity.Task nesnelerini döndürür.
        public async Task<Entities.Entity.Task> AddAsync(Entities.Entity.Task task) //bu metod, TaskAPI.Entities.Models.Entity.Task nesnesini alır ve veritabanına ekler. SaveChanges() metodu ile değişiklikleri kaydeder.
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }
        public async Task<Entities.Entity.Task> GetAsync(int id) //bu metod, verilen id'ye sahip TaskAPI.Entities.Models.Entity.Task nesnesini döndürür. Eğer nesne bulunamazsa null döner.
        {
            return await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        }
        public async Task<Entities.Entity.Task> UpdateAsync(Entities.Entity.Task task) //bu metod, verilen TaskAPI.Entities.Models.Entity.Task nesnesini günceller ve SaveChanges() metodu ile değişiklikleri kaydeder.
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return task;
        }
        public async Task<Entities.Entity.Task> DeleteAsync(Entities.Entity.Task task) //bu metod, verilen TaskAPI.Entities.Models.Entity.Task nesnesini siler ve SaveChanges() metodu ile değişiklikleri kaydeder.
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return task;
        }
    }
}
